using System;
using System.Linq;
using DynamicData.Binding;
using Noise.RemoteClient.Dto;
using Noise.RemoteClient.Interfaces;
using Noise.RemoteServer.Protocol;
using Prism.Mvvm;

namespace Noise.RemoteClient.ViewModels {
    class TagListViewModel : BindableBase, IDisposable {
        private readonly ITagInformationProvider    mTagProvider;
        private readonly IQueuePlayProvider         mPlayProvider;
        private bool                                mLibraryOpen;
        private TagInfo                             mCurrentTag;
        private IDisposable                         mLibraryStatusSubscription;

        public  ObservableCollectionExtended<TagInfo>           TagList { get; }
        public  ObservableCollectionExtended<UiTagAssociation>  TaggedItemsList { get; }

        public TagListViewModel( ITagInformationProvider tagProvider, IHostInformationProvider hostInformationProvider, IQueuePlayProvider queuePlayProvider ) {
            mTagProvider = tagProvider;
            mPlayProvider = queuePlayProvider;

            TagList = new ObservableCollectionExtended<TagInfo>();
            TaggedItemsList = new ObservableCollectionExtended<UiTagAssociation>();

            mLibraryStatusSubscription = hostInformationProvider.LibraryStatus.Subscribe( OnLibraryStatus );
        }

        public TagInfo CurrentTag {
            get => mCurrentTag;
            set => SetProperty( ref mCurrentTag, value, OnTagChanged );
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

            if( mLibraryOpen ) {
                var tagList = await mTagProvider.GetUserTags();

                if( tagList.Success ) {
                    TagList.AddRange( from tag in tagList.TagList orderby tag.TagName select tag );
                }
            }
        }

        private async void LoadTagAssociations() {
            TaggedItemsList.Clear();

            if(( mLibraryOpen ) &&
               ( mCurrentTag != null )) {
                var associations = await mTagProvider.GetAssociations( mCurrentTag );

                if( associations.Success ) {
                    TaggedItemsList.AddRange( from association in associations.TagAssociations 
                                              orderby association.TrackName 
                                              select new UiTagAssociation( association, OnPlay ));
                }
            }
        }

        private void OnPlay( TagAssociationInfo tag ) {
            mPlayProvider.QueueTrack( tag.TrackId );
        }

        public void Dispose() {
            mLibraryStatusSubscription?.Dispose();
            mLibraryStatusSubscription = null;
        }
    }
}
