﻿using System.IO;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.DataProviders;
using Noise.Infrastructure.Dto;

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
			var		artworkProvider = new FileArtworkProvider( mDatabase );
			var		textProvider =new FileTextProvider( mDatabase );
			var		fileEnum = from StorageFile file in mDatabase.Database where file.FileType == eFileType.Undetermined orderby file.ParentFolder select file;

			foreach( var file in fileEnum ) {
				file.FileType = DetermineFileType( file );

				switch( file.FileType ) {
					case eFileType.Music:
						var		track = new DbTrack();

						fileNameProvider.BuildMetaData( file, track  );
						tagProvider.BuildMetaData( file, track );

						file.MetaDataPointer = mDatabase.Database.Store( track );
						mDatabase.Database.Store( file );
						break;

					case eFileType.Picture:
						artworkProvider.BuildMetaData( file );

						mDatabase.Database.Store( file );
						break;

					case eFileType.Text:
						textProvider.BuildMetaData( file );

						mDatabase.Database.Store( file );
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

				case ".jpg":
					retValue = eFileType.Picture;
					break;

				case ".txt":
				case ".nfo":
					retValue = eFileType.Text;
					break;
			}

			return( retValue );
		}

	}
}
