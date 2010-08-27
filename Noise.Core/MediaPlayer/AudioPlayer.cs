using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Timers;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Tags;
using Timer = System.Timers.Timer;

namespace Noise.Core.MediaPlayer {
	internal class AudioStream {
		public	int				Channel { get; private set; }
		public	StorageFile		PhysicalFile { get; private set; }
		public	string			Url { get; private set; }
		public	bool			IsActive { get; set; }
		public	bool			InSlide { get; set; }
		public	bool			PauseOnSlide { get; set; }
		public	bool			StopOnSlide { get; set; }
		public	bool			Faded { get; set; }
		public	BASSActive		Mode { get; set; }
		public	float			SampleRate { get; private set; }
		public	int				MetaDataSync { get; set; }

		private AudioStream( int channel, float sampleRate ) {
			Channel = channel;
			SampleRate = sampleRate;

			Mode = BASSActive.BASS_ACTIVE_STOPPED;
		}

		public AudioStream( StorageFile file, int channel, float sampleRate ) :
			this( channel, sampleRate ) {
			PhysicalFile = file;
		}

		public AudioStream( string url, int channel, float sampleRate ) :
			this( channel, sampleRate ) {
			Url = url;
		}

		public bool IsStream {
			get{ return(!String.IsNullOrWhiteSpace( Url )); }
		}
	}

	public class AudioPlayer : IAudioPlayer {
		private readonly IUnityContainer				mContainer;
		private readonly IEventAggregator				mEventAggregator;
		private readonly IDatabaseManager				mDatabase;
		private readonly ILog							mLog;
		private float									mPlaySpeed;
		private float									mPan;
		private readonly SYNCPROC						mStreamMetadataSync;
		private readonly Timer							mUpdateTimer;
		private readonly Dictionary<int, AudioStream>	mCurrentStreams;

		private readonly DOWNLOADPROC					mDownloadProc;
		private FileStream								mRecordStream;
		private byte[]									mRecordBuffer;
		private bool									mRecord;

		public AudioPlayer( IUnityContainer container ) {
			mContainer = container;
			mEventAggregator = mContainer.Resolve<IEventAggregator>();
			mDatabase = mContainer.Resolve<IDatabaseManager>();
			mLog = mContainer.Resolve<ILog>();
			mCurrentStreams = new Dictionary<int, AudioStream>();
			mStreamMetadataSync = new SYNCPROC( SyncProc );
			mUpdateTimer = new Timer { Enabled = false, Interval = 50, AutoReset = true };
			mUpdateTimer.Elapsed += OnUpdateTimer;
			mDownloadProc = new DOWNLOADPROC( RecordProc );

			try {
				Bass.BASS_Init( -1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero );
			}
			catch( Exception ex ) {
				mLog.LogException( "Bass Audio could not be initialized. ", ex);
			}

			LoadPlugin( "bassflac.dll" );
			LoadPlugin( "basswma.dll" );
			LoadPlugin( "bass_aac.dll" );

			mRecord = false;
		}

		private void LoadPlugin( string plugin ) {
			try {
			if( Bass.BASS_PluginLoad( plugin ) == 0 ) {
				mLog.LogMessage( String.Format( "Cannot load Bass Plugin: {0}", plugin ));
			}
			}
			catch( Exception ex ) {
				mLog.LogException( String.Format( "Could not load Bass plugin: {0}", plugin ), ex );
			}
		}

		public float Pan {
			get { return( mPan ); }
			set {
				mPan = value;

				SetPan();
			}
		}

		public float PlaySpeed {
			get{ return( mPlaySpeed ); }
			set{
				mPlaySpeed = value; 
				SetPlaySpeed();
			}
		}

