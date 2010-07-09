using System.IO;
using Noise.Core.Database;
using Noise.Infrastructure.Dto;

namespace Noise.Core.DataProviders {
	public class FileTagProvider {
		private readonly IDatabaseManager	mDatabase;
		private readonly Mp3TagProvider		mMp3TagProvider;

		public FileTagProvider( IDatabaseManager database ) {
			mDatabase = database;
			mMp3TagProvider = new Mp3TagProvider( mDatabase );
		}

		public void BuildMetaData( StorageFile storageFile, DbTrack track ) {
			track.Encoding = DetermineAudioEncoding( storageFile );

			switch( track.Encoding ) {
				case eAudioEncoding.MP3:
				case eAudioEncoding.FLAC:
					mMp3TagProvider.BuildMetaData( storageFile, track );
					break;
			}
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
			}

			return( retValue );
		}
	}
}
