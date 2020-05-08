using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Prism.Interactivity.InteractionRequest;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Behaviours;
using Noise.UI.Dto;
using Noise.UI.Interfaces;
using Noise.UI.Logging;
using Noise.UI.Support;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
    internal class TagEditRequest : InteractionRequestData<TagEditDialogModel> {
        public TagEditRequest( TagEditDialogModel viewModel ) : base( viewModel ) { }
    }

    class TagListViewModel : AutomaticCommandBase, IDisposable, IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
        private readonly IEventAggregator                   mEventAggregator;
        private readonly ITagProvider                       mTagProvider;
        private readonly IPlayCommand                       mPlayCommand;
        private readonly IUserTagManager                    mTagManager;
        private readonly IDialogService                     mDialogService;
        private readonly IUiLog                             mLog;
        private readonly IDataExchangeManager               mDataExchangeManager;
        private readonly InteractionRequest<TagEditRequest> mTagAddRequest;
        private readonly InteractionRequest<TagEditRequest> mTagEditRequest;
        private TaskHandler<IEnumerable<UiTag>>             mTaskHandler;
        private UiTag                                       mCurrentTag;
        private IDisposable                                 mSelectionStateSubscription;

        public BindableCollection<UiTag>    TagList { get; }
        public IInteractionRequest          TagAddRequest => mTagAddRequest;
        public IInteractionRequest          TagEditRequest => mTagEditRequest;

        public TagListViewModel( IUserTagManager tagManager, ITagProvider tagProvider, IDatabaseInfo databaseInfo, IPlayCommand playCommand, ISelectionState selectionState,
                                 IDataExchangeManager exchangeManager, IDialogService dialogService, IEventAggregator eventAggregator, IUiLog log ) {
            mTagManager = tagManager;
            mTagProvider = tagProvider;
            mPlayCommand = playCommand;
            mDataExchangeManager = exchangeManager;
            mDialogService = dialogService;
            mEventAggregator = eventAggregator;
            mLog = log;
            mTagAddRequest = new InteractionRequest<TagEditRequest>();
            mTagEditRequest = new InteractionRequest<TagEditRequest>();

            TagList = new BindableCollection<UiTag>();

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

            RaiseCanExecuteChangedEvent( "CanExecute_ExportTags" );
        }

        private void ClearTags() {
            TagList.Clear();

            RaiseCanExecuteChangedEvent( "CanExecute_ExportTags" );
        }

        private void OnPlayTag( UiTag tag ) {
            mPlayCommand.PlayRandomTaggedTracks( tag.Tag );
        }

        private void OnEditTag( UiTag tag ) {
            var dialogModel = new TagEditDialogModel( tag );

            mTagEditRequest.Raise( new TagEditRequest( dialogModel ), OnTagEdited );
        }

        private void OnTagEdited( TagEditRequest confirmation) {
            if( confirmation.Confirmed ) {
                if( confirmation.ViewModel.DeleteRequested ) {
                    mTagManager.DeleteTag( confirmation.ViewModel.Tag.Tag );
                }
                else {
                    if(confirmation.ViewModel.IsValid) {
                        var updater = mTagProvider.GetTagForUpdate(confirmation.ViewModel.Tag.Tag.DbId);

                        if (updater.Item != null) {
                            updater.Item.UpdateFrom(confirmation.ViewModel.Tag.Tag);

                            updater.Update();
                        }
                    }
                }

                LoadTags();
            }
        }

        public void Execute_AddTag() {
            var tag = new UiTag( new DbTag( eTagGroup.User, String.Empty ));
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

        public void Execute_ExportTags() {
            if( mDialogService.SaveFileDialog( "Export Tags", Constants.ExportFileExtension, "Export Tags|*" + Constants.ExportFileExtension, out var fileName ) == true ) {
                mDataExchangeManager.ExportUserTags( fileName );
            }
        }

        public bool CanExecute_ExportTags() {
            return( TagList.Count > 0 );
        }

        public void Execute_ImportTags() {
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
