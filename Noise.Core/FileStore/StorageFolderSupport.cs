using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.FileStore {
	public class StorageFolderSupport : IStorageFolderSupport {
		private readonly INoiseLog				mLog;
		private readonly IRootFolderProvider	mRootFolderProvider;
		private readonly IStorageFolderProvider	mStorageFolderProvider;
		private readonly IStorageFileProvider	mStorageFileProvider;
		private readonly IAlbumProvider			mAlbumProvider;
		private readonly ITrackProvider			mTrackProvider;

		public StorageFolderSupport( IRootFolderProvider rootFolderProvider, IStorageFolderProvider storageFolderProvider, IStorageFileProvider storageFileProvider,
									 IAlbumProvider albumProvider, ITrackProvider trackProvider, INoiseLog log ) {
			mLog = log;
			mRootFolderProvider = rootFolderProvider;
			mStorageFolderProvider = storageFolderProvider;
			mStorageFileProvider = storageFileProvider;
			mAlbumProvider = albumProvider;
			mTrackProvider = trackProvider;
		}

		public string GetPath( StorageFolder forFolder ) {
			var retValue = "";
			var pathParts = new Stack<string>();
			var folder = forFolder;

			Condition.Requires( mStorageFolderProvider ).IsNotNull();
			Condition.Requires( forFolder ).IsNotNull();

			pathParts.Push( folder.Name );

			while( folder.ParentFolder != Constants.cDatabaseNullOid ) {
				var parentId = folder.ParentFolder;

				folder = mStorageFolderProvider.GetFolder( parentId ) ?? mRootFolderProvider.GetRootFolder( parentId );

				if( folder != null ) {
					pathParts.Push( folder.Name );
				}
				else {
					break;
				}
			}

			while( pathParts.Count > 0 ) {
				retValue = Path.Combine( retValue, pathParts.Pop());
			}

			Condition.Ensures( retValue ).IsNotEmpty();

			return( retValue );
		}

		public string GetPath( StorageFile forFile ) {
			Condition.Requires( forFile ).IsNotNull();

			var retValue = string.Empty;
			var parentId = forFile.ParentFolder;
			var folder = mStorageFolderProvider.GetFolder( parentId ) ?? mRootFolderProvider.GetRootFolder( parentId );

			if( folder != null ) {
				retValue = Path.Combine( GetPath( folder ), forFile.Name );
			}

			return( retValue );
		}

		public string GetArtistPath( long artistId ) {
			var retValue = string.Empty;

			try {
				using( var albumList = mAlbumProvider.GetAlbumList( artistId )) {
					var album = albumList.List.FirstOrDefault();

					if( album != null ) {
						var path = GetAlbumPath( album.DbId );

						if(!string.IsNullOrWhiteSpace( path )) {
							retValue = Directory.GetParent( path ).FullName;
						}
						else {
							mLog.LogMessage( "Could not get path for album '{0}'", album.Name );
						}
					}
				}
			}
			catch( Exception exception ) {
				mLog.LogException( "Building artist path", exception );
			}

			return( retValue );
		}

		public string GetAlbumPath( long albumId ) {
			var retValue = string.Empty;

			try {
				using( var albumTracks = mTrackProvider.GetTrackList( albumId )) {
					var fileList = albumTracks.List.Select( mStorageFileProvider.GetPhysicalFile );
					var parentList = fileList.Select( file => file.ParentFolder ).Distinct();
					var folderList = parentList.Select( mStorageFolderProvider.GetFolder );
					var pathList = folderList.Select( GetPath );

					retValue = FindCommonParent( pathList );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Building Album Path", ex );
			}

			return( retValue );
		}

		public StorageFolder GetArtistFolder( long artistId ) {
			var retValue = default( StorageFolder );

			try {
				using( var albumList = mAlbumProvider.GetAlbumList( artistId )) {
					var album = albumList.List.FirstOrDefault();

					if( album != null ) {
						var albumFolder = GetAlbumFolder( album.DbId );

						if( albumFolder != null ) {
							retValue = mStorageFolderProvider.GetFolder( albumFolder.ParentFolder );
						}
					}
				}
			}
			catch( Exception exception ) {
				mLog.LogException( "Retrieving artist folder", exception );
			}

			return( retValue );
		}

		public StorageFolder GetAlbumFolder( long albumId ) {
			var retValue = default( StorageFolder );

			try {
				using( var albumTracks = mTrackProvider.GetTrackList( albumId )) {
					var fileList = albumTracks.List.Select( mStorageFileProvider.GetPhysicalFile );
					var parentList = fileList.Select( file => file.ParentFolder ).Distinct();
					var folderList = parentList.Select( mStorageFolderProvider.GetFolder );

					retValue = FindCommonAncestor( folderList.ToList());
				}
			}
			catch( Exception exception ) {
				mLog.LogException( "Retrieving album folder", exception );
			}

			return( retValue );
		}

		private StorageFolder FindCommonAncestor( IList<StorageFolder> folderList ) {
			var retValue = default( StorageFolder );

			if( folderList.Count > 1 ) {
				var parentLists = new List<List<long>>();

				foreach( var folder in folderList ) {
					var parentList = new List<long>();
					var thisFolder = folder;

					while( thisFolder.ParentFolder != Constants.cDatabaseNullOid ) {
						parentList.Insert( 0, thisFolder.ParentFolder );

						thisFolder = mStorageFolderProvider.GetFolder( thisFolder.ParentFolder );
					}

					parentLists.Add( parentList );
				}

				var level = 0;
				while( level > -1 ) {
					var distinct = parentLists.Select( list => list.Count > level ? list[level] : Constants.cDatabaseNullOid ).Distinct().ToArray();

					if( distinct.Length == 1 ) {
						var folderId = distinct.First();

						if( folderId != Constants.cDatabaseNullOid ) {
							level++;

							retValue = mStorageFolderProvider.GetFolder( distinct.First());
						}
						else {
							level = -1;
						}
					}
					else {
						level = -1;
					}
				}
			}
			else {
				retValue = folderList.FirstOrDefault();
			}

			return( retValue );
		}

		private static string FindCommonParent( IEnumerable<string> paths ) {
			var retValue = string.Empty;
			var pathList = paths.Where( path => !string.IsNullOrWhiteSpace( path )).ToList();

			if( pathList.Any()) {
				if( pathList.Count == 1 ) {
					retValue = pathList.First();
				}
				else {
					var match = true;
					var index = 0;

					retValue = pathList.First().Substring( 0, index + 1 );

					while(( match ) &&
						  ( index < pathList.First().Length )) {
						var matchString = retValue;

						match = pathList.All( path => path.StartsWith( matchString ));
						index++;

						if(( match ) &&
						   ( index < pathList.First().Length )) {
							retValue = pathList.First().Substring( 0, index + 1 );
						}
					}

					if(!match ) {
						var lastSlash = retValue.LastIndexOfAny( new [] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar } );

						if( lastSlash > 0 ) {
							retValue = retValue.Substring( 0, lastSlash );
						}
					}
				}
			}

			return( retValue );
		}

		public FolderStrategyInformation GetFolderStrategy( StorageFile forFile ) {
			Condition.Requires( forFile ).IsNotNull();

			var	retValue = new FolderStrategyInformation();
			var pathParts = new Stack<string>();
			FolderStrategy	strategy = null;

			var folder = mStorageFolderProvider.GetFolder( forFile.ParentFolder );

			if( folder != null ) {
				pathParts.Push( folder.Name );

				while( folder.ParentFolder != Constants.cDatabaseNullOid ) {
					var parentFolder = mStorageFolderProvider.GetFolder( folder.ParentFolder );

					if(( parentFolder == null ) ||
					   ( parentFolder is RootFolder )) {
						var rootFolder = mRootFolderProvider.GetRootFolder( folder.ParentFolder );

						if( rootFolder != null ) {
							strategy = rootFolder.FolderStrategy;
						}
							
						break;
					}

					folder = parentFolder;
					pathParts.Push( folder.Name );
				}

				if( strategy != null ) {
					int	level = 0;

					while( pathParts.Count > 0 ) {
						var folderStrategy = strategy.StrategyForLevel( level );
						var folderName = pathParts.Pop();

						if( folderStrategy != eFolderStrategy.Undefined ) {
							retValue.SetStrategyInformation( folderStrategy, folderName );
							level++;
						}
					}

					retValue.PreferFolderStrategy = strategy.PreferFolderStrategy;
				}
			}

			return( retValue );
		}

		public eFileType DetermineFileType( StorageFile file ) {
			Condition.Requires( file ).IsNotNull();
			Condition.Requires( file.Name ).IsNotNullOrEmpty();

			var retValue = eFileType.Unknown;

			if(!string.IsNullOrEmpty( file.Name )) {
				retValue = DetermineFileType( file.Name );
			}

			return( retValue );
		}

		public eFileType DetermineFileType( string fileName ) {
			Condition.Requires( fileName ).IsNotNullOrEmpty();

			var retValue = eFileType.Unknown;
			var ext = Path.GetExtension( fileName );

			if(!string.IsNullOrEmpty( ext )) {
				switch( ext.ToLower()) {
                    case ".aac":
					case ".flac":
					case ".mp3":
                    case ".m4a":
					case ".ogg":
					case ".wma":
						retValue = eFileType.Music;
						break;

					case ".jpg":
					case ".jpeg":
					case ".png":
					case ".bmp":
						retValue = eFileType.Picture;
						break;

					case ".txt":
					case ".nfo":
						retValue = eFileType.Text;
						break;

					case Constants.SidecarExtension:
						retValue = eFileType.Sidecar;
						break;
				}
			}

			return( retValue );
		}

		public eAudioEncoding DetermineAudioEncoding( StorageFile file ) {
			Condition.Requires( file ).IsNotNull();

			var retValue = eAudioEncoding.Unknown;
			var ext = Path.GetExtension( file.Name );

			if(!string.IsNullOrEmpty( ext )) {
				switch( ext.ToLower()) {
					case ".flac":
						retValue = eAudioEncoding.FLAC;
						break;

					case ".mp3":
						retValue = eAudioEncoding.MP3;
						break;

                    case ".m4a":
                    case ".aac":
                        retValue = eAudioEncoding.AAC;
                        break;

					case ".ogg":
						retValue = eAudioEncoding.OGG;
						break;

					case".wma":
						retValue = eAudioEncoding.WMA;
						break;
				}
			}

			return( retValue );
		}

		public bool IsCoverFile( string fileName ) {
			Condition.Requires( fileName ).IsNotNullOrEmpty();

			var retValue = false;
			var name = Path.GetFileNameWithoutExtension( fileName );

			if(!string.IsNullOrWhiteSpace( name )) {
				var lowerName = name.ToLower();

				if(( lowerName.Equals( "albumartsmall" )) ||
				   ( lowerName.Equals( "cover" )) ||
				   ( lowerName.Equals( "folder" )) ||
				   ( lowerName.Equals( "front" ))) {
					retValue = true;
				}
			}

			return( retValue );
		}

		public short ConvertFromId3Rating( short rating ) {
			short	retValue = 0;

			if( rating > 218 ) {
				retValue = 5;
			}
			else if( rating > 167 ) {
				retValue = 4;
			}
			else if( rating > 113 ) {
				retValue = 3;
			}
			else if( rating > 49 ) {
				retValue = 2;
			}
			else if( rating > 8 ) {
				retValue = 1;
			}
			else if( rating == 2 ) {
				retValue = -1;
			}
			else if( rating == 1 ) {
				retValue = 1;
			}
			else if(( rating > 0 ) &&
			        ( rating <= 5 )) {
				retValue = rating;
			}

			return( retValue );
		}

		public byte ConvertToId3Rating( short rating ) {
			byte	retValue = 0;

			switch( rating ) {
				case -1:
					retValue = 2;
					break;
				case 1:
					retValue = 32;
					break;
				case 2:
					retValue = 64;
					break;
				case 3:
					retValue = 128;
					break;
				case 4:
					retValue = 196;
					break;
				case 5:
					retValue = 255;
					break;
			}
			return( retValue );
		}
	}
}
