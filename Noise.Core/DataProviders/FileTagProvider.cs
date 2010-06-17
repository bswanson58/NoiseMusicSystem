using System.IO;
using Eloquera.Client;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Core.MetaData;

namespace Noise.Core.DataProviders {
	public class FileTagProvider {
		private readonly IDatabaseManager	mDatabase;
		private Mp3TagProvider				mMpsTagProvider;

		public FileTagProvider( IDatabaseManager database ) {
			mDatabase = database;
			mMpsTagProvider = new Mp3TagProvider( mDatabase );
		}

		public void BuildMetaData( StorageFile storageFile, MusicTrack track ) {
			track.Encoding = DetermineAudioEncoding( storageFile );

			switch( track.Encoding ) {
				case eAudioEncoding.MP3:
					mMpsTagProvider.BuildMetaData( storageFile, track );
					break;
			}
		}

		private static eAudioEncoding DetermineAudioEncoding( StorageFile file ) {
			var retValue = eAudioEncoding.Unknown;
			var ext = Path.GetExtension( file.Name ).ToLower();

			switch( ext ) {
				case ".mp3":
					retValue = eAudioEncoding.MP3;
					break;
			}

			return( retValue );
		}
	}
}
