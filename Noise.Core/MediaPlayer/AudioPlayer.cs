using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Timers;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Core.Database;
using Noise.Core.FileStore;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Un4seen.Bass;
using Timer = System.Timers.Timer;

namespace Noise.Core.MediaPlayer {
	internal class AudioStream {
		public	int				Channel { get; private set; }
		public	StorageFile		PhysicalFile { get; private set; }
		public	bool			IsActive { get; set; }
		public	bool			InSlide { get; set; }
		public	bool			PauseOnSlide { get; set; }
		public	bool			StopOnSlide { get; set; }
		public	bool			Faded { get; set; }
		public	BASSActive		Mode { get; set; }

		public AudioStream( int channel, StorageFile file ) {
			Channel = channel;
			PhysicalFile = file;

			Mode = BASSActive.BASS_ACTIVE_STOPPED;
		}
	}

	public class AudioPlayer : IAudioPlayer {
		private readonly IUnityContainer				mContainer;
		private readonly IEventAggregator				mEventAggregator;
		private readonly IDatabaseManager				mDatabase;
		private readonly Timer							mUpdateTimer;
		private readonly Dictionary<int, AudioStream>	mCurrentStreams;

		public AudioPlayer( IUnityContainer container ) {
			mContainer = container;
			mEventAggregator = mContainer.Resolve<IEventAggregator>();
			mDatabase = mContainer.Resolve<IDatabaseManager>();
			mCurrentStreams = new Dictionary<int, AudioStream>();
			mUpdateTimer = new Timer { Enabled = false, Interval = 50, AutoReset = true };
			mUpdateTimer.Elapsed += OnUpdateTimer;

			Bass.BASS_Init( -1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero );
		}

		private void OnUpdateTimer( object sender, ElapsedEventArgs args ) {
			var shouldStopTimer = true;
			var streams = new List<AudioStream>( mCurrentStreams.Values );

			foreach( var stream in streams ) {
				var mode = Bass.BASS_ChannelIsActive( stream.Channel );

				if( stream.Mode != mode ) {
					stream.Mode = mode;

					mEventAggregator.GetEvent<Events.AudioPlayStatusChanged>().Publish( stream.Channel );
				}

				if( stream.Mode != BASSActive.BASS_ACTIVE_STOPPED ) {
					shouldStopTimer = false;
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

			if( shouldStopTimer ) {
				mUpdateTimer.Stop();
			}
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
				if( stream.Faded ) {
					stream.Faded = false;

					Bass.BASS_ChannelSlideAttribute( stream.Channel, BASSAttribute.BASS_ATTRIB_VOL, 1.0f, 1000 );
				}

				Bass.BASS_ChannelPlay( stream.Channel, false );

				mUpdateTimer.Start();
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

		public ePlayingChannelStatus GetChannelStatus( int channel ) {
			var retValue = ePlayingChannelStatus.Unknown;
			var stream = GetStream( channel );

			if(stream != null ) {
				switch( stream.Mode ) {
					case BASSActive.BASS_ACTIVE_STOPPED:
						retValue = ePlayingChannelStatus.Stopped;
						break;

					case BASSActive.BASS_ACTIVE_PAUSED:
						retValue = ePlayingChannelStatus.Paused;
						break;

					case BASSActive.BASS_ACTIVE_PLAYING:
						retValue = ePlayingChannelStatus.Playing;
						break;
				}
			}

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

		public float Volume {
			get { return( Bass.BASS_GetVolume()); }
			set{ Bass.BASS_SetVolume( value ); }
		}
	}
}
