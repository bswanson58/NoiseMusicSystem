using System.Collections.Generic;
using System.IO;
using CuttingEdge.Conditions;
using Eloquera.Client;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;

namespace Noise.Core.FileStore {
	public static class StorageHelpers {
		public static string GetPath( DB database, StorageFolder forFolder ) {
			var retValue = "";
			var pathParts = new Stack<string>();
			var folder = forFolder;

			Condition.Requires( database ).IsNotNull();
			Condition.Requires( forFolder ).IsNotNull();

			pathParts.Push( folder.Name );

			while( folder.ParentFolder != Constants.cDatabaseNullOid ) {
				var param = database.CreateParameters();

				param["id"] = folder.ParentFolder;

				folder = database.ExecuteScalar("Select StorageFolder Where DbId = @id", param ) as StorageFolder;
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

		public static string GetPath( DB database, StorageFile forFile ) {
			Condition.Requires( database ).IsNotNull();
			Condition.Requires( forFile ).IsNotNull();

			var param = database.CreateParameters();

			param["id"] = forFile.ParentFolder;

			var folder = database.ExecuteScalar( "SELECT StorageFolder WHERE DbId = @id", param) as StorageFolder;
			var path = GetPath( database, folder );

			return( Path.Combine( path, forFile.Name ));
		}

		public static FolderStrategyInformation GetFolderStrategy( DB database, StorageFile forFile ) {
			Condition.Requires( database ).IsNotNull();
			Condition.Requires( forFile ).IsNotNull();

			var	retValue = new FolderStrategyInformation();
			var pathParts = new Stack<string>();
			var param = database.CreateParameters();
			FolderStrategy	strategy = null;

			param["id"] = forFile.ParentFolder;
			var folder = database.ExecuteScalar( "SELECT StorageFolder WHERE DbId = @id", param ) as StorageFolder;

			if( folder != null ) {
				pathParts.Push( folder.Name );

				while( folder.ParentFolder != Constants.cDatabaseNullOid ) {
					param["id"] = folder.ParentFolder;

					folder = database.ExecuteScalar( "SELECT StorageFolder WHERE DbId = @id", param ) as StorageFolder;
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
	}
}
