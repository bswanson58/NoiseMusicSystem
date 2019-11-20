using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Data;
using Album4Matter.Dto;
using Album4Matter.Interfaces;
using Caliburn.Micro;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Album4Matter.ViewModels {
    class StructureWorkshopViewModel : AutomaticCommandBase, IDisposable {
        private readonly IPlatformDialogService         mDialogService;
        private readonly IPreferences                   mPreferences;
        private readonly IAlbumBuilder                  mAlbumBuilder;
        private readonly BindableCollection<SourceItem> mSourceList;
        private string                                  mSourceDirectory;
        private IDisposable                             mInspectionChangedSubscription;
        private string                                  mArtistName;
        private string                                  mAlbumName;
        private string                                  mPublishDate;

        public  IItemInspectionViewModel                InspectionViewModel { get; }
        public  IFinalStructureViewModel                FinalStructureViewModel { get; }
        public  ICollectionView                         SourceList { get; }

        public StructureWorkshopViewModel( IItemInspectionViewModel inspectionViewModel, IFinalStructureViewModel finalStructureViewModel, IAlbumBuilder albumBuilder,
                                           IPlatformDialogService dialogService, IPreferences preferences ) {
            InspectionViewModel = inspectionViewModel;
            FinalStructureViewModel = finalStructureViewModel;
            mAlbumBuilder = albumBuilder;
            mDialogService = dialogService;
            mPreferences = preferences;

            mSourceList = new BindableCollection<SourceItem>();
            SourceList = CollectionViewSource.GetDefaultView( mSourceList );

            mInspectionChangedSubscription = InspectionViewModel.InspectionItemChanged.Subscribe( OnInspectionChanged);

            var appPreferences = mPreferences.Load<Album4MatterPreferences>();

            SourceDirectory = appPreferences.SourceDirectory;
            CollectRootFolder( SourceDirectory );
        }

        public string SourceDirectory {
            get => mSourceDirectory;
            set {
                mSourceDirectory = value;

                mSourceList.Clear();
                RaisePropertyChanged( () => SourceDirectory );
            }
        }

        public string ArtistName {
            get => mArtistName;
            set {
                mArtistName = value;

                RaisePropertyChanged( () => ArtistName );
                UpdateTargetStructure();
            }
        }

        public string AlbumName {
            get => mAlbumName;
            set {
                mAlbumName = value;

                RaisePropertyChanged( () => AlbumName );
                UpdateTargetStructure();
            }
        }

        public string PublishDate {
            get => mPublishDate;
            set {
                mPublishDate = value;

                RaisePropertyChanged( () => PublishDate );
                UpdateTargetStructure();
            }
        }

        private void OnInspectionChanged( InspectionItemUpdate update ) {
            switch( update.ItemChanged ) {
                case InspectionItem.Artist:
                    ArtistName = update.Item;
                    break;

                case InspectionItem.Album:
                    AlbumName = update.Item;
                    break;

                case InspectionItem.Date:
                    PublishDate = update.Item;
                    break;
            }
        }

        public void Execute_BrowseSourceFolder() {
            var directory = SourceDirectory;

            if( mDialogService.SelectFolderDialog( "Select Source Directory", ref directory ) == true ) {
                SourceDirectory = directory;
                CollectRootFolder( SourceDirectory );

                var appPreferences = mPreferences.Load<Album4MatterPreferences>();

                appPreferences.SourceDirectory = SourceDirectory;
                mPreferences.Save( appPreferences );
            }
        }

        public void Execute_RefreshSourceFolder() {
            CollectRootFolder( SourceDirectory );
        }

        private void CollectRootFolder( string rootPath ) {
            if( Directory.Exists( rootPath )) {
                var rootFolder = new SourceFolder( rootPath, null );

                CollectFolder( rootFolder );

                mSourceList.Clear();
                mSourceList.AddRange( rootFolder.Children );
            }
        }

        private void CollectFolder( SourceFolder rootFolder ) {
            foreach( var directory in Directory.GetDirectories( rootFolder.FileName )) {
                var folder = new SourceFolder( directory, rootFolder.Key, OnItemInspect );

                rootFolder.Children.Add( folder );
                CollectFolder( folder );
            }

            foreach( var file in Directory.EnumerateFiles( rootFolder.FileName )) {
                rootFolder.Children.Add( new SourceFile( file, rootFolder.Key, OnItemInspect ));
            }
        }

        private void OnItemInspect( SourceItem item ) {
            InspectionViewModel.SetInspectionItem( item );
        }

        public void Execute_BuildAlbum() {
            mAlbumBuilder.BuildAlbum( CollectAlbumLayout());
        }

        private void UpdateTargetStructure() {
            FinalStructureViewModel.SetTargetLayout( CollectAlbumLayout());
        }

        private TargetAlbumLayout CollectAlbumLayout() {
            return new TargetAlbumLayout();
        }

        public void Dispose() {
            mInspectionChangedSubscription?.Dispose();
            mInspectionChangedSubscription = null;
        }
    }
}
