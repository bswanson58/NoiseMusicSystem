using System;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.DataProviders {
	internal class FolderStrategyProvider {
		private readonly IStorageFolderSupport	mStorageFolderSupport;
		private readonly ITagManager			mTagManager;

		public FolderStrategyProvider( IStorageFolderSupport storageFolderSupport, ITagManager tagManager ) {
			mStorageFolderSupport = storageFolderSupport;
			mTagManager = tagManager;
		}

		public IMetaDataProvider GetProvider( StorageFile forFile ) {
			return( new FileStrategyProvider( mTagManager, mStorageFolderSupport, forFile ));
		}
	}

	internal class FileStrategyProvider : IMetaDataProvider {
		private	readonly ITagManager						mTagManager;
		private readonly IStorageFolderSupport				mStorageFolderSupport;
		private readonly StorageFile						mFile;
		private	readonly Lazy<FolderStrategyInformation>	mStrategyInformation;

		public FileStrategyProvider( ITagManager tagManager, IStorageFolderSupport storageFolderSupport, StorageFile file ) {
			mTagManager = tagManager;
			mStorageFolderSupport = storageFolderSupport;
			mFile = file;

			mStrategyInformation = new Lazy<FolderStrategyInformation>(() => {
				FolderStrategyInformation	retValue = null;

				try {
					retValue = mStorageFolderSupport.GetFolderStrategy( mFile );
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - FileStrategyProvider:", ex );
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
