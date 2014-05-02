using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using Noise.Core.Database;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Recls;

namespace Noise.Core.FileStore {
	internal class FolderExplorer : IFolderExplorer, IHandle<Events.LibraryConfigurationChanged> {
		private readonly IEventAggregator		mEventAggregator;
		private readonly ILibraryConfiguration	mLibraryConfiguration;
		private readonly IStorageFolderSupport	mStorageFolderSupport;
		private readonly IRootFolderProvider	mRootFolderProvider;
		private readonly IStorageFolderProvider	mStorageFolderProvider;
		private readonly IStorageFileProvider	mStorageFileProvider;
		private bool							mStopExploring;
		private DatabaseCache<StorageFile>		mFileCache;
		private DatabaseCache<StorageFolder>	mFolderCache;

		private long	mFoldersAdded;
		private long	mFilesAdded;

		public  FolderExplorer( IEventAggregator eventAggregator, ILibraryConfiguration libraryConfiguration, IStorageFolderSupport storageFolderSupport,
								IRootFolderProvider rootFolderProvider, IStorageFolderProvider storageFolderProvider, IStorageFileProvider storageFileProvider ) {
			mEventAggregator = eventAggregator;
			mLibraryConfiguration = libraryConfiguration;
			mStorageFolderSupport = storageFolderSupport;
			mRootFolderProvider = rootFolderProvider;
			mStorageFolderProvider = storageFolderProvider;
			mStorageFileProvider = storageFileProvider;

			mEventAggregator.Subscribe( this );
		}

		public IEnumerable<RootFolder> RootFolderList() {
			var retValue = new List<RootFolder>();

			using( var folderList = mRootFolderProvider.GetRootFolderList()) {
				retValue.AddRange( folderList.List );
			}

			if( retValue.Count == 0 ) {
				LoadConfiguration();

				using( var folderList = mRootFolderProvider.GetRootFolderList()) {
					retValue.AddRange( folderList.List );
				}
			}

			return( retValue );
		}

		public void SynchronizeDatabaseFolders() {
			mStopExploring = false;

			var rootFolders = RootFolderList().ToList();

			if( rootFolders.Any()) {
					try {
						using( var fileList = mStorageFileProvider.GetAllFiles()) {
							mFileCache = new DatabaseCache<StorageFile>( fileList.List );
						}
						using( var folderList = mStorageFolderProvider.GetAllFolders()) {
							mFolderCache = new DatabaseCache<StorageFolder>( folderList.List );
						}

						foreach( var rootFolder in rootFolders ) {
							mFilesAdded = 0;
							mFoldersAdded = 0;

							if( Directory.Exists( mStorageFolderSupport.GetPath( rootFolder ))) {
								NoiseLogger.Current.LogMessage( "Synchronizing folder: {0}", rootFolder.Name );
								mEventAggregator.Publish( new Events.StatusEvent( string.Format( "Starting folder synchronization for: {0}", rootFolder.Name )));

								BuildFolder( rootFolder );
							}
							else {
								NoiseLogger.Current.LogMessage( "Storage folder does not exists: {0}", rootFolder.DisplayName );
							}

							if( mStopExploring ) {
								break;
							}

							using( var updater = mRootFolderProvider.GetFolderForUpdate( rootFolder.DbId )) {
								if( updater.Item != null ) {
									updater.Item.UpdateLibraryScan();

									updater.Update();
								}
							}

							NoiseLogger.Current.LogMessage( "In Folder '{0}' there were {1} folders added with {2} files.", rootFolder.DisplayName, mFoldersAdded, mFilesAdded );
						}

						mFileCache.Clear();
						mFolderCache.Clear();
					}
					catch( Exception ex ) {
						NoiseLogger.Current.LogException( "Exception - FolderExplorer:", ex );
					}
			}
		}

		public void Stop() {
			mStopExploring = true;
		}

		public void Handle( Events.LibraryConfigurationChanged eventArgs ) {
			LoadConfiguration();
		}

