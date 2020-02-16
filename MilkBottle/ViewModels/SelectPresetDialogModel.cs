using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class SelectPresetDialogModel : PropertyChangeBase, IDialogAware {
        public  const string                cLibraryParameter = "library";
        public  const string                cPresetParameter = "preset";

        private readonly IPresetLibrarian                       mPresetLibrarian;
        private readonly ObservableCollection<MilkDropPreset>   mPresets;
        private ICollectionView                                 mPresetView;
        private string                                          mCurrentLibrary;
        private string                                          mFilterText;

        public string                       Title { get; }
        public DelegateCommand              Ok { get; }
        public DelegateCommand              Cancel { get; }

        public ObservableCollection<string> Libraries { get; }
        public MilkDropPreset               CurrentPreset { get; set; }

        public event Action<IDialogResult>  RequestClose;

        public SelectPresetDialogModel( IPresetLibrarian librarian ) {
            mPresetLibrarian = librarian;

            Title = "Select Preset";
            Libraries = new ObservableCollection<string>();
            mPresets = new ObservableCollection<MilkDropPreset>();

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );

            Libraries.AddRange( mPresetLibrarian.AvailableLibraries );
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            CurrentLibrary = parameters.GetValue<string>( cLibraryParameter );
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

            if(( listItem is MilkDropPreset preset ) &&
               (!string.IsNullOrWhiteSpace( FilterText ))) {
                if( preset.PresetName.IndexOf( FilterText, StringComparison.OrdinalIgnoreCase ) == -1 ) {
                    retValue = false;
                }
            }

            return ( retValue );
        }

        public string CurrentLibrary {
            get => mCurrentLibrary;
            set {
                mCurrentLibrary = value;

                LoadLibrary( mCurrentLibrary );
                RaisePropertyChanged( () => CurrentLibrary );
            }
        }

        private void LoadLibrary( string libraryName ) {
            mPresets.Clear();

            if(!String.IsNullOrWhiteSpace( libraryName )) {
                var library = mPresetLibrarian.GetLibrary( libraryName );

                if( library != null ) {
                    mPresets.AddRange( library.Presets );
                }
            }
        }

        public bool CanCloseDialog() {
            return  true;
        }

        public void OnDialogClosed() { }

        public void OnOk() {
            if( CurrentPreset != null ) {
                var parameter = new DialogParameters { { cPresetParameter, CurrentPreset } };

                RaiseRequestClose( new DialogResult( ButtonResult.OK, parameter ));
            }
            else {
                RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
            }
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        public virtual void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
