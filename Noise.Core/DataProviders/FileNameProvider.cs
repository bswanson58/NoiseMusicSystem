using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataProviders {
	internal class FileNameProvider {
		private readonly IDatabaseManager	mDatabaseManager;
		private readonly ILog				mLog;
		private long						mFolderId;
		private readonly Regex				mDatePattern;
		private List<StorageFile>			mFolderFiles;

		public FileNameProvider( IUnityContainer container ) {
			mDatabaseManager = container.Resolve<IDatabaseManager>();
			mLog = container.Resolve<ILog>();

			mDatePattern = new Regex( "(?<month>0?[1-9]|1[012]) [- .] (?<day>0?[1-9]|[12][0-9]|3[01]) [- .] (?<year>[0-9]{2,})", RegexOptions.IgnorePatternWhitespace );
		}

		public IMetaDataProvider GetProvider( StorageFile forFile ) {
			if( forFile.ParentFolder != mFolderId ) {
				BuildFolderFiles( forFile.ParentFolder );
			}

			return( new NameProvider( forFile, mFolderFiles, mDatePattern ));
		}

		private void BuildFolderFiles( long parentId ) {
			var database = mDatabaseManager.ReserveDatabase();

			try {
				var files = from StorageFile file in database.Database
							where ( file.ParentFolder == parentId ) && ( StorageHelpers.DetermineFileType( file ) == eFileType.Music )
							orderby file.Name select file;

				mFolderFiles = new List<StorageFile>( files.ToList());
				mFolderId = parentId;
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - FileNameProvider", ex );
			}
			finally {
				mDatabaseManager.FreeDatabase( database );
			}
		}
	}

	internal class NameProvider : IMetaDataProvider {
		private readonly StorageFile		mFile;
		private readonly IList<StorageFile>	mFolderFiles;
		private readonly Regex				mDatePattern;

		public NameProvider( StorageFile file, IList<StorageFile> folderFiles, Regex datePattern ) {
			mFolderFiles = folderFiles;
			mFile = file;
			mDatePattern = datePattern;
		}

		public string Artist {
			get{ return( "" ); }
		}

		public string Album {
			get{ return( "" ); }
		}

		public string TrackName {
			get {
				var	trackName = Path.GetFileNameWithoutExtension( mFile.Name );
				var nameParts = trackName.Split( new []{ '-' });

				return( nameParts.Count() > 1 ? nameParts[1].Trim() : trackName );
			}
		}

		public string VolumeName {
			get{ return( "" ); }
		}

		public void AddAvailableMetaData( DbArtist artist, DbAlbum album, DbTrack track ) {
			if( mFolderFiles != null ) {
				var listTrack = mFolderFiles.FirstOrDefault( item => item.Name == mFile.Name );

				if( listTrack != null ) {
					track.TrackNumber = (UInt16)( mFolderFiles.IndexOf( listTrack ) + 1 );
				}
			}

			if( track.PublishedYear == Constants.cUnknownYear ) {
				var	match = mDatePattern.Match( album.Name );

				if( match.Success ) {
					var year = Convert.ToUInt16( match.Groups["year"].Captures[0].Value );

					if( year < 30 ) {
						year += 2000;
					}
					else if( year < 100 ) {
						year += 1900;
					}

					track.PublishedYear = year;
				}
			}
		}
	}
}
