using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using MilkBottle.Dto;
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
        private PresetList                              mCurrentLibrary;
        private string                                  mFilterText;
        private bool                                    mListingAllLibraries;

        public string                                   Title { get; }
        public DelegateCommand                          Ok { get; }
        public DelegateCommand                          Cancel { get; }

        public ObservableCollection<PresetList>         Libraries { get; }
        public Preset                                   CurrentPreset { get; set; }
        public bool                                     IsListingSingleLibrary => !mListingAllLibraries;

        public event Action<IDialogResult>              RequestClose;

        public SelectPresetDialogModel( IPresetListProvider listProvider, IPresetProvider presetProvider ) {
            mPresetProvider = presetProvider;

            Title = "Select Preset";
            Libraries = new ObservableCollection<PresetList>();
            mPresets = new ObservableCollection<Preset>();
            mListingAllLibraries = false;

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );

            Libraries.AddRange( from l in listProvider.GetLists() orderby l.Name select l );
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            var libraryName = parameters.GetValue<string>( cLibraryParameter );

            CurrentLibrary = !String.IsNullOrWhiteSpace( libraryName ) ? 
                Libraries.FirstOrDefault( l => l.Name.Equals( libraryName )) : 
                Libraries.FirstOrDefault();
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

        public bool ListAllLibraries {
            get => mListingAllLibraries;
            set {
                mListingAllLibraries = value;

                if( value ) {
                    LoadAllPresets();
                }
                else {
                    LoadLibrary( mCurrentLibrary );
                }

                RaisePropertyChanged( () => ListAllLibraries );
                RaisePropertyChanged( () => IsListingSingleLibrary );
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

                LoadLibrary( mCurrentLibrary );
                RaisePropertyChanged( () => CurrentLibrary );
            }
        }

        private void LoadAllPresets() {
            mPresets.Clear();

            mPresetProvider.SelectPresets( list => mPresets.AddRange(( from p in list orderby p.Name select p ).Distinct()));
        }

        private void LoadLibrary( PresetList library ) {
            mPresets.Clear();

            if( library != null ) {
                mPresets.AddRange( from p in library.GetPresets() orderby p.Name select p );
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
