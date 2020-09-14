using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Infrastructure.Interfaces;
using MilkBottle.Interfaces;
using MilkBottle.Types;
using Prism;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class PresetEditViewModel : PropertyChangeBase, IActiveAware, IHandle<Events.ModeChanged>, IHandle<Events.InitializationComplete> {
        private readonly IEventAggregator               mEventAggregator;
        private readonly IPresetListProvider            mListProvider;
        private readonly IPresetProvider                mPresetProvider;
        private readonly IPresetController              mPresetController;
        private readonly IStateManager                  mStateManager;
        private readonly ITagProvider                   mTagProvider;
        private readonly IDialogService                 mDialogService;
        private readonly IPreferences                   mPreferences;
        private readonly IPlatformLog                   mLog;
        private readonly BindableCollection<UiPreset>   mPresets;
        private ICollectionView                         mPresetView;
        private PresetList                              mCurrentLibrary;
        private UiPreset                                mCurrentPreset;
        private string                                  mFilterText;
        private bool                                    mIsActive;
        private bool                                    mDisplayDoNotPlay;

        public  BindableCollection<PresetList>          Libraries { get; }
        public  BindableCollection<UiTag>               Tags { get; }

        public  DelegateCommand                         NewTag { get; }

        public  string                                  Title => "Presets";
        public  bool                                    IsPresetSelected => mCurrentPreset != null;
        public  event EventHandler                      IsActiveChanged = delegate { };

        public PresetEditViewModel( IPresetListProvider listProvider, IPresetProvider presetProvider, ITagProvider tagProvider, IPreferences preferences,
                                    IPresetController presetController,  IStateManager stateManager, IDialogService dialogService, IEventAggregator eventAggregator,
                                    IPlatformLog log ) {
            mEventAggregator = eventAggregator;
            mPresetProvider = presetProvider;
            mListProvider = listProvider;
            mPresetController = presetController;
            mStateManager = stateManager;
            mTagProvider = tagProvider;
            mDialogService = dialogService;
            mPreferences = preferences;
            mLog = log;

            Libraries = new BindableCollection<PresetList>();
            mPresets = new BindableCollection<UiPreset>();
            Tags = new BindableCollection<UiTag>();

            NewTag = new DelegateCommand( OnNewTag );

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
                    LoadTags();
                    LoadLibraries();
                }
            }
        }

        private void Initialize() {
            LoadTags();
            LoadLibraries();

            var milkPreferences = mPreferences.Load<MilkPreferences>();

            CurrentLibrary = Libraries.FirstOrDefault( l => l.Name.Equals( milkPreferences.CurrentPresetLibrary ));
        }

        public void Handle( Events.InitializationComplete args ) {
            Initialize();
        }

        public bool DisplayDoNotPlayOnly {
            get => mDisplayDoNotPlay;
            set {
                mDisplayDoNotPlay = value;

                mPresetView.Refresh();
                RaisePropertyChanged( () => PresetListTitle );
            }
        }

        public void Handle( Events.ModeChanged args ) {
            if( args.ToView != ShellView.Review ) {
                mEventAggregator.Unsubscribe( this );
            }
        }

        public ICollectionView PresetList {
            get{ 
                if( mPresetView == null ) {
                    mPresetView = CollectionViewSource.GetDefaultView( mPresets );

                    mPresetView.Filter += OnPresetFilter;
                }

                return( mPresetView );
            }
        }

        public string FilterText {
            get => mFilterText;
            set {
                mFilterText = value;

                mPresetView.Refresh();
                RaisePropertyChanged( () => PresetListTitle );
            }
        }

        public string PresetListTitle {
            get {
                var retValue = " Library Presets ";

                if( mPresetView is CollectionView view ) {
                    retValue = $"({view.Count}) Library Presets ";
                }

                return retValue;
            }
        }

        private bool OnPresetFilter( object listItem ) {
            var retValue = true;

            if( listItem is UiPreset preset ) {
                if(!string.IsNullOrWhiteSpace( FilterText )) {
                    if( preset.Name.IndexOf( FilterText, StringComparison.OrdinalIgnoreCase ) == -1 ) {
                        retValue = false;
                    }
                }

                if( mDisplayDoNotPlay ) {
                    retValue &= preset.Preset.Rating == PresetRating.DoNotPlayValue;
                }
            }

            return ( retValue );
        }

        public PresetList CurrentLibrary {
            get => mCurrentLibrary;
            set {
                mCurrentLibrary = value;

                OnLibraryChanged();
                RaisePropertyChanged( () => CurrentLibrary );

                if( mCurrentLibrary != null ) {
                    var preferences = mPreferences.Load<MilkPreferences>();

                    preferences.CurrentPresetLibrary = mCurrentLibrary.Name;

                    mPreferences.Save( preferences );
                }
            }
        }

        public UiPreset CurrentPreset {
            get => mCurrentPreset;
            set {
                mCurrentPreset = value;

                OnPresetChanged();
                RaisePropertyChanged( () => CurrentPreset );
                RaisePropertyChanged( () => IsPresetSelected );
            }
        }

        private void OnLibraryChanged() {
            if( CurrentLibrary != null ) {
                mPresetController.LoadLibrary( CurrentLibrary );
            }

            LoadPresets();
        }

        private void OnPresetChanged() {
            SetPresetState();

            if( CurrentPreset != null ) {
                mPresetController.PlayPreset( CurrentPreset.Preset );

                mStateManager.EnterState( eStateTriggers.Run );
            }
            else {
                mStateManager.EnterState( eStateTriggers.Stop );
            }
        }

        private void LoadLibraries() {
            var previousLibrary = CurrentLibrary;

            Libraries.Clear();
            Libraries.AddRange( from l in mListProvider.GetLists() orderby l.Name select l );

            if( previousLibrary != null ) {
                CurrentLibrary = Libraries.FirstOrDefault( l => l.Name.Equals( previousLibrary.Name ));
            }
        }

        private void LoadPresets() {
            var restoreToPreset = CurrentPreset;

            mPresets.Clear();

            if( mCurrentLibrary != null ) {
                mPresets.AddRange( from p in mCurrentLibrary.GetPresets() orderby p.Name select new UiPreset( p, null, null ));
            }

            if( restoreToPreset != null ) {
                CurrentPreset = mPresets.FirstOrDefault( p => p.Preset.Id.Equals( restoreToPreset.Preset.Id ));
            }

            if( CurrentPreset == null ) {
                CurrentPreset = mPresets.FirstOrDefault();
            }

            RaisePropertyChanged( () => PresetListTitle );
        }

        private void LoadTags() {
            Tags.Clear();

            mTagProvider.SelectTags( list => Tags.AddRange( from t in list orderby t.Name select new UiTag( t, OnTagSelected, null, null )))
                .IfLeft( ex => LogException( "LoadTags", ex ));
        }

        private void OnTagSelected( UiTag tag ) {
            if( CurrentPreset != null ) {
                UpdateAndReload( CurrentPreset.Preset.WithTagState( tag.Tag, tag.IsSelected ));
            }
        }

        public bool IsFavorite {
            get => CurrentPreset?.Preset.IsFavorite ?? false;
            set {
                if( CurrentPreset != null ) {
                    UpdateAndReload( CurrentPreset.Preset.WithFavorite( value ));
                }
            }
        }

        public bool DoNotPlay {
            get => CurrentPreset != null && CurrentPreset.Preset.Rating == PresetRating.DoNotPlayValue;
            set {
                if( CurrentPreset != null ) {
                    UpdateAndReload( CurrentPreset.Preset.WithRating( value ? PresetRating.DoNotPlayValue : PresetRating.UnRatedValue ));
                }
            }
        }

        private async void UpdateAndReload( Preset preset ) {
            ( await mPresetProvider.UpdateAll( preset )).IfLeft( ex => LogException( "UpdateAndReload", ex ));

            LoadPresets();
        }

        private void SetPresetState() {
            foreach( var tag in Tags ) {
                tag.SetSelectedState( false );
            }

            if( CurrentPreset != null ) {
                foreach( var tag in CurrentPreset.Preset.Tags ) {
                    var uiTag = Tags.FirstOrDefault( t => t.Tag.Id.Equals( tag.Id ));

                    uiTag?.SetSelectedState( true );
                }
            }

            RaisePropertyChanged( () => IsFavorite );
            RaisePropertyChanged( () => DoNotPlay );
        }

        private void OnNewTag() {
            mDialogService.ShowDialog( "NewTagDialog", null, OnNewTagResult );
        }

        private void OnNewTagResult( IDialogResult result ) {
            if( result.Result == ButtonResult.OK ) {
                var newTag = new PresetTag( result.Parameters.GetValue<string>( NewTagDialogModel.cTagNameParameter ));

                mTagProvider.Insert( newTag ).IfLeft( ex => LogException( "OnNewTagResult", ex ));

                LoadTags();
                SetPresetState();
            }
        }

        private void LogException( string message, Exception ex ) {
            mLog.LogException( message, ex );
        }
    }
}
