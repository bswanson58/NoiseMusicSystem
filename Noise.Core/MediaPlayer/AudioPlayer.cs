using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass.AddOn.Mix;
using Un4seen.Bass.AddOn.Tags;
using Un4seen.Bass.Misc;
using Timer = System.Timers.Timer;

namespace Noise.Core.MediaPlayer {
	internal class AudioStream {
		public	int						Channel { get; private set; }
		public	StorageFile				PhysicalFile { get; private set; }
		public	string					Url { get; private set; }
		public	bool					IsActive { get; set; }
		public	bool					InSlide { get; set; }
		public	bool					PauseOnSlide { get; set; }
		public	bool					StopOnSlide { get; set; }
		public	bool					Faded { get; set; }
		public	BASSActive				Mode { get; set; }
		public	int						MetaDataSync { get; set; }
		public	int						ReplayGainFx { get; set; }

		private AudioStream( int channel ) {
			Channel = channel;

			Mode = BASSActive.BASS_ACTIVE_STOPPED;
		}

		public AudioStream( StorageFile file, int channel ) :
			this( channel ) {
			PhysicalFile = file;
		}

		public AudioStream( string url, int channel ) :
			this( channel ) {
			Url = url;
		}

		public bool IsStream {
			get{ return(!String.IsNullOrWhiteSpace( Url )); }
		}
	}

	public class AudioPlayer : IAudioPlayer {
		private readonly IUnityContainer				mContainer;
		private readonly ILog							mLog;
		private readonly int							mMixerChannel;
		private	readonly float							mMixerSampleRate;
		private float									mPlaySpeed;
		private float									mPan;
		private float									mPreampVolume;
		private readonly SYNCPROC						mStreamMetadataSync;
		private readonly Timer							mUpdateTimer;
		private readonly Dictionary<int, AudioStream>	mCurrentStreams;
		private readonly DSP_PeakLevelMeter				mLevelMeter;
		private readonly DSP_StereoEnhancer				mStereoEnhancer;
		private readonly DSP_SoftSaturation				mSoftSaturation;
		private ParametricEqualizer						mEq;
		private readonly int							mMuteFx;
		private bool									mMuted;
		private	int										mPreampFx;
		private	int										mParamEqFx;
		private	readonly Dictionary<long, int>			mEqChannels;
		private	bool									mEqEnabled;
		private readonly DOWNLOADPROC					mDownloadProc;
		private FileStream								mRecordStream;
		private byte[]									mRecordBuffer;
		private bool									mRecord;
		private readonly Visuals						mSpectumVisual;

		private readonly Subject<int>					mChannelStatusSubject;
		public	IObservable<int>						ChannelStatusChange { get { return( mChannelStatusSubject.AsObservable()); } }

		private readonly Subject<AudioLevels>			mAudioLevelsSubject;
		public	IObservable<AudioLevels>				AudioLevelsChange { get { return( mAudioLevelsSubject.AsObservable()); }}

		private readonly Subject<StreamInfo>			mAudioStreamInfoSubject;
		public	IObservable<StreamInfo>					AudioStreamInfoChange { get { return( mAudioStreamInfoSubject.AsObservable()); }}

