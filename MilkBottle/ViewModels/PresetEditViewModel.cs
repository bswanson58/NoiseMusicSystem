using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using MilkBottle.Types;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class PresetEditViewModel : PropertyChangeBase, IHandle<Events.ModeChanged>, IHandle<Events.InitializationComplete> {
        private readonly IEventAggregator           mEventAggregator;
        private readonly IPresetListProvider        mListProvider;
        private readonly IPresetProvider            mPresetProvider;
        private readonly IPresetController          mPresetController;
        private readonly IStateManager              mStateManager;
        private readonly ITagProvider               mTagProvider;
        private readonly IDialogService             mDialogService;
        private readonly IPreferences               mPreferences;
        private readonly BindableCollection<Preset> mPresets;
        private ICollectionView                     mPresetView;
        private PresetList                          mCurrentLibrary;
        private Preset                              mCurrentPreset;
        private string                              mFilterText;

        public  BindableCollection<PresetList>      Libraries { get; }
        public  BindableCollection<UiTag>           Tags { get; }

        public  DelegateCommand                     NewTag { get; }

        public PresetEditViewModel( IPresetListProvider listProvider, IPresetProvider presetProvider, ITagProvider tagProvider, IPreferences preferences,
                                    IPresetController presetController,  IStateManager stateManager, IDialogService dialogService, IEventAggregator eventAggregator ) {
            mEventAggregator = eventAggregator;
            mPresetProvider = presetProvider;
            mListProvider = listProvider;
            mPresetController = presetController;
            mStateManager = stateManager;
            mTagProvider = tagProvider;
            mDialogService = dialogService;
            mPreferences = preferences;

            Libraries = new BindableCollection<PresetList>();
            mPresets = new BindableCollection<Preset>();
            Tags = new BindableCollection<UiTag>();

            NewTag = new DelegateCommand( OnNewTag );

            mPresetController.BlendPresetTransition = false;
            mPresetController.ConfigurePresetSequencer( PresetSequence.Sequential );
            mPresetController.ConfigurePresetTimer( PresetTimer.Infinite );
            mStateManager.EnterState( eStateTriggers.Stop );

            LoadLibraries();
            LoadTags();

            if( mPresetController.IsInitialized ) {
                Initialize();
            }

            mEventAggregator.Subscribe( this );
        }

        private void Initialize() {
            var milkPreferences = mPreferences.Load<MilkPreferences>();

            CurrentLibrary = Libraries.FirstOrDefault( l => l.Name.Equals( milkPreferences.CurrentPresetLibrary ));
        }

        public void Handle( Events.InitializationComplete args ) {
            Initialize();
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
            }
        }

        private bool OnPresetFilter( object listItem ) {
            var retValue = true;

            if(( listItem is Preset preset ) &&
               (!string.IsNullOrWhiteSpace( FilterText ))) {
                if( preset.Name.IndexOf( FilterText, StringComparison.OrdinalIgnoreCase ) == -1 ) {
                    retValue = false;
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

        public Preset CurrentPreset {
            get => mCurrentPreset;
            set {
                mCurrentPreset = value;

                OnPresetChanged();
                RaisePropertyChanged( () => CurrentPreset );
            }
        }

        private void OnLibraryChanged() {
            LoadPresets();

            if( CurrentLibrary != null ) {
                mPresetController.LoadLibrary( CurrentLibrary );
            }
        }

        private void OnPresetChanged() {
            SetPresetState();

            if( CurrentPreset != null ) {
                mPresetController.PlayPreset( CurrentPreset );

                mStateManager.EnterState( eStateTriggers.Run );
            }
            else {
                mStateManager.EnterState( eStateTriggers.Stop );
            }
        }

        private void LoadLibraries() {
            Libraries.Clear();

            Libraries.AddRange( mListProvider.GetLists());
        }

        private void LoadPresets() {
            var restoreToPreset = CurrentPreset;

            mPresets.Clear();

            if( mCurrentLibrary != null ) {
                mPresets.AddRange( mCurrentLibrary.GetPresets());
            }

            if( restoreToPreset != null ) {
                CurrentPreset = mPresets.FirstOrDefault( p => p.Id.Equals( restoreToPreset.Id ));
            }

            if( CurrentPreset == null ) {
                CurrentPreset = mPresets.FirstOrDefault();
            }
        }

        private void LoadTags() {
            Tags.Clear();

            mTagProvider.SelectTags( list => Tags.AddRange( from t in list orderby t.Name select new UiTag( t, OnTagSelected )));
        }

        private void OnTagSelected( UiTag tag ) {
            if( CurrentPreset != null ) {
                var preset = CurrentPreset.WithTagState( tag.Tag, tag.IsSelected );

                mPresetProvider.Update( preset );

                LoadPresets();
            }
        }

        public bool IsFavorite {
            get => CurrentPreset?.IsFavorite ?? false;
            set {
                if( CurrentPreset != null ) {
                    var preset = CurrentPreset.WithFavorite( value );

                    mPresetProvider.Update( preset );

                    LoadPresets();
                }
            }
        }

        public bool DoNotPlay {
            get => CurrentPreset != null && CurrentPreset.Rating == PresetRating.DoNotPlayValue;
            set {
                if( CurrentPreset != null ) {
                    var preset = CurrentPreset.WithRating( value ? PresetRating.DoNotPlayValue : PresetRating.UnRatedValue );

                    mPresetProvider.Update( preset );

                    LoadPresets();
                }
            }
        }

        private void SetPresetState() {
            foreach( var tag in Tags ) {
                tag.SetSelectedState( false );
            }

            if( CurrentPreset != null ) {
                foreach( var tag in CurrentPreset.Tags ) {
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

                mTagProvider.Insert( newTag );

                LoadTags();
            }
        }
    }
}