		public void LoadConfiguration() {
			if( mLibraryConfiguration.Current != null ) {
				var rootList = new List<RootFolder>();

				using( var roots = mRootFolderProvider.GetRootFolderList()) {
					rootList.AddRange( roots.List );
				}

				foreach( var folder in rootList ) {
					mRootFolderProvider.DeleteRootFolder( folder );
				}

				foreach( var mediaConfig in mLibraryConfiguration.Current.MediaLocations ) {
					var root = new RootFolder( mediaConfig.Key, mediaConfig.Path, string.Empty ) { DisplayName = mLibraryConfiguration.Current.LibraryName };
					var level = 0;

					foreach( var strategy in mLibraryConfiguration.Current.MediaLocations[0].FolderStrategy ) {
						root.FolderStrategy.SetStrategyForLevel( level, strategy );

						level++;
					}

					root.FolderStrategy.PreferFolderStrategy = mediaConfig.PreferFolderStrategy;

					mRootFolderProvider.AddRootFolder( root );
				}
			}
		}

		private void BuildFolder( StorageFolder parent ) {
			var directories = FileSearcher.Search( mStorageFolderSupport.GetPath( parent ), null, SearchOptions.Directories, 0 );
			var folderList = mFolderCache.FindList( dbFolder => dbFolder.ParentFolder == parent.DbId );

			foreach( var directory in directories ) {
				var folderName = directory.File;

				var folder = folderList.Find( dbFolder => dbFolder.Name == folderName );
				if( folder == null ) {
					folder = new StorageFolder( directory.File, parent.DbId );

					mStorageFolderProvider.AddFolder( folder );
					mFoldersAdded++;

					if( parent is RootFolder ) {
						NoiseLogger.Current.LogInfo( string.Format( "Adding folder: {0}", mStorageFolderSupport.GetPath( folder )));
					}
				}
				else {
					folderList.Remove( folder );
				}

				BuildFolderFiles( folder );
				BuildFolder( folder );

				if( mStopExploring ) {
					break;
				}
			}

			// Any folders remaining in the list must not have located on disk.
			foreach( var dbFolder in folderList ) {
				using( var updater = mStorageFolderProvider.GetFolderForUpdate( dbFolder.DbId )) {
					if( updater.Item != null ) {
						updater.Item.IsDeleted = true;

						updater.Update();
					}
				}
			}
		}

		private void BuildFolderFiles( StorageFolder storageFolder ) {
			var dbList = mFileCache.FindList( file => file.ParentFolder == storageFolder.DbId );
			var files = FileSearcher.BreadthFirst.Search( mStorageFolderSupport.GetPath( storageFolder ), null,
														  SearchOptions.Files | SearchOptions.IncludeSystem | SearchOptions.IncludeHidden, 0 );
			var newFiles = new List<StorageFile>();

			foreach( var file in files ) {
				var fileName = file.File;

				var	dbFile = dbList.Find( f => f.Name == fileName );
				if( dbFile != null ) {
					dbList.Remove( dbFile );

					if( file.ModificationTime.Ticks != dbFile.FileModifiedTicks ) {
						using( var updater = mStorageFileProvider.GetFileForUpdate( dbFile.DbId )) {
							if( updater.Item != null ) {
								updater.Item.UpdateModifiedDate( file.ModificationTime );

								updater.Update();
							}
						}
					}
				}
				else {
					newFiles.Add( new StorageFile( file.File, storageFolder.DbId, file.Size, file.ModificationTime ));

					mFilesAdded++;
				}

				if( mStopExploring ) {
					break;
				}
			}

			if( newFiles.Any()) {
				mStorageFileProvider.Add( newFiles );
			}

			// Any files that we have remaining must not exist on disk.
			foreach( var dbFile in dbList ) {
				using( var updater = mStorageFileProvider.GetFileForUpdate( dbFile.DbId )) {
					if( updater.Item != null ) {
						updater.Item.IsDeleted = true;

						updater.Update();
					}
				}
			}
		}
	}
}
