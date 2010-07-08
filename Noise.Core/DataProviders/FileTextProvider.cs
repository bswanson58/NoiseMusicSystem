using System;
using System.IO;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataProviders {
	internal class FileTextProvider {
		private readonly IDatabaseManager	mDatabase;

		public FileTextProvider( IDatabaseManager database ) {
			mDatabase = database;
		}

		public void BuildMetaData( StorageFile file ) {
			var fileId = mDatabase.Database.GetUid( file );
			var dbText = new DbTextInfo( fileId ) { InfoType = TextInfoTypes.Unknown, Source = InfoSource.File, FolderLocation = file.ParentFolder };

			try {
				var	fileName = StorageHelpers.GetPath( mDatabase.Database, file );

				dbText.Text = File.ReadAllText( fileName );

				mDatabase.Database.Store( dbText );
			}
			catch( Exception ex ) {
				
			}
		}
	}
}