		public AudioPlayer( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();
			mCurrentStreams = new Dictionary<int, AudioStream>();
			mEqChannels = new Dictionary<long, int>();
			mStreamMetadataSync = new SYNCPROC( SyncProc );
			mUpdateTimer = new Timer { Enabled = false, Interval = 50, AutoReset = true };
			mUpdateTimer.Elapsed += OnUpdateTimer;
			mDownloadProc = new DOWNLOADPROC( RecordProc );
			mSpectumVisual = new Visuals();

			try {
				var licenseManager = mContainer.Resolve<ILicenseManager>();
				if( licenseManager.Initialize( Constants.LicenseKeyFile )) {
					var key = licenseManager.RetrieveKey( LicenseKeys.BassAudio );

					if( key != null ) {
						BassNet.Registration( key.Name, key.Key );
					}
				}

				Bass.BASS_Init( -1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero );
				Bass.BASS_SetConfig( BASSConfig.BASS_CONFIG_FLOATDSP, true );
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

					InitializePreamp();
				}
				else {
					mLog.LogMessage( "AudioPlayer: Mixer channel could not be created." );
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Bass Audio could not be initialized. ", ex);
			}

			LoadPlugin( "bassflac.dll" );
			LoadPlugin( "basswma.dll" );
			LoadPlugin( "bass_aac.dll" );

			mPreampVolume = 1.0f;
			mRecord = false;

			mChannelStatusSubject = new Subject<int>();
			mAudioLevelsSubject = new Subject<AudioLevels>();
			mAudioStreamInfoSubject = new Subject<StreamInfo>();
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

		private void OnUpdateTimer( object sender, ElapsedEventArgs args ) {
			var streams = new List<AudioStream>( mCurrentStreams.Values );

			foreach( var stream in streams ) {
				var mode = Bass.BASS_ChannelIsActive( mMixerChannel );
				var channelMode = Bass.BASS_ChannelIsActive( stream.Channel );

				if( mode == BASSActive.BASS_ACTIVE_STALLED ) {
					mode = channelMode == BASSActive.BASS_ACTIVE_STOPPED ? channelMode : BASSActive.BASS_ACTIVE_PAUSED;
				}
				else if( mode != BASSActive.BASS_ACTIVE_PAUSED ) {
					mode = Bass.BASS_ChannelIsActive( stream.Channel );
				}

				if( stream.Mode != mode ) {
					stream.Mode = mode;

					mChannelStatusSubject.OnNext( stream.Channel );

					if( stream.Mode != BASSActive.BASS_ACTIVE_PLAYING ) {
						mAudioLevelsSubject.OnNext( new AudioLevels());
					}
				}

				if( stream.InSlide ) {
					var sliding = Bass.BASS_ChannelIsSliding( stream.Channel, BASSAttribute.BASS_ATTRIB_VOL );
					
					if(!sliding ) {
						stream.InSlide = false;

						if( stream.PauseOnSlide ) {
							BassMix.BASS_Mixer_ChannelPause( stream.Channel );
						}
						if( stream.StopOnSlide ) {
							BassMix.BASS_Mixer_ChannelRemove( stream.Channel );
							Bass.BASS_ChannelStop( stream.Channel );
						}
					}
				}
			}

			if( mCurrentStreams.Count == 0 ) {
				mUpdateTimer.Stop();
			}
		}

		public int OpenFile( string filePath ) {
			return( OpenFile( filePath, 0.0f ));
		}

		public int OpenFile( string  filePath, float gainAdjustment ) {
			var retValue = 0;

			if( File.Exists( filePath )) {
				try {
					var channel = Bass.BASS_StreamCreateFile( filePath, 0, 0,
															  BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_STREAM_PRESCAN );
					if( channel != 0 ) {
						if(!BassMix.BASS_Mixer_StreamAddChannel( mMixerChannel, channel,
																 BASSFlag.BASS_MIXER_NORAMPIN | BASSFlag.BASS_MIXER_PAUSE | BASSFlag.BASS_MIXER_DOWNMIX | BASSFlag.BASS_STREAM_AUTOFREE )) {
							mLog.LogMessage( string.Format( "AudioPlayer - Stream could not be added to mixer: {0}", Bass.BASS_ErrorGetCode()));
						}
					
						var stream = new AudioStream( filePath, channel );
						mCurrentStreams.Add( channel, stream );

						if( gainAdjustment != 0.0f ) {
							stream.ReplayGainFx = Bass.BASS_ChannelSetFX( channel, BASSFXType.BASS_FX_BFX_VOLUME, 2 );
							if( stream.ReplayGainFx != 0 ) {
								 // convert the replaygain dB gain to a linear value
								var volparam = new BASS_BFX_VOLUME { lChannel = BASSFXChan.BASS_BFX_CHANALL,
																	 fVolume = (float)Math.Pow( 10, gainAdjustment / 20 )};
								if(!Bass.BASS_FXSetParameters( stream.ReplayGainFx, volparam )) {
									mLog.LogMessage( string.Format( "AudioPlayer - Could not set replay gain volume ({0}).", Bass.BASS_ErrorGetCode()));
								}
							}
							else {
								mLog.LogMessage( string.Format( "AudioPlayer - Could not set replay gain preamp ({0}).", Bass.BASS_ErrorGetCode()));
							}
						}

						retValue = channel;
					}
					else {
						mLog.LogMessage( String.Format( "AudioPlayer - Channel could not be created for {0} ({1})", filePath, Bass.BASS_ErrorGetCode()));
					}
				}
				catch( Exception ex ) {
					mLog.LogException( String.Format( "AudioPlayer opening {0}", filePath ), ex );
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

					var channel = Bass.BASS_StreamCreateURL( stream.Url, 0, BASSFlag.BASS_SAMPLE_FLOAT, mDownloadProc, IntPtr.Zero );
					if( channel != 0 ) {
						Single	sampleRate = 0;
						Bass.BASS_ChannelGetAttribute( channel, BASSAttribute.BASS_ATTRIB_FREQ, ref sampleRate );

						var audioStream = new AudioStream( stream.Url, channel ) {
													MetaDataSync = Bass.BASS_ChannelSetSync( channel, BASSSync.BASS_SYNC_META, 0, mStreamMetadataSync, IntPtr.Zero ) };
						mCurrentStreams.Add( channel, audioStream );

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

		public ParametricEqualizer ParametricEq {
			get{ return( mEq ); }
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
			get{ return( mEqEnabled ); }
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
						mLog.LogInfo( string.Format( "AudioPlayer - Could not set eq channel: {0}", Bass.BASS_ErrorGetCode()));
					}

					var eq = new BASS_BFX_PEAKEQ { fQ = 0f, fBandwidth = mEq.Bandwidth / 12, lChannel = BASSFXChan.BASS_BFX_CHANALL };

					int bandId = 0;
					foreach( var band in mEq.Bands ) {
						eq.lBand = bandId;
						eq.fCenter = band.CenterFrequency;
						eq.fGain = band.Gain;
						if(!Bass.BASS_FXSetParameters( mParamEqFx, eq )) {
							mLog.LogMessage( string.Format( "AudioPlayer - could not set eq band setting: {0}", Bass.BASS_ErrorGetCode()));
						}

						mEqChannels.Add( band.BandId, bandId );

						bandId++;
					}
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - AudioPlayer:InitializeEq", ex );
				}
			}
		}

		private void RemoveEq() {
			try {
				if(!Bass.BASS_ChannelRemoveFX( mMixerChannel, mParamEqFx )) {
					mLog.LogMessage( string.Format( "AudioPlayer - could not remove eq fx: {0}", Bass.BASS_ErrorGetCode()));
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - AudioPlayer:RemoveEq", ex );
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
								mLog.LogInfo( "AudioPlayer - Could not retrieve parametricEq band." );
							}
						}
					}
					catch( Exception ex ) {
						mLog.LogException( "Exception - AudioPlayer:AdjustEq", ex );
					}
				}
			}
		}

