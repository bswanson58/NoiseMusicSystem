using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using Noise.UI.Views;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits;
using ReusableBits.ExtensionClasses.MoreLinq;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Ui.Platform;

namespace Noise.UI.ViewModels {
    class TagListViewModel : PropertyChangeBase, IDisposable, IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
        private readonly IEventAggregator           mEventAggregator;
        private readonly ITagProvider               mTagProvider;
        private readonly IPlayCommand               mPlayCommand;
        private readonly IUserTagManager            mTagManager;
        private readonly IDialogService             mDialogService;
        private readonly IPlatformDialogService     mPlatformDialogs;
        private readonly IUiLog                     mLog;
        private readonly IDataExchangeManager       mDataExchangeManager;
        private TaskHandler<IEnumerable<UiTag>>     mTaskHandler;
        private UiTag                               mCurrentTag;
        private IDisposable                         mSelectionStateSubscription;

        public  BindableCollection<UiTag>           TagList { get; }
        public  DelegateCommand                     AddTag { get; }
        public  DelegateCommand                     ExportTags { get; }
        public  DelegateCommand                     ImportTags { get; }

        public TagListViewModel( IUserTagManager tagManager, ITagProvider tagProvider, IDatabaseInfo databaseInfo, IPlayCommand playCommand, ISelectionState selectionState,
                                 IDataExchangeManager exchangeManager, IDialogService dialogService, IPlatformDialogService platformDialogService, IEventAggregator eventAggregator, IUiLog log ) {
            mTagManager = tagManager;
            mTagProvider = tagProvider;
            mPlayCommand = playCommand;
            mDataExchangeManager = exchangeManager;
            mDialogService = dialogService;
            mPlatformDialogs = platformDialogService;
            mEventAggregator = eventAggregator;
            mLog = log;

            TagList = new BindableCollection<UiTag>();
            AddTag = new DelegateCommand( OnAddTag );
            ExportTags = new DelegateCommand( OnExportTags, CanExportTags );
            ImportTags = new DelegateCommand( OnImportTags );

            if( databaseInfo.IsOpen ) {
                LoadTags();
            }

            mSelectionStateSubscription = selectionState.PlayingTrackChanged.Subscribe( OnPlayingTrackChanged );
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

        private void OnPlayingTrackChanged( PlayingItem item ) {
            if( item?.Track != null ) {
                var playingTags = mTagManager.GetAssociatedTags( item.Track ).ToList();

                TagList.ForEach( tag => tag.SetIsPlaying( playingTags.Any( t => t.DbId.Equals( tag.Tag.DbId ))));
            }
            else {
                TagList.ForEach( tag => tag.SetIsPlaying( false ));
            }
        }

        internal TaskHandler<IEnumerable<UiTag>> TaskHandler {
            get {
                if( mTaskHandler == null ) {
                    Execute.OnUIThread(() => mTaskHandler = new TaskHandler<IEnumerable<UiTag>>());
                }

                return mTaskHandler;
            }

            set => mTaskHandler = value;
        }

        private void LoadTags() {
            TaskHandler.StartTask( RetrieveTags, SetTags, exception => mLog.LogException( "Loading User Tags", exception ));
        }

        private IEnumerable<UiTag> RetrieveTags() {
            return( from tag in mTagManager.GetUserTagList() orderby tag.Name select new UiTag( tag, OnEditTag, OnPlayTag ));
        }

        private void SetTags( IEnumerable<UiTag> list ) {
            TagList.Clear();
            TagList.AddRange( list );

            ExportTags.RaiseCanExecuteChanged();
        }

        private void ClearTags() {
            TagList.Clear();

            ExportTags.RaiseCanExecuteChanged();
        }

        private void OnPlayTag( UiTag tag ) {
            mPlayCommand.PlayRandomTaggedTracks( tag.Tag );
        }

        private void OnEditTag( UiTag tag ) {
            var parameters = new DialogParameters{{ TagEditDialogModel.cTagParameter, tag } };

            mDialogService.ShowDialog( nameof( TagEditDialog ), parameters, result => {
                if( result.Result == ButtonResult.OK ) {
                    var deleteRequested = result.Parameters.GetValue<bool>( TagEditDialogModel.cDeleteRequested );
                    var editedTag = result.Parameters.GetValue<UiTag>( TagEditDialogModel.cTagParameter );

                    if( editedTag != null ) {
                        if( deleteRequested ) {
                            mTagManager.DeleteTag( editedTag.Tag );
                        }
                        else {
                            var updater = mTagProvider.GetTagForUpdate( editedTag.Tag.DbId );

                            if( updater.Item != null ) {
                                updater.Item.UpdateFrom( editedTag.Tag );

                                updater.Update();
                            }
                        }
                    }

                    LoadTags();
                }
            });
        }

        private void OnAddTag() {
            var tag = new UiTag( new DbTag( eTagGroup.User, String.Empty ));
            var parameters = new DialogParameters{{ TagEditDialogModel.cTagParameter, tag }};

            mDialogService.ShowDialog( nameof( TagAddDialog ), parameters, result => {
                if( result.Result == ButtonResult.OK ) {
                    var editedTag = result.Parameters.GetValue<UiTag>( TagEditDialogModel.cTagParameter );

                    if( editedTag != null ) {
                        mTagProvider.AddTag( editedTag.Tag );

                        LoadTags();
                    }
                }
            });
        }

        private void OnExportTags() {
            if( mPlatformDialogs.SaveFileDialog( "Export Tags", Constants.ExportFileExtension, "Export Tags|*" + Constants.ExportFileExtension, out var fileName ) == true ) {
                mDataExchangeManager.ExportUserTags( fileName );
            }
        }

        private bool CanExportTags() {
            return( TagList.Count > 0 );
        }

        private void OnImportTags() {
            GlobalCommands.ImportUserTags.Execute( null );

            LoadTags();
        }

        public void Dispose() {
            mSelectionStateSubscription?.Dispose();
            mSelectionStateSubscription = null;

            mEventAggregator.Unsubscribe( this );
        }
    }
}
