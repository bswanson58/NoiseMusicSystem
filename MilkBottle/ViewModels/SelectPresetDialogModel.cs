using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using MilkBottle.Entities;
using MilkBottle.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class SelectPresetDialogModel : PropertyChangeBase, IDialogAware {
        public  const string                cLibraryParameter = "library";
        public  const string                cPresetParameter = "preset";

        private readonly IPresetProvider                mPresetProvider;
        private readonly ObservableCollection<Preset>   mPresets;
        private ICollectionView                         mPresetView;
        private PresetLibrary                           mCurrentLibrary;
        private string                                  mFilterText;

        public string                                   Title { get; }
        public DelegateCommand                          Ok { get; }
        public DelegateCommand                          Cancel { get; }

        public ObservableCollection<PresetLibrary>      Libraries { get; }
        public Preset                                   CurrentPreset { get; set; }

        public event Action<IDialogResult>          RequestClose;

        public SelectPresetDialogModel( IPresetLibraryProvider libraryProvider, IPresetProvider presetProvider ) {
            mPresetProvider = presetProvider;

            Title = "Select Preset";
            Libraries = new ObservableCollection<PresetLibrary>();
            mPresets = new ObservableCollection<Preset>();

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );

            libraryProvider.SelectLibraries( list => Libraries.AddRange( from l in list orderby l.Name select l ));
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            var libraryName = parameters.GetValue<string>( cLibraryParameter );

            if(!String.IsNullOrWhiteSpace( libraryName )) {
                CurrentLibrary = Libraries.FirstOrDefault( l => l.Name.Equals( libraryName ));
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

        public PresetLibrary CurrentLibrary {
            get => mCurrentLibrary;
            set {
                mCurrentLibrary = value;

                LoadLibrary( mCurrentLibrary );
                RaisePropertyChanged( () => CurrentLibrary );
            }
        }

        private void LoadLibrary( PresetLibrary library ) {
            mPresets.Clear();

            if( library != null ) {
                mPresetProvider.SelectPresets( library, list => mPresets.AddRange( from p in list orderby p.Name select p ));
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
