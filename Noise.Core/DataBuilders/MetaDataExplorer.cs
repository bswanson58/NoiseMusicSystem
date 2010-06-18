using System.IO;
using Eloquera.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.DataProviders;
using Noise.Core.FileStore;
using Noise.Core.MetaData;

namespace Noise.Core.DataBuilders {
	public class MetaDataExplorer : IMetaDataExplorer {
		private readonly IUnityContainer	mContainer;
		private readonly IDatabaseManager	mDatabase;

		public  MetaDataExplorer( IUnityContainer container ) {
			mContainer = container;
			mDatabase = mContainer.Resolve<IDatabaseManager>();
		}

		public void BuildMetaData() {
			var		fileNameProvider = new FileNameProvider( mDatabase );
			var		tagProvider = new FileTagProvider( mDatabase );
			var		fileEnum = from StorageFile file in mDatabase.Database where file.FileType == eFileType.Undetermined orderby file.ParentFolder select file;

			foreach( var file in fileEnum ) {
				file.FileType = DetermineFileType( file );
				mDatabase.Database.Store( file );

				switch( file.FileType ) {
					case eFileType.Music:
						var		track = new DbTrack();

						fileNameProvider.BuildMetaData( file, track  );
						tagProvider.BuildMetaData( file, track );

						mDatabase.Database.Store( track );
						break;
				}
			}

			var	lastFmProvider = new LastFmProvider( mDatabase );
			lastFmProvider.BuildMetaData();
		}

		private static eFileType DetermineFileType( StorageFile file ) {
			var retValue = eFileType.Unknown;
			var ext = Path.GetExtension( file.Name ).ToLower();

			switch( ext ) {
				case ".mp3":
					retValue = eFileType.Music;
					break;
			}

			return( retValue );
		}
	}
}
