using System;
using System.Collections.Generic;
using System.IO;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Un4seen.Bass;

namespace Noise.Core.MediaPlayer {
	internal class AudioStream {
		public	int				Channel { get; private set; }
		public	StorageFile		PhysicalFile { get; private set; }
		public	bool			IsActive { get; set; }

		public AudioStream( int channel, StorageFile file ) {
			Channel = channel;
			PhysicalFile = file;
		}
	}

	public class AudioPlayer : IAudioPlayer {
		private readonly IDatabaseManager				mDatabase;
		private readonly Dictionary<int, AudioStream>	mCurrentStreams;

		public AudioPlayer( IDatabaseManager database ) {
			mDatabase = database;
			mCurrentStreams = new Dictionary<int, AudioStream>();

			Bass.BASS_Init( -1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero );
		}

		public int OpenFile( StorageFile file ) {
			var retValue = 0;
			var path = StorageHelpers.GetPath( mDatabase.Database, file );

			if( File.Exists( path )) {
				try {
					var channel = Bass.BASS_StreamCreateFile( path, 0, 0, BASSFlag.BASS_DEFAULT );

					mCurrentStreams.Add( channel, new AudioStream( channel, file ));
					retValue = channel;
				}
				catch( Exception ex ) {
					
				}
				return( retValue );
			}

			return ( retValue );
		}

		private AudioStream GetStream( int channel ) {
			AudioStream	retValue = null;

			if( mCurrentStreams.ContainsKey( channel )) {
				retValue = mCurrentStreams[channel];
			}

			return( retValue );
		}

		public void Stop( int channel ) {
			var stream = GetStream( channel );

			if( stream != null ) {
				Bass.BASS_ChannelStop( stream.Channel );
			}
		}

		public void CloseFile( int channel ) {
			var stream = GetStream( channel );

			if( stream != null ) {
				Bass.BASS_StreamFree( stream.Channel );
				mCurrentStreams.Remove( channel );
			}
		}

		public void Play( int channel ) {
			var stream = GetStream( channel );

			if( stream != null ) {
				Bass.BASS_ChannelPlay( stream.Channel, false );
			}
		}

		public void Pause( int channel ) {
			var stream = GetStream( channel );

			if( stream != null ) {
				Bass.BASS_ChannelPause( stream.Channel );
			}
		}

		public void Fade( int channel ) {
			var stream = GetStream( channel );

			if( stream != null ) {
				Bass.BASS_ChannelSlideAttribute( stream.Channel, BASSAttribute.BASS_ATTRIB_MUSIC_VOL_CHAN, 0, 500 );
			}
		}

		public TimeSpan PlayPosition( int channel ) {
			var retValue = new TimeSpan();
			var stream = GetStream( channel );

			if( stream != null ) {
				var pos = Bass.BASS_ChannelGetPosition( stream.Channel );
				var secs = Bass.BASS_ChannelBytes2Seconds( stream.Channel, pos );
				retValue = new TimeSpan( 0, 0, (int)secs );
			}

			return ( retValue );
		}

		public float Volume {
			get { return( Bass.BASS_GetVolume()); }
			set{ Bass.BASS_SetVolume( value ); }
		}
	}
}
