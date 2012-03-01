﻿using System;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Core.DataProviders {
	internal class FolderStrategyProvider {
		private readonly IRootFolderProvider	mRootFolderProvider;
		private readonly IStorageFolderProvider	mFolderProvider;
		private readonly ITagManager			mTagManager;

		public FolderStrategyProvider( IRootFolderProvider rootFolderProvider, IStorageFolderProvider folderProvider, ITagManager tagManager ) {
			mRootFolderProvider = rootFolderProvider;
			mFolderProvider = folderProvider;
			mTagManager = tagManager;
		}

		public IMetaDataProvider GetProvider( StorageFile forFile ) {
			return( new FileStrategyProvider( mTagManager, mRootFolderProvider, mFolderProvider, forFile ));
		}
	}

	internal class FileStrategyProvider : IMetaDataProvider {
		private	readonly ITagManager						mTagManager;
		private readonly IRootFolderProvider				mRootFolderProvider;
		private readonly IStorageFolderProvider				mStorageFolderProvider;
		private readonly StorageFile						mFile;
		private	readonly Lazy<FolderStrategyInformation>	mStrategyInformation;

		public FileStrategyProvider( ITagManager tagManager, IRootFolderProvider rootFolderProvider, IStorageFolderProvider folderProvider, StorageFile file ) {
			mTagManager = tagManager;
			mRootFolderProvider = rootFolderProvider;
			mStorageFolderProvider = folderProvider;
			mFile = file;

			mStrategyInformation = new Lazy<FolderStrategyInformation>(() => {
				FolderStrategyInformation	retValue = null;

				try {
					retValue = StorageHelpers.GetFolderStrategy( mRootFolderProvider, mStorageFolderProvider, mFile );
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
