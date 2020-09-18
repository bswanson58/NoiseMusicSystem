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
using ReusableBits;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class BrowseViewModel : PropertyChangeBase {
        private readonly IPresetListProvider    mListProvider;
        private readonly IPlatformLog           mLog;
        private ICollectionView                 mLibrariesView;
        private PresetList                      mCurrentLibrary;
        private TaskHandler                     mImageLoaderTask;

        private readonly ObservableCollection<PresetList>   mLibraries;

        public  ObservableCollection<UiPresetCategory>      Presets { get; }

        public BrowseViewModel( IPresetListProvider listProvider, IPlatformLog log ) {
            mListProvider = listProvider;
            mLog = log;

            mLibraries = new ObservableCollection<PresetList>();
            Presets = new ObservableCollection<UiPresetCategory>();

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
                              select new UiPresetCategory( g.Key, g ));
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