		private void OnUpdateTimer( object sender, ElapsedEventArgs args ) {
			var streams = new List<AudioStream>( mCurrentStreams.Values );

			foreach( var stream in streams ) {
				var mode = Bass.BASS_ChannelIsActive( stream.Channel );

				if( stream.Mode != mode ) {
					stream.Mode = mode;

					mEventAggregator.GetEvent<Events.AudioPlayStatusChanged>().Publish( stream.Channel );
				}

				if( stream.InSlide ) {
					var sliding = Bass.BASS_ChannelIsSliding( stream.Channel, BASSAttribute.BASS_ATTRIB_VOL );
					
					if(!sliding ) {
						stream.InSlide = false;

						if( stream.PauseOnSlide ) {
							Bass.BASS_ChannelPause( stream.Channel );
						}
						if( stream.StopOnSlide ) {
							Bass.BASS_ChannelStop( stream.Channel );
						}
					}
				}
			}

			if( mCurrentStreams.Count == 0 ) {
				mUpdateTimer.Stop();
			}
		}

		public int OpenFile( StorageFile file ) {
			var retValue = 0;
			var path = StorageHelpers.GetPath( mDatabase.Database, file );

			if( File.Exists( path )) {
				try {
					var channel = Bass.BASS_StreamCreateFile( path, 0, 0, BASSFlag.BASS_DEFAULT );
					
					Single	sampleRate = 0;
					Bass.BASS_ChannelGetAttribute( channel, BASSAttribute.BASS_ATTRIB_FREQ, ref sampleRate );

					var stream = new AudioStream( file, channel, sampleRate );
					mCurrentStreams.Add( channel, stream );

					InitializeEq( channel );
					SetPan( stream );
					SetPlaySpeed( stream );
					retValue = channel;
				}
				catch( Exception ex ) {
					mLog.LogException( String.Format( "AudioPlayer opening {0}", path ), ex );
				}
			}

			if(( mCurrentStreams.Count > 0 ) &&
			   (!mUpdateTimer.Enabled )) {
				mUpdateTimer.Start();
			}

			return ( retValue );
		}

		public int OpenStream( DbInternetStream stream ) {
			var retValue = 0;

			if(!String.IsNullOrWhiteSpace( stream.Url )) {
				try {
					if(!Bass.BASS_SetConfig( BASSConfig.BASS_CONFIG_NET_PLAYLIST, stream.IsPlaylistWrapped ? 1 : 0 )) {
						mLog.LogMessage( String.Format( "AudioPlayer - OpenStream cannot set _CONFIG_NET_PLAYLIST for: {0}", stream.Url ));
					}

					var channel = Bass.BASS_StreamCreateURL( stream.Url, 0, BASSFlag.BASS_DEFAULT, mDownloadProc, IntPtr.Zero );
					if( channel != 0 ) {
						Single	sampleRate = 0;
						Bass.BASS_ChannelGetAttribute( channel, BASSAttribute.BASS_ATTRIB_FREQ, ref sampleRate );

						var audioStream = new AudioStream( stream.Url, channel, sampleRate ) {
													MetaDataSync = Bass.BASS_ChannelSetSync( channel, BASSSync.BASS_SYNC_META, 0, mStreamMetadataSync, IntPtr.Zero ) };
						mCurrentStreams.Add( channel, audioStream );

						InitializeEq( channel );
						SetPan( audioStream );
						SetPlaySpeed( audioStream );
						retValue = channel;

						if(( mCurrentStreams.Count > 0 ) &&
						   (!mUpdateTimer.Enabled )) {
							mUpdateTimer.Start();
						}
					}
					else {
						var errorCode = Bass.BASS_ErrorGetCode();

						mLog.LogMessage( String.Format( "AudioPlayer OpenUrl failed: {0}", errorCode ));
					}
				}
				catch( Exception ex ) {
					mLog.LogException( String.Format( "AudioPlayer opening url: {0}", stream.Url ), ex );
				}
			}

			return( retValue );
		}

		private void SyncProc( int handle, int channel, int data, IntPtr user ) {
			PublishStreamInfo( channel );
		}