		public BitmapSource GetSpectrumImage( int channel, int height, int width, Color baseColor, Color peakColor, Color peakHoldColor ) {
			BitmapSource	retValue = null;

			if( Bass.BASS_ChannelIsActive( channel ) == BASSActive.BASS_ACTIVE_PLAYING ) {
				try {
					const int	lineGap = 1;
					const int	peakHoldHeight = 1;
					const int	peakHoldTime = 5;

					var bars = width > 300 ? 48 : width > 200 ? 32 : 24;
					var barWidth = ( width - ( bars * lineGap )) / bars;
					var bitmap = mSpectumVisual.CreateSpectrumLinePeak( mMixerChannel, width, height, 
																		System.Drawing.Color.FromArgb( baseColor.A, baseColor.R, baseColor.G, baseColor.B ),
																		System.Drawing.Color.FromArgb( peakColor.A, peakColor.R, peakColor.G, peakColor.B ), 
																		System.Drawing.Color.FromArgb( peakHoldColor.A, peakHoldColor.R, peakHoldColor.G, peakHoldColor.B ), 
																		System.Drawing.Color.FromArgb( 0, 0, 0, 0 ),
																		barWidth, peakHoldHeight, lineGap, peakHoldTime, false, true, false );

					if( bitmap != null ) {
						try {
							retValue = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap( bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
																									 BitmapSizeOptions.FromEmptyOptions());
						}
						catch( Exception ex ) {
							mLog.LogException( "Exception - AudioPlayer:CreatingSpectrumBitmap: ", ex );
						}

						bitmap.Dispose();
					}
				}
				catch( Exception ex ) {
					mLog.LogException( "Exception - AudioPlayer:CreatingSpectrumImage: ", ex );
				}
			}

			return( retValue );
		}

