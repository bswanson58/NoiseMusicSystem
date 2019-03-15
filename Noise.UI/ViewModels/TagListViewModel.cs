using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Noise.UI.Logging;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
    class TagListViewModel : AutomaticCommandBase, IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
        private readonly ITagProvider           mTagProvider;
        private readonly IUiLog                 mLog;
        private TaskHandler<IEnumerable<UiTag>> mTaskHandler;

        public BindableCollection<UiTag>   TagList { get; }

        public TagListViewModel( ITagProvider tagProvider, IDatabaseInfo databaseInfo, IUiLog log ) {
            mTagProvider = tagProvider;
            mLog = log;

            TagList = new BindableCollection<UiTag>();

            if( databaseInfo.IsOpen ) {
                LoadTags();
            }
        }

        public void Handle(Events.DatabaseOpened args) {
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
            TaskHandler.StartTask( RetrieveTags, SetTags, exception => mLog.LogException("Loading Favorites", exception));
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

        public void Execute_AddTag() { }
    }
}
