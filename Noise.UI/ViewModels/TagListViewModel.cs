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
        private readonly ITagProvider                       mTagProvider;
        private readonly IUiLog                             mLog;
        private readonly InteractionRequest<TagEditRequest> mTagEditRequest;
        private TaskHandler<IEnumerable<UiTag>>             mTaskHandler;

        public BindableCollection<UiTag>    TagList { get; }
        public IInteractionRequest          TagEditRequest => mTagEditRequest;

        public TagListViewModel( ITagProvider tagProvider, IDatabaseInfo databaseInfo, IUiLog log ) {
            mTagProvider = tagProvider;
            mLog = log;
            mTagEditRequest = new InteractionRequest<TagEditRequest>();

            TagList = new BindableCollection<UiTag>();

            if( databaseInfo.IsOpen ) {
                LoadTags();
            }
        }

        public void Handle( Events.DatabaseOpened args ) {
            LoadTags();
        }

        public void Handle( Events.DatabaseClosing args ) {
            ClearTags();
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
            TaskHandler.StartTask( RetrieveTags, SetTags, exception => mLog.LogException( "Loading Favorites", exception ));
        }

        private IEnumerable<UiTag> RetrieveTags() {
            var retValue = new List<UiTag>();

            using( var tagList = mTagProvider.GetTagList( eTagGroup.User )) {
                retValue.AddRange( from tag in tagList.List select new UiTag( tag ));
            }

            return retValue;
        }

        private void SetTags( IEnumerable<UiTag> list ) {
            TagList.Clear();
            TagList.AddRange( list );
        }

        private void ClearTags() {
            TagList.Clear();
        }

        public void Execute_AddTag() {
            var tag = new UiTag( new DbTag( eTagGroup.User, String.Empty ));
            var dialogModel = new TagEditDialogModel( tag );

            mTagEditRequest.Raise( new TagEditRequest( dialogModel ), OnTagAdded );
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