		private void PublishStreamInfo( int channel ) {
			if( mCurrentStreams.ContainsKey( channel )) {
				var audioStream = mCurrentStreams[channel];

				if( audioStream != null ) {
					var tagInfo = new TAG_INFO( audioStream.Url );
					if( BassTags.BASS_TAG_GetFromURL( channel, tagInfo )) {
						mEventAggregator.GetEvent<Events.AudioPlayStreamInfo>().Publish( new StreamInfo( channel, tagInfo.artist, tagInfo.album, 
																										 tagInfo.title, tagInfo.genre ) );
					}
				}
			}
		}

		private void RecordProc( IntPtr buffer, int length, IntPtr user ) {
			if( mRecord ) {
				if( mRecordStream == null ) {
					mRecordStream = new FileStream( "recording.mp3", FileMode.Create, FileAccess.Write, FileShare.Read );
				}

				if( buffer == IntPtr.Zero ) {
					// finished downloading
					mRecordStream.Flush();
					mRecordStream.Close();

					mRecordStream = null;
				}
				else {
					// size the data buffer as needed
					if(( mRecordBuffer == null ) ||
					   ( mRecordBuffer.Length < length )) {
						mRecordBuffer = new byte[length];
					}
					Marshal.Copy( buffer, mRecordBuffer, 0, length );

					mRecordStream.Write( mRecordBuffer, 0, length );
				}
			}
		}

		private static void InitializeEq( int channel ) {
/*			int[] fxChannels = {0, 0, 0};

			// 3-band EQ
			var eq = new BASS_DX8_PARAMEQ();

			fxChannels[0] = Bass.BASS_ChannelSetFX( channel, BASSFXType.BASS_FX_DX8_PARAMEQ, 0 );
			fxChannels[1] = Bass.BASS_ChannelSetFX( channel, BASSFXType.BASS_FX_DX8_PARAMEQ, 0 );
			fxChannels[2] = Bass.BASS_ChannelSetFX( channel, BASSFXType.BASS_FX_DX8_PARAMEQ, 0 );

			eq.fBandwidth = 18f;
			eq.fCenter = 100f;
			eq.fGain = -10f;

			Bass.BASS_FXSetParameters( fxChannels[0], eq );
			eq.fCenter = 1000f;
			Bass.BASS_FXSetParameters( fxChannels[1], eq );

			eq.fCenter = 8000f;
			eq.fGain = 10F;
			Bass.BASS_FXSetParameters( fxChannels[2], eq );
*/		}

		private void SetPan() {
			foreach( var stream in mCurrentStreams.Values ) {
				SetPan( stream );
			}
		}

		private void SetPan( AudioStream stream ) {
			Bass.BASS_ChannelSetAttribute( stream.Channel, BASSAttribute.BASS_ATTRIB_PAN, mPan );
		}

		private void SetPlaySpeed() {
			foreach( var stream in mCurrentStreams.Values ) {
				SetPlaySpeed( stream );
			}
		}

