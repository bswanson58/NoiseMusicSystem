using System;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataProviders {
	internal class FolderStrategyProvider {
		private	 readonly IDatabaseManager	mDatabase;

		public FolderStrategyProvider( IDatabaseManager database ) {
			mDatabase = database;
		}

		public IMetaDataProvider GetProvider( StorageFile forFile ) {
			return( new FileStrategyProvider( mDatabase, forFile ));
		}
	}

	internal class FileStrategyProvider : IMetaDataProvider {
		private readonly IDatabaseManager		mDatabase;
		private readonly StorageFile			mFile;
		private	readonly Lazy<FolderStrategyInformation>	mStrategyInformation;

		public FileStrategyProvider( IDatabaseManager database, StorageFile file ) {
			mDatabase = database;
			mFile = file;

			mStrategyInformation = new Lazy<FolderStrategyInformation>(() => StorageHelpers.GetFolderStrategy( mDatabase.Database, mFile ));
		}

		private FolderStrategyInformation StrategyInformation {
			get{ return( mStrategyInformation.Value ); }
		}

		public string Artist {
			get { return( StrategyInformation.GetStrategyDefinition( eFolderStrategy.Artist )); }
		}

		public string Album {
			get { return( StrategyInformation.GetStrategyDefinition( eFolderStrategy.Album )); }
		}

		public string TrackName {
			get{ return( "" ); }
		}

		public string VolumeName {
			get{ return( StrategyInformation.GetStrategyDefinition( eFolderStrategy.Volume )); }
		}

		public void AddAvailableMetaData( DbArtist artist, DbAlbum album, DbTrack track ) {
		}
	}
}
