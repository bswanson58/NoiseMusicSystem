using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Noise.AudioSupport.Logging;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.AddOn.Tags;
using Un4seen.Bass.Misc;

namespace Noise.AudioSupport.Player {
	public class AudioPlayer : IAudioPlayer {
		private const double							ComparisonEpsilon = 0.1;

		private readonly ILogAudioPlay					mLog;
		private int										mMixerChannel;
		private	float									mMixerSampleRate;
		private float									mPlaySpeed;
		private float									mPan;
		private float									mPreampVolume;
		private readonly SYNCPROC						mStreamMetadataSync;
		private readonly SYNCPROC						mPlayEndSyncProc;
		private readonly SYNCPROC						mPlayRequestSyncProc;
		private	readonly SYNCPROC						mPlayStalledSyncProc;
		private readonly SYNCPROC						mSlideSyncProc;
		private readonly SYNCPROC						mQueuedTrackPlaySync;
		private readonly Dictionary<int, AudioStream>	mCurrentStreams;
		private DSP_PeakLevelMeter						mLevelMeter;
		private DSP_StereoEnhancer						mStereoEnhancer;
		private DSP_SoftSaturation						mSoftSaturation;
		private ParametricEqualizer						mEq;
		private int										mMuteFx;
		private bool									mMuted;
		private	int										mPreampFx;
		private	int										mParamEqFx;
		private int										mReverbFx;
		private	readonly Dictionary<long, int>			mEqChannels;
		private	bool									mEqEnabled;
		private readonly DOWNLOADPROC					mDownloadProc;
		private FileStream								mRecordStream;
		private byte[]									mRecordBuffer;
		private readonly bool							mRecord;	// recording is currently not implemented.
		private int										mTrackOverlapMs;
		private int										mQueuedChannel;
		private readonly Visuals						mSpectrumVisual;
		private readonly List<AudioDevice>				mDeviceList;
		private int										mCurrentDevice;

		// Variables used to store parameter values before channel is created.
		private bool									mStereoEnhancerEnable;
		private double									mStereoEnhancerWetDry;
		private double									mStereoEnhancerWidth;
		private bool									mSoftSaturationEnable;
		private double									mSoftSaturationDepth;
		private double									mSoftSaturationFactor;
		private bool									mReverbEnable;
		private float									mReverbLevel;
		private float									mReverbDelay;


		private readonly Subject<AudioChannelStatus>	mChannelStatusSubject;
		public	IObservable<AudioChannelStatus>			ChannelStatusChange => mChannelStatusSubject.AsObservable();

        private readonly Subject<AudioLevels>			mAudioLevelsSubject;
		public	IObservable<AudioLevels>				AudioLevelsChange => mAudioLevelsSubject.AsObservable();

        private readonly Subject<StreamInfo>			mAudioStreamInfoSubject;
		public	IObservable<StreamInfo>					AudioStreamInfoChange => mAudioStreamInfoSubject.AsObservable();

        public	bool									TrackOverlapEnable { get; set; }

		public AudioPlayer( ILicenseManager licenseManager, ILogAudioPlay log ) {
			mLog = log;
			mCurrentStreams = new Dictionary<int, AudioStream>();
			mEqChannels = new Dictionary<long, int>();
			mStreamMetadataSync = StreamSyncProc;
			mPlayEndSyncProc = PlayEndSyncProc;
			mPlayRequestSyncProc = PlayRequestSyncProc;
			mPlayStalledSyncProc = PlayStallSyncProc;
			mQueuedTrackPlaySync = PlayQueuedTrackSyncProc;
			mSlideSyncProc = SlideSyncProc;
			mDownloadProc = RecordProc;
			mSpectrumVisual = new Visuals();
			mDeviceList = new List<AudioDevice>();
			mCurrentDevice = -1;

			mReverbDelay = 0.1f;
			mReverbLevel = 0.3f;

			mTrackOverlapMs = 100;

			try {
				var key = licenseManager.RetrieveKey( LicenseKeys.BassAudio );

				if( key != null ) {
					BassNet.Registration( key.Name, key.Key );
				}

				InitializeDevices();
				InitializeWithDefaultDevice();

			}
			catch( Exception ex ) {
				mLog.LogException( "Bass Audio could not be initialized", ex );
			}

			LoadPlugin( "bassflac.dll" );
			LoadPlugin( "basswma.dll" );
			LoadPlugin( "bass_aac.dll" );

			mPreampVolume = 1.0f;
			mRecord = false;

			mChannelStatusSubject = new Subject<AudioChannelStatus>();
			mAudioLevelsSubject = new Subject<AudioLevels>();
			mAudioStreamInfoSubject = new Subject<StreamInfo>();
		}

