using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.DataProviders {
	internal class FileNameProvider {
		private readonly IStorageFileProvider	mFileProvider;
		private readonly IStorageFolderSupport	mStorageFolderSupport;
		private readonly INoiseLog				mLog;
		private long							mFolderId;
		private readonly List<Regex>			mDatePatterns;
		private readonly List<Regex>			mTrackNamePatterns; 
		private List<StorageFile>				mFolderFiles;

		public FileNameProvider( IStorageFileProvider fileProvider, IStorageFolderSupport storageFolderSupport, INoiseLog log ) {
			mFileProvider = fileProvider;
			mStorageFolderSupport = storageFolderSupport;
			mLog = log;

			mDatePatterns = new List<Regex>{
				new Regex( "(?<month>0?[1-9]|1[012]) [- .] (?<day>0?[1-9]|[12][0-9]|3[01]) [- .] (?<year>[0-9]{2,})", RegexOptions.IgnorePatternWhitespace ),
				new Regex( "(?<year1>[0-9]{4})-(?<year>[0-9]{4})" ),
				new Regex( "(?<year1>[0-9]{2})-(?<year>[0-9]{2})" ),
				new Regex( "'(?<year1>[0-9]{2})-'(?<year>[0-9]{2})" ),
				new Regex( "(?<year>[0-9]{4})" ),
				new Regex( "'(?<year>[0-9]{2})" )
			};

			mTrackNamePatterns = new List<Regex>{
				new Regex( @"\(*(?<index>\d{1,4}) *[\).-]* *(?<trackName>.+)" ) // 01 - trackname / 01. trackname / 01 trackname / (01) trackname
			};
		}

		public IMetadataInfoProvider GetProvider( StorageFile forFile ) {
			if( forFile.ParentFolder != mFolderId ) {
				BuildFolderFiles( forFile.ParentFolder );
			}

			return( new NameProvider( forFile, mFolderFiles, mTrackNamePatterns, mDatePatterns ));
		}

		private void BuildFolderFiles( long parentId ) {
			try {
				using( var fileList = mFileProvider.GetFilesInFolder( parentId )) {
					mFolderFiles = new List<StorageFile>( from file in fileList.List 
														  where mStorageFolderSupport.DetermineFileType( file ) == eFileType.Music 
														  orderby file.Name 
														  select file );
				}

				mFolderId = parentId;
			}
			catch( Exception ex ) {
				mLog.LogException( "Building file list", ex );
			}
		}
	}

	internal class NameProvider : IMetadataInfoProvider {
		private readonly StorageFile		mFile;
		private readonly IList<StorageFile>	mFolderFiles;
		private readonly IEnumerable<Regex>	mDatePatterns;
		private readonly IEnumerable<Regex>	mTrackNamePatterns; 

		public NameProvider( StorageFile file, IList<StorageFile> folderFiles, IEnumerable<Regex> trackNamePatterns, IEnumerable<Regex> datePatterns ) {
			mFolderFiles = folderFiles;
			mFile = file;
			mTrackNamePatterns = trackNamePatterns;
			mDatePatterns = datePatterns;
		}

		public string Artist {
			get{ return( "" ); }
		}

		public string Album {
			get{ return( "" ); }
		}

		public string TrackName {
			get {
				var retValue = string.Empty;
				var	trackName = Path.GetFileNameWithoutExtension( mFile.Name );

				if( !string.IsNullOrWhiteSpace( trackName )) {
					foreach( var regex in mTrackNamePatterns ) {
						var match = regex.Match( trackName );

						if( match.Success ) {
							retValue = match.Groups["trackName"].Captures[0].Value;

							break;
						}
					}
				}

				return( retValue );
			}
		}

		public string VolumeName {
			get{ return( "" ); }
		}

		public void AddAvailableMetaData( DbArtist artist, DbAlbum album, DbTrack track ) {
			if( mFolderFiles != null ) {
				var listTrack = mFolderFiles.FirstOrDefault( item => item.Name == mFile.Name );

				if( listTrack != null ) {
					track.TrackNumber = (Int16)( mFolderFiles.IndexOf( listTrack ) + 1 );
				}
			}

			if( track.PublishedYear == Constants.cUnknownYear ) {
				foreach( var regex in mDatePatterns ) {
					var	match = regex.Match( album.Name );

					if( match.Success ) {
						var year = Convert.ToUInt16( match.Groups["year"].Captures[0].Value );

						if( year < 30 ) {
							year += 2000;
						}
						else if( year < 100 ) {
							year += 1900;
						}

						if(( year >= 1930 ) &&
						   ( year <= DateTime.Now.Year )) {
							track.PublishedYear = year;
						}

						break;
					}
				}
			}
		}
	}
}
