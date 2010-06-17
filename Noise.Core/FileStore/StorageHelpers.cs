using System.Collections.Generic;
using System.IO;
using CuttingEdge.Conditions;
using Eloquera.Client;
using Eloquera.Linq;
using Noise.Core.Database;

namespace Noise.Core.FileStore {
	public static class StorageHelpers {
		public static string GetPath( DB database, StorageFolder forFolder ) {
			var retValue = "";
			var pathParts = new Stack<string>();
			var folder = forFolder;

			Condition.Requires( database ).IsNotNull();
			Condition.Requires( forFolder ).IsNotNull();

			pathParts.Push( folder.Name );

			while( folder.ParentFolder != DatabaseManager.cNullOid ) {
				var param = database.CreateParameters();

				param["id"] = folder.ParentFolder;

				folder = database.ExecuteScalar("Select StorageFolder Where $ID = @id", param ) as StorageFolder;
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
			var param = database.CreateParameters();

			param["id"] = forFile.ParentFolder;

			var folder = database.ExecuteScalar( "SELECT StorageFolder WHERE $ID = @id", param) as StorageFolder;
			var path = GetPath( database, folder );

			return( Path.Combine( path, forFile.Name ));
		}
	}
}
