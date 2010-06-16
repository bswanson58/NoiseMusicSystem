using System.Collections.Generic;
using System.IO;
using CuttingEdge.Conditions;
using Eloquera.Client;

namespace Noise.Core.FileStore {
	public static class StorageHelpers {
		public	const long	cNullOid = 0;

		public static string GetPath( DB database, StorageFolder forFolder ) {
			var retValue = "";
			var pathParts = new Stack<string>();
			var folder = forFolder;

			Condition.Requires( database ).IsNotNull();
			Condition.Requires( forFolder ).IsNotNull();

			pathParts.Push( folder.Name );

			while( folder.ParentFolder != cNullOid ) {
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
	}
}
