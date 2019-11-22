using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        private readonly List<SourceItem>               mAlbumContents;
        private string                                  mSourceDirectory;
        private IDisposable                             mInspectionChangedSubscription;
        private string                                  mArtistName;
        private string                                  mAlbumName;
        private string                                  mPublishDate;
        private bool                                    mIsSoundboard;
        private bool                                    mIsRadioBrodcast;
        private bool                                    mIsRemastered;
        private bool                                    mIsDeluxeEdition;
        private bool                                    mIsOtherMetadata;
        private string                                  mOtherMetadata;


        public  IItemInspectionViewModel                InspectionViewModel { get; }
        public  IFinalStructureViewModel                FinalStructureViewModel { get; }
        public  ICollectionView                         SourceList { get; }
        public  BindableCollection<SourceItem>          SelectedSourceItems { get; }

        public StructureWorkshopViewModel( IItemInspectionViewModel inspectionViewModel, IFinalStructureViewModel finalStructureViewModel, IAlbumBuilder albumBuilder,
                                           IPlatformDialogService dialogService, IPreferences preferences ) {
            InspectionViewModel = inspectionViewModel;
            FinalStructureViewModel = finalStructureViewModel;
            mAlbumBuilder = albumBuilder;
            mDialogService = dialogService;
            mPreferences = preferences;

            mAlbumContents = new List<SourceItem>();
            mSourceList = new BindableCollection<SourceItem>();
            SourceList = CollectionViewSource.GetDefaultView( mSourceList );
            SelectedSourceItems = new BindableCollection<SourceItem>();
            SelectedSourceItems.CollectionChanged += OnSelectionChanged;

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

        public bool IsDeluxeEdition {
            get => mIsDeluxeEdition;
            set {
                mIsDeluxeEdition = value;

                RaisePropertyChanged( () => IsDeluxeEdition );
                UpdateTargetStructure();
            }
        }

        public bool IsRemastered {
            get => mIsRemastered;
            set {
                mIsRemastered = value;

                RaisePropertyChanged( () => IsRemastered );
                UpdateTargetStructure();
            }
        }

        public bool IsSoundboard {
            get => mIsSoundboard;
            set {
                mIsSoundboard = value;

                RaisePropertyChanged( () => IsSoundboard );
                UpdateTargetStructure();
            }
        }

        public bool IsRadioBroadcast {
            get => mIsRadioBrodcast;
            set {
                mIsRadioBrodcast = value;

                RaisePropertyChanged( () => IsRadioBroadcast );
                UpdateTargetStructure();
            }
        }

        public bool IsOtherMetadata {
            get => mIsOtherMetadata;
            set {
                mIsOtherMetadata = value;

                RaisePropertyChanged( () => IsOtherMetadata );
                UpdateTargetStructure();
            }
        }

        public string OtherMetadata {
            get => mOtherMetadata;
            set {
                mOtherMetadata = value;

                RaisePropertyChanged( () => OtherMetadata );
                UpdateTargetStructure();
            }
        }

        private void OnSelectionChanged( object sender, NotifyCollectionChangedEventArgs args ) {
            RaiseCanExecuteChangedEvent( "CanExecute_CopyToAlbum" );
        }

        public void Execute_CopyToAlbum() {
            mAlbumContents.AddRange( SelectedSourceItems );

            UpdateTargetStructure();
        }

        public bool CanExecute_CopyToAlbum() {
            return SelectedSourceItems.Any();
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
            return new TargetAlbumLayout( ArtistName, BuildAlbumName(), mAlbumContents );
        }

        private string BuildAlbumName() {
            var retValue = AlbumName;
            var metadata = String.Empty;

            if( IsSoundboard ) {
                metadata = "Soundboard";
            }
            else if( IsDeluxeEdition && IsRemastered ) {
                metadata = "Remastered Deluxe Edition";
            }
            else if( IsDeluxeEdition ) {
                metadata = "Deluxe Edition";
            }
            else if( IsRadioBroadcast ) {
                metadata = "FM Broadcast";
            }
            else if( IsRemastered ) {
                metadata = "Remastered";
            }
            else if( IsOtherMetadata && !String.IsNullOrWhiteSpace( OtherMetadata )) {
                metadata = OtherMetadata;
            }

            if(!String.IsNullOrWhiteSpace( metadata )) {
                retValue = retValue + $" ({metadata})";
            }

            if(!String.IsNullOrWhiteSpace( PublishDate )) {
                retValue = retValue + $" - {PublishDate}";
            }

            return retValue;
        }

        public void Execute_ClearMetadata() {
            ArtistName = String.Empty;
            AlbumName = String.Empty;
            PublishDate = String.Empty;

            IsDeluxeEdition = false;
            IsRadioBroadcast = false;
            IsRemastered = false;
            IsSoundboard = false;
            IsOtherMetadata = false;
            OtherMetadata = String.Empty;

            mAlbumContents.Clear();

            UpdateTargetStructure();
        }

        public void Dispose() {
            mInspectionChangedSubscription?.Dispose();
            mInspectionChangedSubscription = null;
        }
    }
}
