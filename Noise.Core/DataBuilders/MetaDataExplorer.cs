using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CuttingEdge.Conditions;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.DataProviders;
using Noise.Core.FileStore;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataBuilders {
	public class MetaDataExplorer : IMetaDataExplorer {
		private readonly IUnityContainer		mContainer;
		private readonly IDatabaseManager		mDatabase;
		private readonly FileTagProvider		mTagProvider;
		private readonly FileNameProvider		mFileNameProvider;
		private readonly FolderStrategyProvider	mStrategyProvider;
		private readonly DefaultProvider		mDefaultProvider;
		private readonly ILog					mLog;
		private bool							mStopExploring;
		private DatabaseChangeSummary			mSummary;

		public  MetaDataExplorer( IUnityContainer container ) {
			mContainer = container;
			mDatabase = mContainer.Resolve<IDatabaseManager>();
			mLog = mContainer.Resolve<ILog>();

			mTagProvider = new FileTagProvider( mDatabase );
			mFileNameProvider = new FileNameProvider( mDatabase );
			mStrategyProvider = new FolderStrategyProvider( mDatabase );
			mDefaultProvider = new DefaultProvider();
		}

		public void BuildMetaData( DatabaseChangeSummary summary ) {
			Condition.Requires( summary ).IsNotNull();
			mStopExploring = false;
			mSummary = summary;

			try {
				var		artworkProvider = new FileArtworkProvider( mDatabase );
				var		textProvider =new FileTextProvider( mDatabase );
				var		fileEnum = from StorageFile file in mDatabase.Database where file.FileType == eFileType.Undetermined orderby file.ParentFolder select file;

				foreach( var file in fileEnum ) {
					file.FileType = DetermineFileType( file );

					switch( file.FileType ) {
						case eFileType.Music:
							BuildMusicMetaData( file );
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

					if( mStopExploring ) {
						break;
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Building Metadata:", ex );
			}
		}

		public void Stop() {
			mStopExploring = true;
		}

		private void BuildMusicMetaData( StorageFile file ) {
			Condition.Requires( file ).IsNotNull();

			try {
				var	track = new DbTrack();
				var folderStrategy = StorageHelpers.GetFolderStrategy( mDatabase.Database, file );
				var dataProviders = new List<IMetaDataProvider>();

				track.Encoding = DetermineAudioEncoding( file );

				if( folderStrategy.PreferFolderStrategy ) {
					dataProviders.Add( mStrategyProvider.GetProvider( file ));
					dataProviders.Add( mTagProvider.GetProvider( file, track.Encoding ));
					dataProviders.Add( mFileNameProvider.GetProvider( file ));
					dataProviders.Add( mDefaultProvider.GetProvider( file ));
				}
				else {
					dataProviders.Add( mTagProvider.GetProvider( file, track.Encoding ));
					dataProviders.Add( mStrategyProvider.GetProvider( file ));
					dataProviders.Add( mFileNameProvider.GetProvider( file ));
					dataProviders.Add( mDefaultProvider.GetProvider( file ));
				}

				var artist = DetermineArtist( dataProviders );
				if( artist != null ) {
					var album = DetermineAlbum( dataProviders, artist );

					if( album != null ) {
						track.Name = DetermineTrackName( dataProviders );

						if(!string.IsNullOrWhiteSpace( track.Name )) {
							track.VolumeName = DetermineVolumeName( dataProviders );

							foreach( var provider in dataProviders ) {
								provider.AddAvailableMetaData( artist, album, track );
							}

							track.Album = mDatabase.Database.GetUid( album );
							file.MetaDataPointer = mDatabase.Database.Store( track );
							mDatabase.Database.Store( file );
							mSummary.TracksAdded++;
						}
						else {
							mLog.LogMessage( "Track name cannot be determined for file: {0}", StorageHelpers.GetPath( mDatabase.Database, file ));
						}
					}
					else {
						mLog.LogMessage( "Album cannot be determined for file: {0}", StorageHelpers.GetPath( mDatabase.Database, file ));
					}
				}
				else {
					mLog.LogMessage( "Artist cannot determined for file: {0}", StorageHelpers.GetPath( mDatabase.Database, file ));
				}
			}
			catch( Exception ex ) {
				mLog.LogException( String.Format( "Building Music Metadata for: {0}", StorageHelpers.GetPath( mDatabase.Database, file )), ex );
			}
		}

		private DbArtist DetermineArtist( IEnumerable<IMetaDataProvider> providers ) {
			DbArtist	retValue = null;
			var			artistName = "";

			foreach( var provider in providers ) {
				artistName = provider.Artist;

				if(!string.IsNullOrWhiteSpace( artistName )) {
					break;
				}
			}

			if(!string.IsNullOrWhiteSpace( artistName )) {
				var parm = mDatabase.Database.CreateParameters();

				parm["artistName"] = artistName;

				retValue = mDatabase.Database.ExecuteScalar( "SELECT DbArtist WHERE Name = @artistName", parm ) as DbArtist;
				if( retValue == null ) {
					retValue = new DbArtist { Name = artistName };

					mDatabase.Database.Store( retValue );
					mSummary.ArtistsAdded++;
					mLog.LogInfo( "Added artist: {0}", retValue.Name );
				}
			}

			return( retValue );
		}

		private DbAlbum DetermineAlbum( IEnumerable<IMetaDataProvider> providers, DbArtist artist ) {
			DbAlbum		retValue = null;
			var			albumName = "";

			foreach( var provider in providers ) {
				albumName = provider.Album;

				if(!string.IsNullOrWhiteSpace( albumName )) {
					break;
				}
			}

			if(!string.IsNullOrWhiteSpace( albumName )) {
				var artistId = mDatabase.Database.GetUid( artist );

				retValue = ( from DbAlbum album in mDatabase.Database where album.Name == albumName && album.Artist == artistId select album ).FirstOrDefault();
				if( retValue == null ) {
					retValue = new DbAlbum { Name = albumName, Artist = mDatabase.Database.GetUid( artist ) };

					mDatabase.Database.Store( retValue );
					mSummary.AlbumsAdded++;
					mLog.LogInfo( "Added album: {0}", retValue.Name );
				}
			}

			return( retValue );
		}

		private static string DetermineTrackName( IEnumerable<IMetaDataProvider> providers ) {
			var retValue = "";

			foreach( var provider in providers ) {
				retValue = provider.TrackName;

				if(!string.IsNullOrWhiteSpace( retValue )) {
					break;
				}
			}

			return( retValue );
		}

		private static string DetermineVolumeName( IEnumerable<IMetaDataProvider> providers ) {
			var retValue = "";

			foreach( var provider in providers ) {
				retValue = provider.VolumeName;

				if(!string.IsNullOrWhiteSpace( retValue )) {
					break;
				}
			}

			return( retValue );
		}

		private static eFileType DetermineFileType( StorageFile file ) {
			var retValue = eFileType.Unknown;
			var ext = Path.GetExtension( file.Name ).ToLower();

			switch( ext ) {
				case ".mp3":
				case ".flac":
				case ".wma":
					retValue = eFileType.Music;
					break;

				case ".jpg":
				case ".bmp":
					retValue = eFileType.Picture;
					break;

				case ".txt":
				case ".nfo":
					retValue = eFileType.Text;
					break;
			}

			return( retValue );
		}

		private static eAudioEncoding DetermineAudioEncoding( StorageFile file ) {
			var retValue = eAudioEncoding.Unknown;
			var ext = Path.GetExtension( file.Name ).ToLower();

			switch( ext ) {
				case ".flac":
					retValue = eAudioEncoding.FLAC;
					break;

				case ".mp3":
					retValue = eAudioEncoding.MP3;
					break;

				case".wma":
					retValue = eAudioEncoding.WMA;
					break;
			}

			return( retValue );
		}
	}
}
