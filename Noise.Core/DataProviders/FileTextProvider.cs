using System;
using System.IO;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataProviders {
	internal class FileTextProvider {
		private readonly IUnityContainer	mContainer;
		private readonly ILog				mLog;

		public FileTextProvider( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();
		}

		public void BuildMetaData( StorageFile file ) {
			var dbManager = mContainer.Resolve<IDatabaseManager>();
			var database = dbManager.ReserveDatabase();

			try {
				var dbText = new DbTextInfo( file.DbId, ContentType.TextInfo ) { Source = InfoSource.File, FolderLocation = file.ParentFolder };

				var	fileName = StorageHelpers.GetPath( database.Database, file );

				dbText.Text = File.ReadAllText( fileName );

				database.Insert( dbText );
			}
			catch( Exception ex ) {
				mLog.LogException( String.Format( "FileTextProvider file: {0}", StorageHelpers.GetPath( database.Database, file )), ex );
			}
			finally {
				dbManager.FreeDatabase( database );
			}
		}
	}
}
