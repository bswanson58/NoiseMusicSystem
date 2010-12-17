using System.IO;

namespace Noise.Core.DataExchange {
	public abstract class BaseExporter : IDataExport {
		public abstract bool Export( string fileName );

		public bool ValidateExportPath( string fileName ) {
			var retValue = false;

			if(!string.IsNullOrWhiteSpace( fileName )) {
				var path = Path.GetDirectoryName( fileName );

				retValue = Directory.Exists( path );
			}
			return( retValue );
		}
	}
}
