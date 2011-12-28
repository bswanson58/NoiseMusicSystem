using System.Collections.Generic;
using System.IO;
using CuttingEdge.Conditions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.FileStore {
	public static class StorageHelpers {
		public static string GetPath( IStorageFolderProvider storageFolderProvider, StorageFolder forFolder ) {
			var retValue = "";
			var pathParts = new Stack<string>();
			var folder = forFolder;

			Condition.Requires( storageFolderProvider ).IsNotNull();
			Condition.Requires( forFolder ).IsNotNull();

			pathParts.Push( folder.Name );

			while( folder.ParentFolder != Constants.cDatabaseNullOid ) {
				folder = storageFolderProvider.GetFolder( folder.ParentFolder );
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

		public static string GetPath( IStorageFolderProvider storageFolderProvider, StorageFile forFile ) {
			Condition.Requires( storageFolderProvider ).IsNotNull();
			Condition.Requires( forFile ).IsNotNull();

			var	folder = storageFolderProvider.GetFolder( forFile.ParentFolder );
			var path = GetPath( storageFolderProvider, folder );

			return( Path.Combine( path, forFile.Name ));
		}

		public static FolderStrategyInformation GetFolderStrategy( IStorageFolderProvider storageFolderProvider, StorageFile forFile ) {
			Condition.Requires( storageFolderProvider ).IsNotNull();
			Condition.Requires( forFile ).IsNotNull();

			var	retValue = new FolderStrategyInformation();
			var pathParts = new Stack<string>();
			FolderStrategy	strategy = null;

			var folder = storageFolderProvider.GetFolder( forFile.ParentFolder );

			if( folder != null ) {
				pathParts.Push( folder.Name );

				while( folder.ParentFolder != Constants.cDatabaseNullOid ) {
					folder = storageFolderProvider.GetFolder( folder.ParentFolder );
					if( folder != null ) {
						if( folder is RootFolder ) {
							strategy = (folder as RootFolder).FolderStrategy;
							
							break;
						}

						pathParts.Push( folder.Name );
					}
					else {
						break;
					}
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

		public static eFileType DetermineFileType( StorageFile file ) {
			Condition.Requires( file ).IsNotNull();

			var retValue = eFileType.Unknown;
			var ext = Path.GetExtension( file.Name ).ToLower();

			switch( ext ) {
				case ".flac":
				case ".mp3":
				case ".ogg":
				case ".wma":
					retValue = eFileType.Music;
					break;

				case ".jpg":
				case ".bmp":
					retValue = eFileType.Picture;
					break;

				case ".txt":
				case ".nfo":
					retValue = eFileType.Text;
					break;
			}

			return( retValue );
		}

		public static eAudioEncoding DetermineAudioEncoding( StorageFile file ) {
			Condition.Requires( file ).IsNotNull();

			var retValue = eAudioEncoding.Unknown;
			var ext = Path.GetExtension( file.Name ).ToLower();

			switch( ext ) {
				case ".flac":
					retValue = eAudioEncoding.FLAC;
					break;

				case ".mp3":
					retValue = eAudioEncoding.MP3;
					break;

				case ".ogg":
					retValue = eAudioEncoding.OGG;
					break;

				case".wma":
					retValue = eAudioEncoding.WMA;
					break;
			}

			return( retValue );
		}

		public static bool IsCoverFile( string fileName ) {
			Condition.Requires( fileName ).IsNotNullOrEmpty();

			var retValue = false;
			var name = Path.GetFileNameWithoutExtension( fileName ).ToLower();

			if(( name.Equals( "albumartsmall" )) ||
			   ( name.Equals( "cover" )) ||
			   ( name.Equals( "folder" )) ||
			   ( name.Equals( "front" ))) {
				retValue = true;
			}

			return( retValue );
		}

		public static short ConvertFromId3Rating( short rating ) {
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

		public static byte ConvertToId3Rating( short rating ) {
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