		private void SetPlaySpeed( AudioStream stream ) {
			if( stream.SampleRate > 0 ) {
				float	multiplier = 1.0f + ( mPlaySpeed * 0.5f );

				Bass.BASS_ChannelSetAttribute( stream.Channel, BASSAttribute.BASS_ATTRIB_FREQ, stream.SampleRate * multiplier );
			}
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

		public void CloseChannel( int channel ) {
			var stream = GetStream( channel );

			if( stream != null ) {
				if( stream.MetaDataSync != 0 ) {
					Bass.BASS_ChannelRemoveSync( channel, stream.MetaDataSync );
				}
				Bass.BASS_StreamFree( stream.Channel );

				mCurrentStreams.Remove( channel );
			}
		}

		public void Play( int channel ) {
			var stream = GetStream( channel );

			if( stream != null ) {
				if( stream.Faded ) {
					stream.Faded = false;

					Bass.BASS_ChannelSlideAttribute( stream.Channel, BASSAttribute.BASS_ATTRIB_VOL, 1.0f, 1000 );
				}

				Bass.BASS_ChannelPlay( stream.Channel, false );

				if( stream.IsStream ) {
					PublishStreamInfo( stream.Channel );
				}
			}
		}

		public void Pause( int channel ) {
			var stream = GetStream( channel );

			if( stream != null ) {
				Bass.BASS_ChannelPause( stream.Channel );
			}
		}

		public void FadeAndPause( int channel ) {
			var stream = GetStream( channel );

			if( stream != null ) {
				Bass.BASS_ChannelSlideAttribute( stream.Channel, BASSAttribute.BASS_ATTRIB_VOL, 0.0f, 500 );

				stream.Faded = true;
				stream.PauseOnSlide = true;
				stream.InSlide = true;
			}
		}

		public void FadeAndStop( int channel ) {
			var	stream = GetStream( channel );

			if( stream != null ) {
				Bass.BASS_ChannelSlideAttribute( stream.Channel, BASSAttribute.BASS_ATTRIB_VOL, 0.0f, 500 );

				stream.Faded = true;
				stream.StopOnSlide = true;
				stream.InSlide = true;
			}
		}

		public ePlaybackStatus GetChannelStatus( int channel ) {
			var retValue = ePlaybackStatus.Unknown;
			var stream = GetStream( channel );

			if(stream != null ) {
				switch( stream.Mode ) {
					case BASSActive.BASS_ACTIVE_STOPPED:
						retValue = ePlaybackStatus.Stopped;
						break;

					case BASSActive.BASS_ACTIVE_PAUSED:
						retValue = ePlaybackStatus.Paused;
						break;

					case BASSActive.BASS_ACTIVE_PLAYING:
						retValue = ePlaybackStatus.Playing;
						break;
				}
			}

			return( retValue );
		}

		public TimeSpan GetLength( int channel ) {
			var length = Bass.BASS_ChannelGetLength( channel );
			var seconds = Bass.BASS_ChannelBytes2Seconds( channel, length );
			var	retValue = new TimeSpan( 0, 0,0, (int)seconds );

			return( retValue );
		}

		public TimeSpan GetPlayPosition( int channel ) {
			var retValue = new TimeSpan();
			var stream = GetStream( channel );

			if( stream != null ) {
				var pos = Bass.BASS_ChannelGetPosition( stream.Channel );
				var secs = Bass.BASS_ChannelBytes2Seconds( stream.Channel, pos );

				retValue = new TimeSpan( 0, 0, (int)secs );
			}

			return ( retValue );
		}

		public void SetPlayPosition( int channel, TimeSpan position ) {
			var stream = GetStream( channel );

			if( stream != null ) {
				Bass.BASS_ChannelSetPosition( stream.Channel, position.TotalSeconds );
			}
		}

		public double GetPercentPlayed( int channel ) {
			var retValue = 0.0;
			var stream = GetStream( channel );

			if( stream != null ) {
				var length = Bass.BASS_ChannelGetLength( channel );
				var position = Bass.BASS_ChannelGetPosition( channel );

				if(( length > 0 ) &&
				   ( position > 0 )) {
					retValue = (double)position / length;
				}
			}

			return( retValue );
		}

		public AudioLevels GetSampleLevels( int channel ) {
			AudioLevels	retValue = null;

			var stream = GetStream( channel );

			if( stream != null ) {
				if( stream.Mode == BASSActive.BASS_ACTIVE_PLAYING ) {
					var levels = Bass.BASS_ChannelGetLevel( channel );
					var leftLevel = Utils.LowWord32( levels ) > 0.0 ? (double)Utils.LowWord32( levels ) / 32768 : Utils.LowWord32( levels );
					var rightLevel = Utils.HighWord32( levels ) > 0.0 ? (double)Utils.HighWord32( levels ) / 32768 : Utils.HighWord32( levels );


					retValue = new AudioLevels( leftLevel, rightLevel );
				}
				else {
					retValue = new AudioLevels( 0.0, 0.0 );
				}
			}

			return( retValue );
		}

		public float Volume {
			get { return( Bass.BASS_GetVolume()); }
			set{ Bass.BASS_SetVolume( value ); }
		}
	}
}
