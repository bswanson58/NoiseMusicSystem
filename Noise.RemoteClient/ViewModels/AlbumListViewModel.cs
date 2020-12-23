using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData.Binding;
using Noise.RemoteClient.Dialogs;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteClient.Support;
using Noise.RemoteServer.Protocol;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Noise.RemoteClient.ViewModels {
    class AlbumListViewModel : BindableBase, IDisposable {
        private readonly IClientState               mClientState;
        private readonly IAlbumProvider             mAlbumProvider;
        private readonly IQueuePlayProvider         mPlayProvider;
        private readonly IDialogService             mDialogService;
        private readonly IPrefixedNameHandler       mPrefixedNameHandler;
        private readonly IPreferences               mPreferences;
        private readonly List<UiAlbum>              mCompleteAlbumList;
        private readonly ArtistInfo                 mCurrentArtist;
        private SortTypes                           mSortOrder;
        private string                              mFilterText;
        private UiAlbum                             mSelectedAlbum;
        private PlayingState                        mPlayingState;
        private IDisposable                         mPlayingTrackSubscription;
        private  ObservableCollectionExtended<UiAlbum>  mAlbumList;

        public  string                                  ArtistName { get; private set; }
        public  Int32                                   AlbumCount { get; private set; }

        public  UiAlbum                                 PlayingAlbum { get; private set; }
        public  bool                                    HavePlayingAlbum => PlayingAlbum != null;
        public  DelegateCommand                         EditPlayingAlbumRatings { get; }

        public  DelegateCommand                         SelectPlayingAlbum { get; }
        public  DelegateCommand<UiAlbum>                SelectAlbum { get; }

        public  DelegateCommand                         SortByName { get; }
        public  DelegateCommand                         SortByUnprefixedName { get; }
        public  DelegateCommand                         SortByRating { get; }

        public  DelegateCommand<UiAlbum>                EditAlbumRatings { get; }

        public AlbumListViewModel( IAlbumProvider albumProvider, IQueuePlayProvider queuePlayProvider, IClientState clientState, IDialogService dialogService,
                                   IPrefixedNameHandler prefixedNameHandler, IPreferences preferences ) {
            mAlbumProvider = albumProvider;
            mPlayProvider = queuePlayProvider;
            mClientState = clientState;
            mDialogService = dialogService;
            mPreferences = preferences;
            mPrefixedNameHandler = prefixedNameHandler;

            SelectPlayingAlbum = new DelegateCommand( OnSelectPlayingAlbum );
            SelectAlbum = new DelegateCommand<UiAlbum>( OnSelectAlbum );

            EditPlayingAlbumRatings = new DelegateCommand( OnEditPlayingAlbumRatings );

            SortByName = new DelegateCommand( OnSortByName );
            SortByUnprefixedName = new DelegateCommand( OnSortByUnprefixedName );
            SortByRating = new DelegateCommand( OnSortByRating );

            EditAlbumRatings = new DelegateCommand<UiAlbum>( OnEditAlbumRatings );

            if(!Enum.TryParse( mPreferences.Get( PreferenceNames.AlbumListSorting, SortTypes.Rating.ToString()), out mSortOrder )) {
                mSortOrder = SortTypes.Rating;
            }

            mCurrentArtist = clientState.CurrentArtist;
            mCompleteAlbumList = new List<UiAlbum>();
        }

        public ObservableCollectionExtended<UiAlbum> AlbumList {
            get {
                if( mAlbumList == null ) {
                    mAlbumList = new ObservableCollectionExtended<UiAlbum>();

                    Initialize();
                }

                return mAlbumList;
            }
        }

        private void Initialize() {
            ArtistName = mCurrentArtist?.ArtistName;
            RaisePropertyChanged( nameof( ArtistName ));

            LoadAlbumList();

            mPlayingTrackSubscription = mClientState.CurrentlyPlaying.Subscribe( OnPlaying );
        }

        private void OnPlaying( PlayingState state ) {
            mPlayingState = state;

            SetPlayingAlbum();
            UpdatePlayStates();
        }

        private void SetPlayingAlbum() {
            PlayingAlbum = mCompleteAlbumList.FirstOrDefault( a => a.AlbumId.Equals( mPlayingState?.AlbumId ));

            RaisePropertyChanged( nameof( PlayingAlbum ));
            RaisePropertyChanged( nameof( HavePlayingAlbum ));
        }

        private void UpdatePlayStates() {
            mCompleteAlbumList.ForEach( a => a.SetIsPlaying( mPlayingState ));
            mAlbumList?.ForEach( a => a.SetIsPlaying( mPlayingState ));
        }

        public string FilterText {
            get => mFilterText;
            set => SetProperty( ref mFilterText, value, OnFilterChanged );
        }

        private void OnFilterChanged() {
            RefreshAlbumList();
        }

        public UiAlbum SelectedAlbum {
            get => mSelectedAlbum;
            set => SetProperty( ref mSelectedAlbum, value, OnAlbumSelected );
        }

        private void OnSelectAlbum( UiAlbum album ) {
            mSelectedAlbum = album;

            OnAlbumSelected();
        }

        private void OnAlbumSelected() {
            if( mSelectedAlbum?.Album != null ) {
                mClientState.SetCurrentAlbum( mSelectedAlbum.Album );

                Shell.Current.GoToAsync( "trackList" );

                SelectedAlbum = null;
            }
        }

        private void OnSelectPlayingAlbum() {
            if( PlayingAlbum != null ) {
                mClientState.SetCurrentAlbum( PlayingAlbum.Album );

                Shell.Current.GoToAsync( "trackList" );
            }
        }

        private async void LoadAlbumList() {
            mCompleteAlbumList.Clear();

            if( mCurrentArtist != null ) {
                var list = await mAlbumProvider.GetAlbumList( mCurrentArtist.DbId );

                if( list?.Success == true ) {
                    mCompleteAlbumList.AddRange( SortAlbums( list.AlbumList.Select( CreateAlbum )));
                }
            }

            SetPlayingAlbum();
            RefreshAlbumList();
        }

        private UiAlbum CreateAlbum( AlbumInfo fromAlbum ) {
            var retValue = new UiAlbum( fromAlbum, OnAlbumPlay );

            retValue.SetDisplayName( mPrefixedNameHandler.FormatPrefixedName( retValue.AlbumName ));
            retValue.SetSortName( mPrefixedNameHandler.FormatSortingName( retValue.AlbumName ));
            retValue.SetIsPlaying( mPlayingState );

            return retValue;
        }

        private IEnumerable<UiAlbum> SortAlbums( IEnumerable<UiAlbum> list ) {
            var retValue = list;

            switch( mSortOrder ) {

                case SortTypes.Name:
                    retValue = retValue.OrderBy( a => a.AlbumName );
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

        private void RefreshAlbumList() {
            if( mAlbumList != null ) {
                mAlbumList.Clear();

                mAlbumList.AddRange( from a in mCompleteAlbumList where FilterAlbum( a ) select a );

                AlbumCount = AlbumList.Count;
                RaisePropertyChanged( nameof( AlbumCount ));
            }
        }

        private bool FilterAlbum( UiAlbum album ) {
            var retValue = true;

            if(!String.IsNullOrWhiteSpace( FilterText )) {
                retValue = album.AlbumName.ToLower().Contains( FilterText.ToLower());
            }

            return retValue;
        }

        private void OnAlbumPlay( UiAlbum album ) {
            mPlayProvider.Queue( album.Album );
        }

        private void OnEditPlayingAlbumRatings() {
            if( PlayingAlbum != null ) {
                OnEditAlbumRatings( PlayingAlbum );
            }
        }

        private void OnEditAlbumRatings( UiAlbum forAlbum ) {
            var parameters = new DialogParameters {{ EditAlbumRatingsViewModel.cAlbumParameter, forAlbum.Album }};

            mDialogService.ShowDialog( nameof( EditAlbumRatingsView ), parameters, async result => {
                var accepted = result.Parameters.GetValue<bool>( EditTrackRatingsViewModel.cDialogAccepted );

                if( accepted ) {
                    var album = parameters.GetValue<AlbumInfo>( EditAlbumRatingsViewModel.cAlbumParameter );

                    if( album != null ) {
                        await mAlbumProvider.UpdateAlbumRatings( album );

                        var uiAlbum = mCompleteAlbumList.FirstOrDefault( a => a.Album.AlbumId.Equals( album.AlbumId ));
                        uiAlbum?.UpdateRatings( album );
                    }
                }
            });
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
            mPreferences.Set( PreferenceNames.AlbumListSorting, mSortOrder.ToString());

            LoadAlbumList();
        }

        public void Dispose() {
            mPlayingTrackSubscription?.Dispose();
            mPlayingTrackSubscription = null;
        }
    }
}
