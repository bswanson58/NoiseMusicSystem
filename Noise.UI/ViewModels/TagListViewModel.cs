using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Behaviours;
using Noise.UI.Dto;
using Noise.UI.Logging;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
    internal class TagEditRequest : InteractionRequestData<TagEditDialogModel> {
        public TagEditRequest( TagEditDialogModel viewModel ) : base( viewModel ) { }
    }

    class TagListViewModel : AutomaticCommandBase, IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
        private readonly IEventAggregator                   mEventAggregator;
        private readonly ITagProvider                       mTagProvider;
        private readonly IUserTagManager                    mTagManager;
        private readonly IUiLog                             mLog;
        private readonly InteractionRequest<TagEditRequest> mTagAddRequest;
        private readonly InteractionRequest<TagEditRequest> mTagEditRequest;
        private TaskHandler<IEnumerable<UiTag>>             mTaskHandler;
        private UiTag                                       mCurrentTag;

        public BindableCollection<UiTag>    TagList { get; }
        public IInteractionRequest          TagAddRequest => mTagAddRequest;
        public IInteractionRequest          TagEditRequest => mTagEditRequest;

        public TagListViewModel( IUserTagManager tagManager, ITagProvider tagProvider, IDatabaseInfo databaseInfo,
                                 IEventAggregator eventAggregator, IUiLog log ) {
            mTagManager = tagManager;
            mTagProvider = tagProvider;
            mEventAggregator = eventAggregator;
            mLog = log;
            mTagAddRequest = new InteractionRequest<TagEditRequest>();
            mTagEditRequest = new InteractionRequest<TagEditRequest>();

            TagList = new BindableCollection<UiTag>();

            if( databaseInfo.IsOpen ) {
                LoadTags();
            }

            mEventAggregator.Subscribe( this );
        }

        public void Handle( Events.DatabaseOpened args ) {
            LoadTags();
        }

        public void Handle( Events.DatabaseClosing args ) {
            ClearTags();
        }

        public UiTag SelectedTag {
            get => mCurrentTag; 
            set {
                mCurrentTag = value;

                if( mCurrentTag != null ) {
                    mEventAggregator.PublishOnUIThread( new Events.TagFocusRequested( mCurrentTag.Tag ));
                }
            }
        }

        internal TaskHandler<IEnumerable<UiTag>> TaskHandler {
            get {
                if (mTaskHandler == null) {
                    Execute.OnUIThread(() => mTaskHandler = new TaskHandler<IEnumerable<UiTag>>());
                }

                return (mTaskHandler);
            }

            set => mTaskHandler = value;
        }

        private void LoadTags() {
            TaskHandler.StartTask( RetrieveTags, SetTags, exception => mLog.LogException( "Loading User Tags", exception ));
        }

        private IEnumerable<UiTag> RetrieveTags() {
            return( from tag in mTagManager.GetUserTagList() orderby tag.Name select new UiTag( tag, OnEditTag ));
        }

        private void SetTags( IEnumerable<UiTag> list ) {
            TagList.Clear();
            TagList.AddRange( list );
        }

        private void ClearTags() {
            TagList.Clear();
        }

        private void OnEditTag( UiTag tag ) {
            var dialogModel = new TagEditDialogModel( tag );

            mTagEditRequest.Raise( new TagEditRequest( dialogModel ), OnTagEdited );
        }

        private void OnTagEdited( TagEditRequest confirmation) {
            if(( confirmation.Confirmed ) &&
               ( confirmation.ViewModel.IsValid )) {
                var updater = mTagProvider.GetTagForUpdate( confirmation.ViewModel.Tag.Tag.DbId );

                if( updater.Item != null ) {
                    updater.Item.UpdateFrom( confirmation.ViewModel.Tag.Tag );

                    updater.Update();
                }

                LoadTags();
            }
        }

        public void Execute_AddTag() {
            var tag = new UiTag( new DbTag( eTagGroup.User, String.Empty ), null );
            var dialogModel = new TagEditDialogModel( tag );

            mTagAddRequest.Raise( new TagEditRequest( dialogModel ), OnTagAdded );
        }

        private void OnTagAdded( TagEditRequest confirmation) {
            if(( confirmation.Confirmed ) &&
               ( confirmation.ViewModel.IsValid )) {
                mTagProvider.AddTag( confirmation.ViewModel.Tag.Tag );

                LoadTags();
            }
        }
    }
}
