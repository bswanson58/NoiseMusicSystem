using System;
using System.IO;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Un4seen.Bass;

namespace Noise.Core.MediaPlayer {
	public class AudioPlayer : IAudioPlayer {
		private readonly IDatabaseManager	mDatabase;
		private int							mCurrentStream;

		public AudioPlayer( IDatabaseManager database ) {
			mDatabase = database;

			Bass.BASS_Init( -1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero );
		}

		public bool OpenFile( StorageFile file ) {
			var retValue = false;
			var path = StorageHelpers.GetPath( mDatabase.Database, file );

			Stop();
			CloseFile();

			if( File.Exists( path ) ) {
				mCurrentStream = Bass.BASS_StreamCreateFile( path, 0, 0, BASSFlag.BASS_DEFAULT );

				retValue = true;
			}

			return ( retValue );
		}

		public bool IsOpen {
			get { return ( mCurrentStream != 0 ); }
		}

		public void Stop() {
			if( IsOpen ) {
				Bass.BASS_ChannelStop( mCurrentStream );
			}
		}

		public void CloseFile() {
			if( IsOpen ) {
				Bass.BASS_StreamFree( mCurrentStream );
			}
			mCurrentStream = 0;
		}

		public void Play() {
			if( IsOpen ) {
				Bass.BASS_ChannelPlay( mCurrentStream, false );
			}
		}

		public void Pause() {
			if( IsOpen ) {
				Bass.BASS_ChannelPause( mCurrentStream );
			}
		}

		public TimeSpan PlayPosition {
			get {
				var retValue = new TimeSpan();

				if( IsOpen ) {
					var pos = Bass.BASS_ChannelGetPosition( mCurrentStream );
					var secs = Bass.BASS_ChannelBytes2Seconds( mCurrentStream, pos );
					retValue = new TimeSpan( 0, 0, (int)secs );
				}

				return ( retValue );
			}
			set { }
		}

		public float Volume {
			get { return( Bass.BASS_GetVolume()); }
			set{ Bass.BASS_SetVolume( value ); }
		}
	}
}
