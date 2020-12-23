using System;
using System.Linq;
using DynamicData.Binding;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Prism.Mvvm;
using Xamarin.Forms.Internals;

namespace Noise.RemoteClient.ViewModels {
    class TagListViewModel : BindableBase, IDisposable {
        private readonly ITagInformationProvider    mTagProvider;
        private readonly IQueuePlayProvider         mPlayProvider;
        private bool                                mLibraryOpen;
        private bool                                mIsBusy;
        private PlayingState                        mPlayingState;
        private TagInfo                             mCurrentTag;
        private IDisposable                         mLibraryStatusSubscription;
        private IDisposable                         mPlayingStateSubscription;

        public  ObservableCollectionExtended<TagInfo>           TagList { get; }
        public  ObservableCollectionExtended<UiTagAssociation>  TaggedItemsList { get; }

        public TagListViewModel( ITagInformationProvider tagProvider, IHostInformationProvider hostInformationProvider, IQueuePlayProvider queuePlayProvider,
                                 IClientState clientState ) {
            mTagProvider = tagProvider;
            mPlayProvider = queuePlayProvider;

            TagList = new ObservableCollectionExtended<TagInfo>();
            TaggedItemsList = new ObservableCollectionExtended<UiTagAssociation>();

            mPlayingStateSubscription = clientState.CurrentlyPlaying.Subscribe( OnPlaying );
            mLibraryStatusSubscription = hostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
        }

        private void OnPlaying( PlayingState state ) {
            mPlayingState = state;

            TaggedItemsList.ForEach( i => i.SetIsPlaying( mPlayingState ));
        }

        public TagInfo CurrentTag {
            get => mCurrentTag;
            set => SetProperty( ref mCurrentTag, value, OnTagChanged );
        }

        public bool IsBusy {
            get => mIsBusy;
            set => SetProperty( ref mIsBusy, value );
        }

        private void OnLibraryStatus( LibraryStatus status ) {
            mLibraryOpen = status?.LibraryOpen == true;

            LoadTagList();
        }

        private void OnTagChanged() {
            LoadTagAssociations();
        }

        private async void LoadTagList() {
            TagList.Clear();
            IsBusy = true;

            if( mLibraryOpen ) {
                var tagList = await mTagProvider.GetUserTags();

                if( tagList.Success ) {
                    TagList.AddRange( from tag in tagList.TagList orderby tag.TagName select tag );
                }
            }

            IsBusy = false;
        }

        private async void LoadTagAssociations() {
            TaggedItemsList.Clear();
            IsBusy = true;

            if(( mLibraryOpen ) &&
               ( mCurrentTag != null )) {
                var associations = await mTagProvider.GetAssociations( mCurrentTag );

                if( associations.Success ) {
                    TaggedItemsList.AddRange( from association in associations.TagAssociations 
                                              orderby association.TrackName 
                                              select new UiTagAssociation( association, OnPlay, mPlayingState ));
                }
            }

            IsBusy = false;
        }

        private TrackInfo CreateTrack( TagAssociationInfo fromTag ) {
            return new TrackInfo {
                TrackId = fromTag.TrackId, ArtistId = fromTag.ArtistId, AlbumId = fromTag.AlbumId,
                TrackName = fromTag.TrackName, ArtistName = fromTag.ArtistName, AlbumName = fromTag.AlbumName, VolumeName = fromTag.VolumeName,
                TrackNumber = fromTag.TrackNumber, Duration = fromTag.Duration, IsFavorite = fromTag.IsFavorite, Rating = fromTag.Rating
            };
        }

        private void OnPlay( TagAssociationInfo tag ) {
            mPlayProvider.Queue( CreateTrack( tag ));
        }

        public void Dispose() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;

            mPlayingStateSubscription?.Dispose();
            mPlayingStateSubscription = null;
        }
    }
}
