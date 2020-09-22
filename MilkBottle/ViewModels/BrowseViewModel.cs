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
        private readonly IStateManager              mStateManager;
        private readonly IEventAggregator           mEventAggregator;
        private readonly IDialogService             mDialogService;
        private Subject<UiVisualPreset>             mPresetDisplaySubject;
        private IDisposable                         mPresetDisplaySubscription;
        private ICollectionView                     mLibrariesView;
        private PresetList                          mCurrentLibrary;
        private TaskHandler                         mImageLoaderTask;
        private bool                                mIsActive;
        private Preset                              mActivePreset;

        private readonly ObservableCollection<PresetList>   mLibraries;

        public  ObservableCollection<UiPresetCategory>      Presets { get; }
        public  string                                      ActivePresetState { get; private set; }
        public  DelegateCommand                             HideActivePreset { get; }
        public  DelegateCommand                             EditTags { get; }

        public  double                                      ActivePresetTop { get; private set; }
        public  double                                      ActivePresetLeft { get; private set; }
        public  string                                      ActivePresetName { get; private set; }
        public  bool                                        HasTags => mActivePreset?.Tags.Any() ?? false;

        public  event EventHandler                          IsActiveChanged = delegate { };

        public BrowseViewModel( IPresetListProvider listProvider, IPresetProvider presetProvider, IPresetController presetController, IStateManager stateManager,
                                IEventAggregator eventAggregator, IDialogService dialogService, IPlatformLog log ) {
            mListProvider = listProvider;
            mPresetController = presetController;
            mPresetProvider = presetProvider;
            mStateManager = stateManager;
            mEventAggregator = eventAggregator;
            mDialogService = dialogService;
            mLog = log;

            mLibraries = new ObservableCollection<PresetList>();
            Presets = new ObservableCollection<UiPresetCategory>();

            HideActivePreset = new DelegateCommand( OnHideActivePreset );
            EditTags = new DelegateCommand( OnTagEdit );

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
            mActivePreset = preset.Preset;

            ActivePresetLeft = preset.Location.X;
            ActivePresetTop = preset.Location.Y;
            ActivePresetName = Path.GetFileNameWithoutExtension( preset.Preset.Name );
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
            get => mActivePreset?.IsFavorite ?? false;
            set => OnIsFavoriteChanged( value );
        }

        private async void OnIsFavoriteChanged( bool toState ) {
            var preset = mActivePreset?.WithFavorite( toState );

            if( preset != null ) {
                if( preset.Id.Equals( mActivePreset?.Id )) {
                    mActivePreset = preset;

                    RaisePropertyChanged( () => IsFavorite );
                }

                ( await mPresetProvider.UpdateAll( preset )).IfLeft( ex => LogException( "OnIsFavoriteChanged.Update", ex ));
            }
        }

        public bool DoNotPlay {
            get => mActivePreset != null && mActivePreset.Rating == PresetRating.DoNotPlayValue;
            set => OnDoNotPlayChanged( value );
        }

        private async void OnDoNotPlayChanged( bool toState ) {
            var preset = mActivePreset?.WithRating( toState ? PresetRating.DoNotPlayValue : PresetRating.UnRatedValue );

            if( preset != null ) {
                if( preset.Id.Equals( mActivePreset?.Id )) {
                    mActivePreset = preset;

                    RaisePropertyChanged( () => DoNotPlay );
                }

                ( await mPresetProvider.UpdateAll( preset )).IfLeft( ex => LogException( "OnDoNotPlayChanged.Update", ex ));
            }
        }

        public string TagsTooltip => 
            mActivePreset != null ? 
                mActivePreset.Tags.Any() ? 
                    String.Join( Environment.NewLine, from t in mActivePreset.Tags orderby t.Name select t.Name ) : "Set Preset Tags" : "Set Preset Tags";

        private void OnTagEdit() {
            if( mActivePreset != null ) {
                var parameters = new DialogParameters { { TagEditDialogModel.cPresetParameter, mActivePreset } };

                mDialogService.ShowDialog( nameof( TagEditDialog ), parameters, OnTagsEdited );
            }
        }

        private async void OnTagsEdited( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var preset = result.Parameters.GetValue<Preset>( TagEditDialogModel.cPresetParameter );

                if( preset != null ) {
                    if( preset.Id.Equals( mActivePreset?.Id )) {
                        mActivePreset = preset;

                        RaisePropertyChanged( () => IsFavorite );
                        RaisePropertyChanged( () => HasTags );
                        RaisePropertyChanged( () => TagsTooltip );
                    }

                    ( await mPresetProvider.UpdateAll( preset )).IfLeft( ex => LogException( "OnTagsEdited", ex ));
                }
            }
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
                    var defaultImage = default( byte[]);
                    var assembly = System.Reflection.Assembly.GetExecutingAssembly();

                    using( var stream = assembly.GetManifestResourceStream( assembly.GetName().Name + ".Resources.Default Preset Image.png" )) {
                        if( stream != null ) {
                            defaultImage = new byte[stream.Length];

                            stream.Read( defaultImage, 0, defaultImage.Length);
                        }
                    }

                    Presets.ForEach( category => {
                        category.Presets.ForEach( preset => {
                            var imagePath = Path.ChangeExtension( preset.Preset.Location, ".jpg" );

                            if(!String.IsNullOrWhiteSpace( imagePath )) {
                                if( File.Exists( imagePath )) {
                                    using ( var stream = File.OpenRead( imagePath )) {
                                        var fileBytes= new byte[stream.Length];

                                        stream.Read( fileBytes, 0, fileBytes.Length );
                                        stream.Close();

                                        preset.SetImage( fileBytes );
                                    }
                                }
                                else {
                                    preset.SetImage( defaultImage );
                                }
                            }
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
