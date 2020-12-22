using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IArtistProvider            mArtistProvider;
        private readonly IClientState               mClientState;
        private readonly IHostInformationProvider   mHostInformationProvider;
        private readonly IPrefixedNameHandler       mPrefixedNameHandler;
        private readonly List<UiArtist>             mCompleteArtistList;
        private readonly IPreferences               mPreferences;
        private bool                                mLibraryOpen;
        private SortTypes                           mSortOrder;
        private string                              mFilterText;
        private PlayingState                        mPlayingState;
        private IDisposable                         mLibraryStatusSubscription;
        private IDisposable                         mPlayingStateSubscription;
        private UiArtist                            mSelectedArtist;

        private ObservableCollectionExtended<UiArtist>  mArtistList;

        public  DelegateCommand                     SortByName { get; }
        public  DelegateCommand                     SortByUnprefixedName { get; }
        public  DelegateCommand                     SortByRating { get; }

        public ArtistListViewModel( IArtistProvider artistProvider, IHostInformationProvider hostInformationProvider, IClientState clientState,
                                    IPrefixedNameHandler prefixedNameHandler, IPreferences preferences ) {
            mArtistProvider = artistProvider;
            mClientState = clientState;
            mHostInformationProvider = hostInformationProvider;
            mPrefixedNameHandler = prefixedNameHandler;
            mPreferences = preferences;

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
            mLibraryStatusSubscription = mHostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
            mPlayingStateSubscription = mClientState.CurrentlyPlaying.Subscribe( OnPlaying );
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

        public UiArtist SelectedArtist {
            get => mSelectedArtist;
            set => SetProperty( ref mSelectedArtist, value, OnArtistSelected );
        }

        private void OnArtistSelected() {
            if( mSelectedArtist != null ) {
                mClientState.SetCurrentArtist( mSelectedArtist.Artist );

                Shell.Current.GoToAsync( "albumList" );
            }
        }

        public string FilterText {
            get => mFilterText;
            set => SetProperty( ref mFilterText, value, OnFilterChanged );
        }

        private void OnFilterChanged() {
            RefreshArtistList();
        }

        private async void LoadArtistList() {
            mCompleteArtistList.Clear();

            if( mLibraryOpen ) {
                var list = await mArtistProvider.GetArtistList();

                if( list?.Success == true ) {
                    mCompleteArtistList.AddRange( SortArtists( list.ArtistList.Select( CreateArtist )));
                }
            }

            RefreshArtistList();
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

                mArtistList.AddRange( from artist in mCompleteArtistList where FilterArtist( artist ) select artist );
            }
        }

        private bool FilterArtist( UiArtist artist ) {
            var retValue = true;

            if(!String.IsNullOrWhiteSpace( FilterText )) {
                retValue = artist.ArtistName.ToLower().Contains( FilterText.ToLower());
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
