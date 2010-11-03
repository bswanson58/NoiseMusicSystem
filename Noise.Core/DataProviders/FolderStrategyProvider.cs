using System;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataProviders {
	internal class FolderStrategyProvider {
		private	readonly IUnityContainer	mContainer;

		public FolderStrategyProvider( IUnityContainer container ) {
			mContainer = container;
		}

		public IMetaDataProvider GetProvider( StorageFile forFile ) {
			return( new FileStrategyProvider( mContainer, forFile ));
		}
	}

	internal class FileStrategyProvider : IMetaDataProvider {
		private readonly IDatabaseManager					mDatabaseManager;
		private	readonly ITagManager						mTagManager;
		private readonly ILog								mLog;
		private readonly StorageFile						mFile;
		private	readonly Lazy<FolderStrategyInformation>	mStrategyInformation;

		public FileStrategyProvider( IUnityContainer container, StorageFile file ) {
			mDatabaseManager = container.Resolve<IDatabaseManager>();

			var noiseManager = container.Resolve<INoiseManager>();
			mTagManager = noiseManager.TagManager;

			mLog = container.Resolve<ILog>();
			mFile = file;

			mStrategyInformation = new Lazy<FolderStrategyInformation>(() => {
				FolderStrategyInformation	retValue = null;

				var database = mDatabaseManager.ReserveDatabase();

				try {
					retValue = StorageHelpers.GetFolderStrategy( database.Database, mFile );
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - FileStrategyProvider:", ex );
				}
				finally {
					mDatabaseManager.FreeDatabase( database );
				}

				return( retValue );
			});
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
			if( track.CalculatedGenre == Constants.cDatabaseNullOid ) {
				track.CalculatedGenre = mTagManager.ResolveGenre( StrategyInformation.GetStrategyDefinition( eFolderStrategy.Genre ));
			}
		}
	}
}
