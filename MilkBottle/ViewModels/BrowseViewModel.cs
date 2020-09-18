using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using MoreLinq;
using Prism.Commands;
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class BrowseViewModel : PropertyChangeBase {
        private const string					cDisplayActivePreset = "_displayActivePreset";
        private const string					cHideActivePreset = "_normal";

        private readonly IPresetListProvider    mListProvider;
        private readonly IPlatformLog           mLog;
        private ICollectionView                 mLibrariesView;
        private PresetList                      mCurrentLibrary;
        private TaskHandler                     mImageLoaderTask;

        private readonly ObservableCollection<PresetList>   mLibraries;

        public  ObservableCollection<UiPresetCategory>      Presets { get; }
        public  string                                      ActivePresetState { get; private set; }
        public  DelegateCommand                             HideActivePreset { get; }

        public  double                                      ActivePresetTop { get; private set; }
        public  double                                      ActivePresetLeft { get; private set; }

        public BrowseViewModel( IPresetListProvider listProvider, IPlatformLog log ) {
            mListProvider = listProvider;
            mLog = log;

            mLibraries = new ObservableCollection<PresetList>();
            Presets = new ObservableCollection<UiPresetCategory>();

            HideActivePreset = new DelegateCommand( OnHideActivePreset );

            ActivePresetState = cHideActivePreset;

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
            ActivePresetLeft = preset.Location.X;
            ActivePresetTop = preset.Location.Y;
            ActivePresetState = cDisplayActivePreset;

            RaisePropertyChanged( () => ActivePresetLeft );
            RaisePropertyChanged( () => ActivePresetTop );
            RaisePropertyChanged( () => ActivePresetState );
        }

        private void OnHideActivePreset() {
            ActivePresetState = cHideActivePreset;

            RaisePropertyChanged( () => ActivePresetState );
        }

        private void OnLibraryChanged() {
            if( mCurrentLibrary != null ) {
                LoadPresets( mCurrentLibrary );
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
                            var imagePath = Path.ChangeExtension( preset.Preset.Location, ".jpg" );

                            if((!String.IsNullOrWhiteSpace( imagePath )) && 
                               ( File.Exists( imagePath ))) {
                                using ( var stream = File.OpenRead( imagePath )) {
                                    var fileBytes= new byte[stream.Length];

                                    stream.Read( fileBytes, 0, fileBytes.Length );
                                    stream.Close();

                                    preset.SetImage( fileBytes );
                                }
                            }
                        });
                    });
                },
                () => { }, 
                ex => { mLog.LogException( "LoadImages", ex ); });
        }
    }
}
