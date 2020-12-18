using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private TagInfo                             mCurrentTag;
        private IDisposable                         mLibraryStatusSubscription;

        public  ObservableCollection<TagInfo>           TagList { get; }
        public  ObservableCollection<UiTagAssociation>  TaggedItemsList { get; }

        public TagListViewModel( ITagInformationProvider tagProvider, IHostInformationProvider hostInformationProvider, IQueuePlayProvider queuePlayProvider ) {
            mTagProvider = tagProvider;
            mPlayProvider = queuePlayProvider;

            TagList = new ObservableCollection<TagInfo>();
            TaggedItemsList = new ObservableCollection<UiTagAssociation>();

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
                    tagList.TagList.OrderBy( t => t.TagName ).ForEach( t => TagList.Add( t ));
                }
            }
        }

        private async void LoadTagAssociations() {
            TaggedItemsList.Clear();

            if(( mLibraryOpen ) &&
               ( mCurrentTag != null )) {
                var associations = await mTagProvider.GetAssociations( mCurrentTag );

                if( associations.Success ) {
                    associations.TagAssociations.OrderBy( a => a.TrackName ).ForEach( a => TaggedItemsList.Add( new UiTagAssociation( a, OnPlay )));
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
