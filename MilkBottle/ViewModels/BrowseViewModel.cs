using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Data;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Types;
using MilkBottle.Views;
using MoreLinq;
using Prism;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class BrowseViewModel : PropertyChangeBase, IActiveAware, IDisposable,
                            IHandle<Events.ModeChanged>, IHandle<Events.InitializationComplete> {
        private const string					    cDisplayActivePreset = "_displayActivePreset";
        private const string					    cHideActivePreset = "_normal";

        private readonly IPresetListProvider        mListProvider;
        private readonly IPresetProvider            mPresetProvider;
        private readonly IPlatformLog               mLog;
        private readonly IPresetController          mPresetController;
        private readonly IPresetImageHandler        mImageHandler;
        private readonly IStateManager              mStateManager;
        private readonly IEventAggregator           mEventAggregator;
        private readonly IDialogService             mDialogService;
        private Subject<UiVisualPreset>             mPresetDisplaySubject;
        private IDisposable                         mPresetDisplaySubscription;
        private ICollectionView                     mLibrariesView;
        private PresetList                          mCurrentLibrary;
        private TaskHandler                         mImageLoaderTask;
        private bool                                mIsActive;
        private UiVisualPreset                      mActivePreset;

        private readonly ObservableCollection<PresetList>   mLibraries;

        public  ObservableCollection<UiPresetCategory>      Presets { get; }
        public  string                                      ActivePresetState { get; private set; }
        public  DelegateCommand                             HideActivePreset { get; }
        public  DelegateCommand                             EditTags { get; }
        public  DelegateCommand                             CapturePresetImage { get; }

        public  double                                      ActivePresetTop { get; private set; }
        public  double                                      ActivePresetLeft { get; private set; }
        public  string                                      ActivePresetName { get; private set; }
        public  bool                                        HasTags => mActivePreset?.Preset.Tags.Any() ?? false;

        public  event EventHandler                          IsActiveChanged = delegate { };

        public BrowseViewModel( IPresetListProvider listProvider, IPresetProvider presetProvider, IPresetController presetController, IStateManager stateManager,
                                IPresetImageHandler imageHandler, IEventAggregator eventAggregator, IDialogService dialogService, IPlatformLog log ) {
            mListProvider = listProvider;
            mPresetController = presetController;
            mPresetProvider = presetProvider;
            mImageHandler = imageHandler;
            mStateManager = stateManager;
            mEventAggregator = eventAggregator;
            mDialogService = dialogService;
            mLog = log;

            mLibraries = new ObservableCollection<PresetList>();
            Presets = new ObservableCollection<UiPresetCategory>();

            HideActivePreset = new DelegateCommand( OnHideActivePreset );
            EditTags = new DelegateCommand( OnTagEdit );
            CapturePresetImage = new DelegateCommand( OnCapturePresetImage );

            ActivePresetState = cHideActivePreset;

            mPresetDisplaySubject = new Subject<UiVisualPreset>();
            mPresetDisplaySubscription =  mPresetDisplaySubject.Delay( TimeSpan.FromMilliseconds( 500 )).Subscribe( DisplayActivePreset );

            mPresetController.BlendPresetTransition = false;
            mPresetController.ConfigurePresetSequencer( PresetSequence.Sequential );
            mPresetController.ConfigurePresetTimer( PresetTimer.Infinite );
            mStateManager.EnterState( eStateTriggers.Stop );

            if( mPresetController.IsInitialized ) {
                Initialize();
            }

            mEventAggregator.Subscribe( this );
        }

        public bool IsActive {
            get => mIsActive;
            set {
                mIsActive = value;

                if( mIsActive ) {
                    LoadLibraries();
                }
            }
        }

        public void Handle( Events.ModeChanged args ) {
            if( args.ToView != ShellView.Review ) {
                mEventAggregator.Unsubscribe( this );
            }
        }

        public void Handle( Events.InitializationComplete args ) {
            Initialize();
        }

        private void Initialize() {
            LoadLibraries();
        }

        public ICollectionView Libraries {
            get {
                if( mLibrariesView == null ) {
                    mLibrariesView = CollectionViewSource.GetDefaultView( mLibraries );
                    mLibrariesView.SortDescriptions.Add( new SortDescription( "Name", ListSortDirection.Ascending ));
                }

                return mLibrariesView;
            }
        }

        public PresetList CurrentLibrary {
            get => mCurrentLibrary;
            set {
                mCurrentLibrary = value;

                OnLibraryChanged();
                RaisePropertyChanged( () => CurrentLibrary );
            }
        }

        private void OnDisplayActivePreset( UiVisualPreset preset ) {
            mPresetDisplaySubject.OnNext( preset );
        }

        private void DisplayActivePreset( UiVisualPreset preset ) {
            mActivePreset = preset;

            ActivePresetLeft = preset.Location.X;
            ActivePresetTop = preset.Location.Y;
            ActivePresetName = Path.GetFileNameWithoutExtension( preset.PresetName );
            ActivePresetState = cDisplayActivePreset;

            mPresetController.PlayPreset( preset.Preset );
            mStateManager.EnterState( eStateTriggers.Run );

            RaisePropertyChanged( () => ActivePresetLeft );
            RaisePropertyChanged( () => ActivePresetTop );
            RaisePropertyChanged( () => ActivePresetName );
            RaisePropertyChanged( () => ActivePresetState );
            RaisePropertyChanged( () => HasTags );
            RaisePropertyChanged( () => TagsTooltip );
            RaisePropertyChanged( () => IsFavorite );
            RaisePropertyChanged( () => DoNotPlay );
        }

        private void OnHideActivePreset() {
            ActivePresetState = cHideActivePreset;

            mStateManager.EnterState( eStateTriggers.Stop );

            RaisePropertyChanged( () => ActivePresetState );
        }

        public bool IsFavorite {
            get => mActivePreset?.Preset.IsFavorite ?? false;
            set => OnIsFavoriteChanged( value );
        }

        private async void OnIsFavoriteChanged( bool toState ) {
            var preset = mActivePreset?.Preset.WithFavorite( toState );

            if( preset != null ) {
                if( preset.Id.Equals( mActivePreset?.Preset.Id )) {
                    mActivePreset.UpdatePreset( preset );

                    RaisePropertyChanged( () => IsFavorite );
                }

                ( await mPresetProvider.UpdateAll( preset )).IfLeft( ex => LogException( "OnIsFavoriteChanged.Update", ex ));
            }
        }

        public bool DoNotPlay {
            get => mActivePreset != null && mActivePreset?.Preset.Rating == PresetRating.DoNotPlayValue;
            set => OnDoNotPlayChanged( value );
        }

        private async void OnDoNotPlayChanged( bool toState ) {
            var preset = mActivePreset?.Preset.WithRating( toState ? PresetRating.DoNotPlayValue : PresetRating.UnRatedValue );

            if( preset != null ) {
                if( preset.Id.Equals( mActivePreset?.Preset.Id )) {
                    mActivePreset.UpdatePreset( preset );

                    RaisePropertyChanged( () => DoNotPlay );
                }

                ( await mPresetProvider.UpdateAll( preset )).IfLeft( ex => LogException( "OnDoNotPlayChanged.Update", ex ));
            }
        }

        public string TagsTooltip => 
            mActivePreset != null ? 
                mActivePreset.Preset.Tags.Any() ? 
                    String.Join( Environment.NewLine, from t in mActivePreset.Preset.Tags orderby t.Name select t.Name ) : "Set Preset Tags" : "Set Preset Tags";

        private void OnTagEdit() {
            if( mActivePreset != null ) {
                var parameters = new DialogParameters { { TagEditDialogModel.cPresetParameter, mActivePreset } };

                mDialogService.ShowDialog( nameof( TagEditDialog ), parameters, OnTagsEdited );
            }
        }

        private async void OnTagsEdited( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var preset = result.Parameters.GetValue<Preset>( TagEditDialogModel.cPresetParameter );

                if(( preset != null ) &&
                   ( mActivePreset != null )) {
                    if( preset.Id.Equals( mActivePreset?.Preset.Id )) {
                        mActivePreset.UpdatePreset( preset );

                        RaisePropertyChanged( () => IsFavorite );
                        RaisePropertyChanged( () => HasTags );
                        RaisePropertyChanged( () => TagsTooltip );
                    }

                    ( await mPresetProvider.UpdateAll( preset )).IfLeft( ex => LogException( "OnTagsEdited", ex ));
                }
            }
        }

        private async void OnCapturePresetImage() {
            mActivePreset?.SetImage( await mImageHandler.CapturePresetImage( mActivePreset.Preset ));
        }

        private void OnLibraryChanged() {
            if( mCurrentLibrary != null ) {
                LoadPresets( mCurrentLibrary );
                mPresetController.LoadLibrary( mCurrentLibrary );
                LoadImages();
            }
        }

        private void LoadPresets( PresetList forLibrary ) {
            Presets.Clear();

            Presets.AddRange( from p in mListProvider.GetPresets( forLibrary.ListType, forLibrary.ListIdentifier ) 
                              group p by p.PrimaryCategory into g 
                              select new UiPresetCategory( g.Key, g, OnDisplayActivePreset ));
        }

        private void LoadLibraries() {
            mLibraries.Clear();

            mLibraries.AddRange( mListProvider.GetLists());

            mCurrentLibrary = mLibraries.FirstOrDefault();

            RaisePropertyChanged( () => CurrentLibrary );
        }

        protected TaskHandler ImageLoaderTask {
            get {
                if( mImageLoaderTask == null ) {
                    Execute.OnUIThread( () => mImageLoaderTask = new TaskHandler());
                }

                return mImageLoaderTask;
            }
            set => mImageLoaderTask = value;
        }

        private void LoadImages() {
            ImageLoaderTask.StartTask( () => {
                    Presets.ForEach( category => {
                        category.Presets.ForEach( preset => {
                            preset.SetImage( mImageHandler.GetPresetImage( preset.Preset ));
                        });
                    });
                },
                () => { }, 
                ex => { mLog.LogException( "LoadImages", ex ); });
        }

        private void LogException( string message, Exception ex ) {
            mLog.LogException( message, ex );
        }

        public void Dispose() {
            mPresetDisplaySubject?.Dispose();
            mPresetDisplaySubject = null;

            mPresetDisplaySubscription?.Dispose();
            mPresetDisplaySubscription = null;

            mEventAggregator.Unsubscribe( this );
        }
    }
}