		private void LoadPlugin( string plugin ) {
			try {
				if( Bass.BASS_PluginLoad( plugin ) == 0 ) {
					mLog.LogPluginLoadFailed( plugin, (int)Bass.BASS_ErrorGetCode());
				}
				else {
					mLog.LogPluginLoaded( plugin );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( $"Could not load plugin \"{plugin}\"", ex );
			}
		}

		private void InitializeDevices() {
			mDeviceList.Clear();

			var deviceCount = Bass.BASS_GetDeviceCount();

			for( var device = 0; device < deviceCount; device++ ) {
				var deviceInfo = Bass.BASS_GetDeviceInfo( device );

				if( deviceInfo != null ) {
					mDeviceList.Add( new AudioDevice { DeviceId = device, Name = deviceInfo.name,
													   IsDefault = deviceInfo.IsDefault, IsEnabled = deviceInfo.IsEnabled });
				}
				else {
					mLog.LogErrorCode( "Could not get device info", (int)Bass.BASS_ErrorGetCode());
				}
			}
		}

		public IEnumerable<AudioDevice> GetDeviceList() {
			InitializeDevices();

			return( mDeviceList );
		}

		public AudioDevice GetCurrentDevice() {
			return( mDeviceList.FirstOrDefault( device => device.DeviceId == mCurrentDevice ) ?? new AudioDevice());
		}

		public void SetDevice( AudioDevice device ) {
			if(( device != null ) &&
			   ( device.DeviceId != mCurrentDevice )) {
				mLog.LogSetOutputDevice( device );

				InitializeWithDevice( device.DeviceId );
			}
		}

		private void InitializeWithDefaultDevice() {
			if( mDeviceList.Any()) {
				SetDevice( mDeviceList.FirstOrDefault( device => device.IsDefault ));

				StereoEnhancerEnable = mStereoEnhancerEnable;
				StereoEnhancerWetDry = mStereoEnhancerWetDry;
				StereoEnhancerWidth = mStereoEnhancerWidth;
			}
		}

		private void InitializeWithDevice( int deviceId ) {
			if( mCurrentStreams.Any()) {
				var channelList = mCurrentStreams.Keys.ToArray();

				foreach( var channel in channelList ) {
					Stop( channel );					
					CloseChannel( channel );
				}
			}

			if( mCurrentDevice != -1 ) {
				Bass.BASS_Free();

				mMixerChannel = 0;
				mMuteFx = 0;

				mStereoEnhancer = null;
				mSoftSaturation = null;
				mLevelMeter = null;
			}

			if( Bass.BASS_Init( deviceId, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero )) {
				mCurrentDevice = deviceId;

				if( Bass.BASS_SetConfig( BASSConfig.BASS_CONFIG_FLOATDSP, true )) {
					// Load the fx library.
					BassFx.BASS_FX_GetVersion();

					mMixerChannel = BassMix.BASS_Mixer_StreamCreate( 44100, 2, BASSFlag.BASS_SAMPLE_FLOAT );
					if( mMixerChannel != 0 ) {
						Bass.BASS_ChannelPlay( mMixerChannel, false );
						Bass.BASS_ChannelGetAttribute( mMixerChannel, BASSAttribute.BASS_ATTRIB_FREQ, ref mMixerSampleRate );

						mMuteFx = Bass.BASS_ChannelSetFX( mMixerChannel, BASSFXType.BASS_FX_BFX_VOLUME, 3 );
						mStereoEnhancer = new DSP_StereoEnhancer( mMixerChannel, 4 );
						mSoftSaturation = new DSP_SoftSaturation( mMixerChannel, 5 );

						mLevelMeter = new DSP_PeakLevelMeter( mMixerChannel, -20 );
						mLevelMeter.Notification += OnLevelMeter;

						InitializeAudioParameters();
					}
					else {
						mLog.LogErrorCode( "Mixer channel could not be created", (int)Bass.BASS_ErrorGetCode());
					}
				}
				else {
					mLog.LogErrorCode( "Could not set configuration", (int)Bass.BASS_ErrorGetCode());
				}
			}
			else {
				mLog.LogErrorCode( "Could not initialize with device", (int)Bass.BASS_ErrorGetCode());
			}
		}

		private void InitializeAudioParameters() {
			if( mMixerChannel != 0 ) {
				InitializePreamp();

				SoftSaturationEnable = mSoftSaturationEnable;
				SoftSaturationDepth = mSoftSaturationDepth;
				SoftSaturationFactor = mSoftSaturationFactor;

				StereoEnhancerEnable = mStereoEnhancerEnable;
				StereoEnhancerWetDry = mStereoEnhancerWetDry;
				StereoEnhancerWidth = mStereoEnhancerWidth;

				ReverbEnable = mReverbEnable;
				ReverbDelay = mReverbDelay;
				ReverbLevel = mReverbLevel;
			}
		}

		public int OpenFile( string filePath ) {
			return( OpenFile( filePath, 0.0f ));
		}

        public int OpenFile( string  filePath, float gainAdjustment ) {
			return OpenFile( filePath, gainAdjustment, 0.0, 0.0  );
        }

		public int OpenFile( string  filePath, float gainAdjustment, double fadeInPoint, double fadeOutPoint ) {
			var retValue = 0;

			if( File.Exists( filePath )) {
				try {
					var channel = Bass.BASS_StreamCreateFile( filePath, 0, 0,
															  BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN );
					if( channel != 0 ) {
						if(!BassMix.BASS_Mixer_StreamAddChannel( mMixerChannel, channel,
																 BASSFlag.BASS_MIXER_NORAMPIN | BASSFlag.BASS_MIXER_PAUSE | BASSFlag.BASS_MIXER_DOWNMIX | BASSFlag.BASS_STREAM_AUTOFREE )) {
							mLog.LogErrorCode( "Stream could not be added to mixer", (int)Bass.BASS_ErrorGetCode());
						}
					
						var stream = new AudioStream( filePath, channel );
						mCurrentStreams.Add( channel, stream );

						if( Math.Abs( gainAdjustment - 0.0f ) > 0.01f ) {
							stream.ReplayGainFx = Bass.BASS_ChannelSetFX( channel, BASSFXType.BASS_FX_BFX_VOLUME, 2 );
							if( stream.ReplayGainFx != 0 ) {
								 // convert the replay gain dB gain to a linear value
								var volParam = new BASS_BFX_VOLUME { lChannel = BASSFXChan.BASS_BFX_CHANALL,
																	 fVolume = (float)Math.Pow( 10, gainAdjustment / 20 )};
								if(!Bass.BASS_FXSetParameters( stream.ReplayGainFx, volParam )) {
									mLog.LogErrorCode( "Could not set replay gain volume", (int)Bass.BASS_ErrorGetCode());
								}
							}
							else {
								mLog.LogErrorCode( "Could not set replay gain preamp", (int)Bass.BASS_ErrorGetCode());
							}
						}

						stream.SyncEnd = BassMix.BASS_Mixer_ChannelSetSync( stream.Channel, BASSSync.BASS_SYNC_END, 0L, mPlayEndSyncProc, IntPtr.Zero );
						if( stream.SyncEnd == 0 ) {
							mLog.LogErrorCode( "Could not set end sync", (int)Bass.BASS_ErrorGetCode());
						}

						stream.SyncStalled = BassMix.BASS_Mixer_ChannelSetSync( stream.Channel, BASSSync.BASS_SYNC_STALL, 0L, mPlayStalledSyncProc, IntPtr.Zero );
						if( stream.SyncStalled == 0 ) {
							mLog.LogErrorCode( "Could not set stall sync", (int)Bass.BASS_ErrorGetCode());
						}

						if( TrackOverlapEnable ) {
							var trackLength = Bass.BASS_ChannelGetLength( stream.Channel );
							var fadeOutPosition = Math.Abs( fadeOutPoint ) > ComparisonEpsilon ? fadeOutPoint : 3.0;
							var position = trackLength - Bass.BASS_ChannelSeconds2Bytes( stream.Channel, fadeOutPosition );

							if( position > 0 ) {
								stream.SyncNext = BassMix.BASS_Mixer_ChannelSetSync( stream.Channel, BASSSync.BASS_SYNC_POS | BASSSync.BASS_SYNC_ONETIME,
																					 position, mPlayRequestSyncProc, IntPtr.Zero );
								if( stream.SyncNext == 0 ) {
									mLog.LogErrorCode( "Could not set request sync", (int)Bass.BASS_ErrorGetCode());
								}

								position = trackLength - Bass.BASS_ChannelSeconds2Bytes( stream.Channel, mTrackOverlapMs / 1000.0 );
								stream.SyncQueued = BassMix.BASS_Mixer_ChannelSetSync( stream.Channel, BASSSync.BASS_SYNC_POS | BASSSync.BASS_SYNC_ONETIME,
																					   position, mQueuedTrackPlaySync, IntPtr.Zero );
								if( stream.SyncQueued == 0 ) {
									mLog.LogErrorCode( "Could not set queued track sync", (int)Bass.BASS_ErrorGetCode());
								}
							}
						}

						if( Math.Abs( fadeInPoint ) > ComparisonEpsilon ) {
							if(!Bass.BASS_ChannelSetPosition( stream.Channel, fadeInPoint )) {
								mLog.LogErrorCode( "Could not set the channel position to the fade in point.", (int)Bass.BASS_ErrorGetCode());
                            }
                        }

						retValue = channel;
						mLog.LogChannelOpen( channel, filePath );
					}
					else {
						mLog.LogErrorCode( $"Channel could not be created for \"{filePath}\"", (int)Bass.BASS_ErrorGetCode());
					}
				}
				catch( Exception ex ) {
					mLog.LogException( $"Opening \"{filePath}\"", ex );
				}
			}

			return ( retValue );
		}

		public int OpenStream( DbInternetStream stream ) {
			var retValue = 0;

			if(!String.IsNullOrWhiteSpace( stream.Url )) {
				try {
					if(!Bass.BASS_SetConfig( BASSConfig.BASS_CONFIG_NET_PLAYLIST, stream.IsPlaylistWrapped ? 1 : 0 )) {
						mLog.LogErrorCode( $"Stream configuration could not be set \"{stream.Url}\"", (int)Bass.BASS_ErrorGetCode());
					}

					var channel = Bass.BASS_StreamCreateURL( stream.Url, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN,
															 mDownloadProc, IntPtr.Zero );
					if( channel != 0 ) {
						if(!BassMix.BASS_Mixer_StreamAddChannel( mMixerChannel, channel,
																 BASSFlag.BASS_MIXER_NORAMPIN | BASSFlag.BASS_MIXER_PAUSE | BASSFlag.BASS_MIXER_DOWNMIX | BASSFlag.BASS_STREAM_AUTOFREE )) {
							mLog.LogErrorCode( $"Stream could not be added to mixer \"{stream.Url}\"", (int)Bass.BASS_ErrorGetCode());
						}

						var audioStream = new AudioStream( stream.Url, channel ) {
													MetaDataSync = Bass.BASS_ChannelSetSync( channel, BASSSync.BASS_SYNC_META, 0, mStreamMetadataSync, IntPtr.Zero ) };
						mCurrentStreams.Add( channel, audioStream );

						retValue = channel;
						mLog.LogChannelOpen( channel, stream.Url );
					}
					else {
						mLog.LogErrorCode( $"Open stream failed \"{stream.Url}\"", (int)Bass.BASS_ErrorGetCode());
					}
				}
				catch( Exception ex ) {
					mLog.LogException( $"Open stream failed \"{stream.Url}\"", ex );
				}
			}

			return( retValue );
		}

		public void QueueNextChannel( int channel ) {
			if( mCurrentStreams.ContainsKey( channel )) {
				mQueuedChannel = channel;
			}
		}

		private void SetChannelStatus( AudioChannelStatus status ) {
			mChannelStatusSubject.OnNext( status );
			mLog.LogChannelStatus( status );
		}

		private void PlayEndSyncProc( int handle, int channel, int data, IntPtr user ) {
			SetChannelStatus( new AudioChannelStatus( channel, ePlaybackStatus.TrackEnd ));

			StartQueuedTrack();
		}

		private void PlayQueuedTrackSyncProc( int handle, int channel, int data, IntPtr user ) {
			StartQueuedTrack();
		}

		private void StartQueuedTrack() {
			if( mQueuedChannel != 0 ) {
				var nextChannel = mQueuedChannel;
				mQueuedChannel = 0;

				Play( nextChannel );
			}
		}

		private void PlayRequestSyncProc( int handle, int channel, int data, IntPtr user ) {
			SetChannelStatus( new AudioChannelStatus( channel, ePlaybackStatus.RequestNext ));
		}

		private void PlayStallSyncProc( int handle, int channel, int data, IntPtr user ) {
			SetChannelStatus( new AudioChannelStatus( channel, ePlaybackStatus.TrackEnd ));
		}

		private void SlideSyncProc( int handle, int channel, int data, IntPtr user ) {
			var stream = GetStream( channel );

			if( stream != null ) {
				if( stream.PauseOnSlide ) {
					Pause( stream.Channel  );
				}
				if( stream.StopOnSlide ) {
					Stop( stream.Channel );
				}
			}
		}

		private void StreamSyncProc( int handle, int channel, int data, IntPtr user ) {
			PublishStreamInfo( channel );
		}

		private void PublishStreamInfo( int channel ) {
			if( mCurrentStreams.ContainsKey( channel )) {
				var audioStream = mCurrentStreams[channel];

				if( audioStream != null ) {
					var tagInfo = new TAG_INFO( audioStream.Url );
					if( BassTags.BASS_TAG_GetFromURL( channel, tagInfo )) {
						mAudioStreamInfoSubject.OnNext( new StreamInfo( channel, tagInfo.artist, tagInfo.album, tagInfo.title, tagInfo.genre ));
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

		private AudioStream GetStream( int channel ) {
			AudioStream	retValue = null;

			if( mCurrentStreams.ContainsKey( channel )) {
				retValue = mCurrentStreams[channel];
			}

			return( retValue );
		}

		public void Play( int channel ) {
			var stream = GetStream( channel );

			if( stream != null ) {
				if( stream.Faded ) {
					stream.Faded = false;

					Bass.BASS_ChannelSlideAttribute( stream.Channel, BASSAttribute.BASS_ATTRIB_VOL, 1.0f, 1000 );
				}

				BassMix.BASS_Mixer_ChannelPlay( stream.Channel );
				SetChannelStatus( new AudioChannelStatus( channel, ePlaybackStatus.TrackStart ));

				if( stream.IsStream ) {
					PublishStreamInfo( stream.Channel );
				}

				mLog.LogChannelPlay( channel );
			}
		}

		public void Pause( int channel ) {
			var stream = GetStream( channel );

			if( stream != null ) {
				BassMix.BASS_Mixer_ChannelPause( stream.Channel );

				SetChannelStatus( new AudioChannelStatus( stream.Channel, ePlaybackStatus.Paused ));
			}
		}

		public void FadeAndPause( int channel ) {
			var stream = GetStream( channel );

			if( stream != null ) {
				if( GetChannelStatus( channel ) == ePlaybackStatus.TrackStart ) {
					BassMix.BASS_Mixer_ChannelSetSync( stream.Channel, BASSSync.BASS_SYNC_SLIDE | BASSSync.BASS_SYNC_ONETIME, 0L, mSlideSyncProc, IntPtr.Zero );
					Bass.BASS_ChannelSlideAttribute( stream.Channel, BASSAttribute.BASS_ATTRIB_VOL, 0.0f, 500 );

					stream.Faded = true;
					stream.PauseOnSlide = true;

					stream.InSlide = true;
				}
				else {
					Pause( channel );
				}
			}
		}

		public void Stop( int channel ) {
			var stream = GetStream( channel );

			if( stream != null ) {
				Bass.BASS_ChannelStop( stream.Channel );

				SetChannelStatus( new AudioChannelStatus( channel, ePlaybackStatus.TrackEnd ));
			}
		}

		public void FadeAndStop( int channel ) {
			var	stream = GetStream( channel );

			if( stream != null ) {
				if( GetChannelStatus( channel ) == ePlaybackStatus.TrackStart ) {
					BassMix.BASS_Mixer_ChannelSetSync( stream.Channel, BASSSync.BASS_SYNC_SLIDE | BASSSync.BASS_SYNC_ONETIME, 0L, mSlideSyncProc, IntPtr.Zero );
					Bass.BASS_ChannelSlideAttribute( stream.Channel, BASSAttribute.BASS_ATTRIB_VOL, 0.0f, 500 );

					stream.Faded = true;
					stream.StopOnSlide = true;
					stream.InSlide = true;
				}
				else {
					Stop( channel );
				}
			}
		}

		public void CloseChannel( int channel ) {
			var stream = GetStream( channel );

			if( stream != null ) {
				BassMix.BASS_Mixer_ChannelRemove( channel );

				if( stream.MetaDataSync != 0 ) {
					Bass.BASS_ChannelRemoveSync( channel, stream.MetaDataSync );
					stream.MetaDataSync = 0;
				}
				if( stream.ReplayGainFx != 0 ) {
					Bass.BASS_ChannelRemoveFX( stream.Channel, stream.ReplayGainFx );
					stream.ReplayGainFx = 0;
				}

				Bass.BASS_StreamFree( stream.Channel );

				mCurrentStreams.Remove( channel );
				mLog.LogChannelClose( channel );
			}
		}

		public ePlaybackStatus GetChannelStatus( int channel ) {
			var retValue = ePlaybackStatus.Unknown;
			var stream = GetStream( channel );

			if(stream != null ) {
				var mode = BassMix.BASS_Mixer_ChannelIsActive( channel );

				switch( mode ) {
					case BASSActive.BASS_ACTIVE_STOPPED:
						retValue = ePlaybackStatus.Stopped;
						break;

					case BASSActive.BASS_ACTIVE_PAUSED:
					case BASSActive.BASS_ACTIVE_STALLED:
						retValue = ePlaybackStatus.Paused;
						break;

					case BASSActive.BASS_ACTIVE_PLAYING:
						retValue = ePlaybackStatus.TrackStart;
						break;
				}
			}

			return( retValue );
		}

		public int TrackOverlapMilliseconds {
			get => mTrackOverlapMs;
            set {
				if(( value >= 50 ) &&
				   ( value <= 2000 )) {
					mTrackOverlapMs = value;
				}
			}
		}

		public ParametricEqualizer ParametricEq {
			get => mEq;
            set{
				mEq = value;

				if( mEq != null ) {
					if( mEqEnabled ) {
						RemoveEq();
						InitializeEq();
					}
				}
				else {
					RemoveEq();
				}
			}
		}

		public bool EqEnabled {
			get => mEqEnabled;
            set {
				if( mEq != null ) {
					mEqEnabled = value;

					if( mEqEnabled ) {
						InitializeEq();
					}
					else {
						RemoveEq();
					}
				}
				else {
					mEqEnabled = false;
				}
			}
		}

		private void InitializeEq() {
			if(( mEq != null ) &&
			   ( mEqEnabled )) {
				try {
					mParamEqFx = Bass.BASS_ChannelSetFX( mMixerChannel, BASSFXType.BASS_FX_BFX_PEAKEQ, 1 );
					if( mParamEqFx == 0 ) {
						mLog.LogErrorCode( "Could not set EQ channel", (int)Bass.BASS_ErrorGetCode());
					}

					var eq = new BASS_BFX_PEAKEQ { fQ = 0f, fBandwidth = mEq.Bandwidth / 12, lChannel = BASSFXChan.BASS_BFX_CHANALL };

					int bandId = 0;
					foreach( var band in mEq.Bands ) {
						eq.lBand = bandId;
						eq.fCenter = band.CenterFrequency;
						eq.fGain = band.Gain;
						if(!Bass.BASS_FXSetParameters( mParamEqFx, eq )) {
							mLog.LogErrorCode( "Could not set EQ Band", (int)Bass.BASS_ErrorGetCode());
						}

						mEqChannels.Add( band.BandId, bandId );

						bandId++;
					}
				}
				catch( Exception ex ) {
					mLog.LogException( "Setting EQ", ex );
				}
			}
		}

		private void RemoveEq() {
			try {
				if(( mMixerChannel != 0 ) &&
				   ( mParamEqFx != 0 )) {
					if(!Bass.BASS_ChannelRemoveFX( mMixerChannel, mParamEqFx )) {
						mLog.LogErrorCode( "Could not remove EQ", (int)Bass.BASS_ErrorGetCode());
					}
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Removing EQ channel", ex );
			}

			mParamEqFx = 0;
			mEqChannels.Clear();
		}

		public void AdjustEq( long bandId, float gain ) {
			if( mEq != null ) {
				mEq.AdjustEq( bandId, gain );

				if( mEqEnabled ) {
					try {
						if( mEqChannels.ContainsKey( bandId )) {
							var eq = new BASS_BFX_PEAKEQ { lBand = mEqChannels[bandId] };

							if( Bass.BASS_FXGetParameters( mParamEqFx, eq )) {
								eq.fGain = gain;

								Bass.BASS_FXSetParameters( mParamEqFx, eq );
							}
							else {
								mLog.LogErrorCode( "Could not retrieve parametric EQ", (int)Bass.BASS_ErrorGetCode());
							}
						}
					}
					catch( Exception ex ) {
						mLog.LogException( "Adjusting EQ", ex );
					}
				}
			}
		}

		[DllImport("gdi32.dll")]
		public static extern bool DeleteObject(IntPtr hObject);
	  
		public BitmapSource GetSpectrumImage( int channel, int height, int width, Color baseColor, Color peakColor, Color peakHoldColor ) {
			BitmapSource	retValue = null;

			if( Bass.BASS_ChannelIsActive( channel ) == BASSActive.BASS_ACTIVE_PLAYING ) {
				try {
					const int	lineGap = 1;
					const int	peakHoldHeight = 1;
					const int	peakHoldTime = 5;

					var bars = width > 300 ? 48 : width > 200 ? 32 : 24;
					var barWidth = ( width - ( bars * lineGap )) / bars;
					var bitmap = mSpectrumVisual.CreateSpectrumLinePeak( mMixerChannel, width, height,
																		System.Drawing.Color.FromArgb( baseColor.A, baseColor.R, baseColor.G, baseColor.B ),
																		System.Drawing.Color.FromArgb( peakColor.A, peakColor.R, peakColor.G, peakColor.B ),
																		System.Drawing.Color.FromArgb( peakHoldColor.A, peakHoldColor.R, peakHoldColor.G, peakHoldColor.B ),
																		System.Drawing.Color.FromArgb( 0, 0, 0, 0 ),
																		barWidth, peakHoldHeight, lineGap, peakHoldTime, false, false, false );

					if( bitmap != null ) {
						IntPtr	hBitmap = IntPtr.Zero;

						try {
							hBitmap = bitmap.GetHbitmap();

							retValue = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap( hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions() );
						}
						catch( Exception ex ) {
							mLog.LogException( "Creating spectrum bitmap", ex );
						}
						finally {
							if( hBitmap != IntPtr.Zero ) {
								DeleteObject( hBitmap );
							}
						}

						bitmap.Dispose();
					}
				}
				catch( Exception ex ) {
					mLog.LogException( "Creating spectrum image", ex );
				}
			}

			return( retValue );
		}

		public float PreampVolume {
			get => mPreampVolume;
            set {
				if(( value > 0.0 ) &&
				   ( value < 2.0 )) {
					mPreampVolume = value;

					AdjustPreamp( mPreampVolume );
				}
			}
		}

		private void InitializePreamp() {
			mPreampFx = Bass.BASS_ChannelSetFX( mMixerChannel, BASSFXType.BASS_FX_BFX_VOLUME, 0 );
			if( mPreampFx == 0 ) {
				mLog.LogErrorCode( "Could not set preamp", (int)Bass.BASS_ErrorGetCode());
			}
			else {
				AdjustPreamp( mPreampVolume );
			}
		}

		private void AdjustPreamp( float value ) {
			var volParam = new BASS_BFX_VOLUME { lChannel = BASSFXChan.BASS_BFX_CHANALL, fVolume = value };

			if(!Bass.BASS_FXSetParameters( mPreampFx, volParam )) {
				mLog.LogErrorCode( "Could not set preamp volume", (int)Bass.BASS_ErrorGetCode());
			}
		}

		public float Pan {
			get => mPan;
            set {
				mPan = value;

				SetPan();
			}
		}

		private void SetPan() {
			Bass.BASS_ChannelSetAttribute( mMixerChannel, BASSAttribute.BASS_ATTRIB_PAN, mPan );
		}

		public float PlaySpeed {
			get => mPlaySpeed;
            set{
				mPlaySpeed = value; 
				SetPlaySpeed();
			}
		}

		private void SetPlaySpeed() {
			if( mMixerSampleRate > 0 ) {
				float	multiplier = 1.0f + ( mPlaySpeed * 0.5f );

				if(!Bass.BASS_ChannelSetAttribute( mMixerChannel, BASSAttribute.BASS_ATTRIB_FREQ, mMixerSampleRate * multiplier )) {
					mLog.LogErrorCode( "Could not set play speed", (int)Bass.BASS_ErrorGetCode());
				}
			}
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
			var mode = GetChannelStatus( channel );

			if(( stream != null ) &&
			  (( mode == ePlaybackStatus.Paused ) ||
			   ( mode == ePlaybackStatus.TrackStart ))) {
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

		private void OnLevelMeter( object sender, EventArgs args ) {
			mAudioLevelsSubject.OnNext( new AudioLevels((double)mLevelMeter.LevelL / 65535, (double)mLevelMeter.LevelR / 65535 ));
		}

		public float Volume {
			get => Bass.BASS_GetVolume();
            set => Bass.BASS_SetVolume( value );
        }

		public bool Mute {
			get => mMuted;
            set {
				var volParam = new BASS_BFX_VOLUME { lChannel = BASSFXChan.BASS_BFX_CHANALL, fVolume = value ? 0.0f : 1.0f };

				if( Bass.BASS_FXSetParameters( mMuteFx, volParam )) {
					mMuted = value;
				}
				else {
					mLog.LogErrorCode( "Could not mute volume", (int)Bass.BASS_ErrorGetCode());
				}
			}
		}

		public bool StereoEnhancerEnable {
			get => mStereoEnhancerEnable;
            set {
				mStereoEnhancerEnable = value;

                mStereoEnhancer?.SetBypass(!value );
            }
		}

		public double StereoEnhancerWidth {
			get => mStereoEnhancerWidth;
            set {
				if(( value >= 0.0 ) &&
				   ( value <= 1.0 )) {
					mStereoEnhancerWidth = value * 10.0;

					if( mStereoEnhancer != null ) {
						mStereoEnhancer.WideCoeff = mStereoEnhancerWidth;
					}
				}
			}
		}

		public double StereoEnhancerWetDry {
			get => mStereoEnhancerWetDry;
            set {
				if(( value >= 0.0 ) &&
				   ( value <= 1.0 )) {
					mStereoEnhancerWetDry = value;

					if( mStereoEnhancer != null ) {
						mStereoEnhancer.WetDry = value;
					}
				}
			}
		}

		public bool SoftSaturationEnable {
			get => mSoftSaturationEnable;
            set {
				mSoftSaturationEnable = value;

                mSoftSaturation?.SetBypass(!mSoftSaturationEnable );
            }
		}

		public double SoftSaturationDepth {
			get => mSoftSaturationDepth;
            set {
				if(( value >= 0.0 ) &&
				   ( value <= 1.0 )) {
					mSoftSaturationDepth = value;

					if( mSoftSaturation != null ) {
						mSoftSaturation.Depth = mSoftSaturationDepth;
					}
				}
			}
		}

		public double SoftSaturationFactor {
			get => mSoftSaturationFactor;
            set {
				if(( value >= 0.0 ) &&
				   ( value <= 0.999 )) {
					mSoftSaturationFactor = value;

					if( mSoftSaturation != null ) {
						mSoftSaturation.Factor = mSoftSaturationFactor;
					}
				}
			}
		}

		public bool ReverbEnable {
			get => mReverbEnable;
            set {
				mReverbEnable = value;

				if( value ) {
					if( mReverbFx == 0 ) {
						mReverbFx = Bass.BASS_ChannelSetFX( mMixerChannel, BASSFXType.BASS_FX_BFX_ECHO4, 6 );

						if( mReverbFx != 0 ) {
							var reverbParam = new BASS_BFX_ECHO4 { fFeedback = 0.6f, fDelay = mReverbDelay, fDryMix = 1.0f, fWetMix = mReverbLevel };

							if(!Bass.BASS_FXSetParameters( mReverbFx, reverbParam )) {
								mLog.LogErrorCode( "Could not set initial reverb levels", (int)Bass.BASS_ErrorGetCode());
							}
						}
						else {
							mLog.LogErrorCode( "Could not set reverb FX", (int)Bass.BASS_ErrorGetCode());
						}
					}
				}
				else {
					if( mReverbFx != 0 ) {
						if( Bass.BASS_ChannelRemoveFX( mMixerChannel, mReverbFx )) {
							mReverbFx = 0;
						}
						else {
							mLog.LogErrorCode( "Could not remove reverb FX", (int)Bass.BASS_ErrorGetCode());
						}
					}
				}
			}
		}

		public float ReverbLevel {
			get {
				var retValue = mReverbLevel;

				if( mReverbFx != 0 ) {
					var reverbParam = new BASS_BFX_ECHO4();

					if( Bass.BASS_FXGetParameters( mReverbFx, reverbParam )) {
						retValue = reverbParam.fWetMix;
					}
				}

				return( retValue );
			}
			set {
				if(( mReverbFx != 0 ) &&
				   ( value >= -2.0f ) &&
				   ( value <= 2.0f )) {
					var reverbParam = new BASS_BFX_ECHO4();

					if( Bass.BASS_FXGetParameters( mReverbFx, reverbParam )) {
						reverbParam.fWetMix = value;
						reverbParam.fDelay = mReverbDelay;

						if( Bass.BASS_FXSetParameters( mReverbFx, reverbParam )) {
							mReverbLevel = value;
						}
						else {
							mLog.LogErrorCode( "Could not set reverb level", (int)Bass.BASS_ErrorGetCode());
						}
					}
				}
			}
		}

		public float ReverbDelay {
			get {
				var	retValue = mReverbDelay;

				if( mReverbFx != 0 ) {
					var reverbParam = new BASS_BFX_ECHO4();

					if( Bass.BASS_FXGetParameters( mReverbFx, reverbParam )) {
						retValue = reverbParam.fDelay;
					}
				}

				return( retValue );
			}
			set {
				if(( mReverbFx != 0 ) &&
				   ( value >= 0.1 ) &&
				   ( value <= 1.0 )) {
					var reverbParam = new BASS_BFX_ECHO4();

					if( Bass.BASS_FXGetParameters( mReverbFx, reverbParam )) {
						reverbParam.fDelay = value;
						reverbParam.fWetMix = mReverbLevel;

						if( Bass.BASS_FXSetParameters( mReverbFx, reverbParam )) {
							mReverbDelay = value;
						}
						else {
							mLog.LogErrorCode( "Could not set reverb delay", (int)Bass.BASS_ErrorGetCode());
						}
					}
				}
			}
		}
	}
}