		public float PreampVolume {
			get{ return( mPreampVolume ); }
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
				mLog.LogInfo( "AudioPlayer - Could not set preamp." );
			}
			else {
				AdjustPreamp( mPreampVolume );
			}
		}

		private void AdjustPreamp( float value ) {
			var volparam = new BASS_BFX_VOLUME { lChannel = BASSFXChan.BASS_BFX_CHANALL, fVolume = value };

			if(!Bass.BASS_FXSetParameters( mPreampFx, volparam )) {
				mLog.LogInfo( "AudioPlayer - Could not set preamp volume." );
			}
		}

		public float Pan {
			get { return( mPan ); }
			set {
				mPan = value;

				SetPan();
			}
		}

		private void SetPan() {
			Bass.BASS_ChannelSetAttribute( mMixerChannel, BASSAttribute.BASS_ATTRIB_PAN, mPan );
		}

		public float PlaySpeed {
			get{ return( mPlaySpeed ); }
			set{
				mPlaySpeed = value; 
				SetPlaySpeed();
			}
		}

		private void SetPlaySpeed() {
			if( mMixerSampleRate > 0 ) {
				float	multiplier = 1.0f + ( mPlaySpeed * 0.5f );

				if(!Bass.BASS_ChannelSetAttribute( mMixerChannel, BASSAttribute.BASS_ATTRIB_FREQ, mMixerSampleRate * multiplier )) {
					mLog.LogInfo( string.Format( "AudioPlayer - Could not set play speed: {0}", Bass.BASS_ErrorGetCode()));
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

		public void Stop( int channel ) {
			var stream = GetStream( channel );

			if( stream != null ) {
				Bass.BASS_ChannelStop( stream.Channel );
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
			}
		}

		public void Play( int channel ) {
			var stream = GetStream( channel );

			if( stream != null ) {
				if( stream.Faded ) {
					stream.Faded = false;

					Bass.BASS_ChannelSlideAttribute( stream.Channel, BASSAttribute.BASS_ATTRIB_VOL, 1.0f, 1000 );
				}

				BassMix.BASS_Mixer_ChannelPlay( stream.Channel );

				if( stream.IsStream ) {
					PublishStreamInfo( stream.Channel );
				}
			}
		}

		public void Pause( int channel ) {
			var stream = GetStream( channel );

			if( stream != null ) {
				BassMix.BASS_Mixer_ChannelPause( mMixerChannel );
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
					case BASSActive.BASS_ACTIVE_STALLED:
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

		private void OnLevelMeter( object sender, EventArgs args ) {
			mAudioLevelsSubject.OnNext( new AudioLevels((double)mLevelMeter.LevelL / 65535, (double)mLevelMeter.LevelR / 65535 ));
		}

		public float Volume {
			get { return( Bass.BASS_GetVolume()); }
			set{ Bass.BASS_SetVolume( value ); }
		}

		public bool Mute {
			get { return( mMuted ); }
			set {
				var volparam = new BASS_BFX_VOLUME { lChannel = BASSFXChan.BASS_BFX_CHANALL, fVolume = value ? 0.0f : 1.0f };

				if( Bass.BASS_FXSetParameters( mMuteFx, volparam )) {
					mMuted = value;
				}
				else {
					mLog.LogMessage( string.Format( "AudioPlayer - Could not mute volume ({0}).", Bass.BASS_ErrorGetCode()));
				}
			}
		}

		public bool StereoEnhancerEnable {
			get{ return(!mStereoEnhancer.IsBypassed ); }
			set{ mStereoEnhancer.SetBypass(!value ); }
		}

		public double StereoEnhancerWidth {
			get{ return( mStereoEnhancer.WideCoeff / 10.0 ); }
			set {
				if(( value >= 0.0 ) &&
				   ( value <= 1.0 )) {
					mStereoEnhancer.WideCoeff = value * 10.0;
				}
			}
		}

		public double StereoEnhancerWetDry {
			get{ return( mStereoEnhancer.WetDry ); }
			set {
				if(( value >= 0.0 ) &&
				   ( value <= 1.0 )) {
					mStereoEnhancer.WetDry = value;
				}
			}
		}

		public bool SoftSaturationEnable {
			get{ return(!mSoftSaturation.IsBypassed ); }
			set{ mSoftSaturation.SetBypass(!value ); }
		}

		public double SoftSaturationDepth {
			get{ return( mSoftSaturation.Depth ); }
			set {
				if(( value >= 0.0 ) &&
				   ( value <= 1.0 )) {
					mSoftSaturation.Depth = value;
				}
			}
		}

		public double SoftSaturationFactor {
			get{ return( mSoftSaturation.Factor ); }
			set {
				if(( value >= 0.0 ) &&
				   ( value <= 0.999 )) {
					mSoftSaturation.Factor = value;
				}
			}
		}
	}
}
