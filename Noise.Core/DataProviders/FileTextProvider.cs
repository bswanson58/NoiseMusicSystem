using System;
using System.IO;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataProviders {
	internal class FileTextProvider {
		private readonly IDatabaseManager	mDatabase;
		private readonly ILog				mLog;

		public FileTextProvider( IDatabaseManager database ) {
			mDatabase = database;

			mLog = new Log();
		}

		public void BuildMetaData( StorageFile file ) {
			try {
				var fileId = mDatabase.Database.GetUid( file );
				var dbText = new DbTextInfo( fileId, ContentType.TextInfo ) { Source = InfoSource.File, FolderLocation = file.ParentFolder };

				var	fileName = StorageHelpers.GetPath( mDatabase.Database, file );

				dbText.Text = File.ReadAllText( fileName );

				mDatabase.Database.Store( dbText );
			}
			catch( Exception ex ) {
				mLog.LogException( String.Format( "FileTextProvider file: {0}", StorageHelpers.GetPath( mDatabase.Database, file )), ex );
			}
		}
	}
}
