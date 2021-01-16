using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using DynamicData.Binding;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Support;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Noise.RemoteClient.ViewModels {
    class ArtistListViewModel : BindableBase, IDisposable {
        private const string                        cGenre = "Genre:";

        private readonly IArtistProvider            mArtistProvider;
        private readonly IClientState               mClientState;
        private readonly IQueueListener             mQueueListener;
        private readonly IHostInformationProvider   mHostInformationProvider;
        private readonly IPrefixedNameHandler       mPrefixedNameHandler;
        private readonly List<UiArtist>             mCompleteArtistList;
        private readonly IPreferences               mPreferences;
        private bool                                mLibraryOpen;
        private bool                                mIsBusy;
        private SortTypes                           mSortOrder;
        private string                              mFilterText;
        private PlayingState                        mPlayingState;
        private IDisposable                         mLibraryStatusSubscription;
        private IDisposable                         mPlayingStateSubscription;
        private UiArtist                            mSelectedArtist;

        private ObservableCollectionExtended<UiArtist>  mArtistList;

        public  UiArtist                            PlayingArtist { get; private set; }
        public  bool                                HavePlayingArtist => PlayingArtist != null;
        public  DelegateCommand                     SelectPlayingArtist { get; }

        public  DelegateCommand<UiArtist>           SelectArtist { get; }
        public  DelegateCommand<UiArtist>           SelectGenre { get; }

        public  DelegateCommand                     SortByName { get; }
        public  DelegateCommand                     SortByUnprefixedName { get; }
        public  DelegateCommand                     SortByRating { get; }

        public ArtistListViewModel( IArtistProvider artistProvider, IHostInformationProvider hostInformationProvider, IClientState clientState,
                                    IQueueListener queueListener, IPrefixedNameHandler prefixedNameHandler, IPreferences preferences ) {
            mArtistProvider = artistProvider;
            mClientState = clientState;
            mQueueListener = queueListener;
            mHostInformationProvider = hostInformationProvider;
            mPrefixedNameHandler = prefixedNameHandler;
            mPreferences = preferences;

            SelectPlayingArtist = new DelegateCommand( OnSelectPlayingArtist );
            SelectArtist = new DelegateCommand<UiArtist>( OnSelectArtist );
            SelectGenre = new DelegateCommand<UiArtist>( OnSelectGenre );

            SortByName = new DelegateCommand( OnSortByName );
            SortByUnprefixedName = new DelegateCommand( OnSortByUnprefixedName );
            SortByRating = new DelegateCommand( OnSortByRating );

            if(!Enum.TryParse( mPreferences.Get( PreferenceNames.ArtistListSorting, SortTypes.UnprefixedName.ToString()), out mSortOrder )) {
                mSortOrder = SortTypes.UnprefixedName;
            }

            mCompleteArtistList = new List<UiArtist>();

            mFilterText = String.Empty;
        }

        private void Initialize() {
            mLibraryStatusSubscription = mHostInformationProvider.LibraryStatus.ObserveOn( SynchronizationContext.Current ).Subscribe( OnLibraryStatus );
        }

        private void OnLibraryStatus( LibraryStatus status ) {
            mLibraryOpen = status?.LibraryOpen == true;

            if( mLibraryOpen ) {
                LoadArtistList();
            }
            else {
                mCompleteArtistList.Clear();

                RefreshArtistList();
            }
        }

        private void OnPlaying( PlayingState state ) {
            mPlayingState = state;

            UpdatePlayingState();
        }

        private void UpdatePlayingState() {
            mCompleteArtistList.ForEach( a => a.SetIsPlaying( mPlayingState ));
            mArtistList?.ForEach( a => a.SetIsPlaying( mPlayingState ));
            PlayingArtist = ArtistList.FirstOrDefault( a => a.ArtistId.Equals( mPlayingState?.ArtistId ));

            RaisePropertyChanged( nameof( PlayingArtist ));
            RaisePropertyChanged( nameof( HavePlayingArtist ));
        }

        public ObservableCollectionExtended<UiArtist> ArtistList {
            get {
                if( mArtistList == null ) {
                    mArtistList = new ObservableCollectionExtended<UiArtist>();

                    Initialize();
                }

                return mArtistList;
            }
        }

        private void OnSelectArtist( UiArtist artist ) {
            mSelectedArtist = artist;

            OnArtistSelected();
        }

        public UiArtist SelectedArtist {
            get => mSelectedArtist;
            set => SetProperty( ref mSelectedArtist, value, OnArtistSelected );
        }

        private void OnArtistSelected() {
            if( mSelectedArtist != null ) {
                mClientState.SetCurrentArtist( mSelectedArtist.Artist );

                Shell.Current.GoToAsync( RouteNames.AlbumList );

                SelectedArtist = null;
            }
        }

        private void OnSelectPlayingArtist() {
            if( PlayingArtist != null ) {
                mClientState.SetCurrentArtist( PlayingArtist.Artist );

                Shell.Current.GoToAsync( RouteNames.AlbumList );
            }
        }

        public string FilterText {
            get => mFilterText;
            set => SetProperty( ref mFilterText, value, OnFilterChanged );
        }

        private void OnFilterChanged() {
            RefreshArtistList();
        }

        private void OnSelectGenre( UiArtist artist ) {
            FilterText = $"{cGenre} {artist.Genre}";
        }

        public bool IsBusy {
            get => mIsBusy;
            set => SetProperty( ref mIsBusy, value );
        }

        private async void LoadArtistList() {
            IsBusy = true;

            mPlayingStateSubscription?.Dispose();
            mPlayingStateSubscription = null;

            mCompleteArtistList.Clear();

            if( mLibraryOpen ) {
                var list = await mArtistProvider.GetArtistList();

                if( list?.Success == true ) {
                    mCompleteArtistList.AddRange( SortArtists( list.ArtistList.Select( CreateArtist )));
                }
            }

            RefreshArtistList();
            UpdatePlayingState();
            IsBusy = false;

            mPlayingStateSubscription = mQueueListener.CurrentlyPlaying.Subscribe( OnPlaying );
        }

        private IEnumerable<UiArtist> SortArtists( IEnumerable<UiArtist> list ) {
            var retValue = list;

            switch( mSortOrder ) {

                case SortTypes.Name:
                    retValue = retValue.OrderBy( a => a.ArtistName );
                    break;

                case SortTypes.UnprefixedName:
                    retValue = retValue.OrderBy( a => a.SortName );
                    break;

                case SortTypes.Rating:
                    retValue = retValue.OrderByDescending( a => a.SortRating ).ThenBy( a => a.SortName );
                    break;
            }

            return retValue;
        }

        private UiArtist CreateArtist( ArtistInfo fromArtist ) {
            var retValue = new UiArtist( fromArtist );

            retValue.SetDisplayName( mPrefixedNameHandler.FormatPrefixedName( retValue.ArtistName ));
            retValue.SetSortName( mPrefixedNameHandler.FormatSortingName( retValue.ArtistName ));
            retValue.SetIsPlaying( mPlayingState );

            return retValue;
        }

        private void RefreshArtistList() {
            if( mArtistList != null ) {
                mArtistList.Clear();

                if( FilterText.StartsWith( cGenre )) {
                    mArtistList.AddRange( from artist in mCompleteArtistList where FilterGenre( artist ) select artist );
                }
                else {
                    mArtistList.AddRange( from artist in mCompleteArtistList where FilterArtist( artist ) select artist );
                }
            }
        }

        private bool FilterArtist( UiArtist artist ) {
            var retValue = true;

            if(!String.IsNullOrWhiteSpace( FilterText )) {
                retValue = artist.ArtistName.ToLower().Contains( FilterText.ToLower());
            }

            return retValue;
        }

        private bool FilterGenre( UiArtist artist ) {
            var retValue = true;

            if(!String.IsNullOrWhiteSpace( FilterText )) {
                var genre = FilterText.Replace( cGenre, String.Empty ).ToLower().Trim();

                retValue = artist.Genre.ToLower().Equals( genre );
            }

            return retValue;
        }

        private void OnSortByName() {
            SetSortTo( SortTypes.Name );
        }

        private void OnSortByUnprefixedName() {
            SetSortTo( SortTypes.UnprefixedName );
        }

        private void OnSortByRating() {
            SetSortTo( SortTypes.Rating );
        }

        private void SetSortTo( SortTypes sort ) {
            mSortOrder = sort;
            mPreferences.Set( PreferenceNames.ArtistListSorting, mSortOrder.ToString());

            LoadArtistList();
        }

        public void Dispose() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;

            mPlayingStateSubscription?.Dispose();
            mPlayingStateSubscription = null;
        }
    }
}
