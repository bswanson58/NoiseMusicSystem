using System;
using System.Collections.Generic;
using System.IO;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Un4seen.Bass;

namespace Noise.AudioSupport.ReplayGain {
	public class ReplayGainScanner : IReplayGainScanner {
		private readonly	List<ReplayGainFile>	mFiles;

		public	double		AlbumGain { get; private set; }

		public ReplayGainScanner() {
			mFiles = new List<ReplayGainFile>();

			Bass.BASS_Init( -1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero );
			//Bass.BASS_SetConfig( BASSConfig.BASS_CONFIG_FLOATDSP, true );

			Bass.BASS_PluginLoad( "bassflac.dll" );

			ResetScanner();
		}

		public IEnumerable<ReplayGainFile> FileList {
			get{ return( mFiles ); }
		} 

		public void ResetScanner() {
			ReplayGain.InitGainAnalysis( 44100 );

			mFiles.Clear();
			AlbumGain = 0.0D;
		}

		private bool IsMusicFile( string fileName ) {
			var retValue = false;
			var ext = Path.GetExtension( fileName );

			if(!string.IsNullOrEmpty( ext )) {
				switch( ext.ToLower()) {
					case ".flac":
					case ".mp3":
					case ".ogg":
					case ".wma":
						retValue = true;
						break;
				}
			}

			return ( retValue );
		}


		public void AddFile( string filePath ) {
			if( IsMusicFile( filePath )) {
				AddFile( new ReplayGainFile( 0L, filePath ) );
			}
		}

		public void AddFile( long fileId, string filePath ) {
			if( IsMusicFile( filePath )) {
				AddFile( new ReplayGainFile( fileId, filePath ));
			}
		}

		public void AddFile( ReplayGainFile file ) {
			mFiles.Add( file );
		}

		public void AddAlbum( string albumDirectory ) {
			if( Directory.Exists( albumDirectory )) {
				// Recurse through subdirectories.
				var directoryList = Directory.EnumerateDirectories( albumDirectory );

				foreach( var directory in directoryList ) {
					AddAlbum( directory );
				}

				var fileList = Directory.EnumerateFiles( albumDirectory );

				foreach( var file in fileList ) {
					if( IsMusicFile( file )) {
						AddFile( file );
					}
				}
			}
		}

		public void AddAlbum( IEnumerable<ReplayGainFile> fileList ) {
			mFiles.AddRange( fileList );
		}

		public bool CalculateReplayGain() {
			bool		retValue = true;
			const int	bufferSize = 2048;
			var			buffer = new Int16[bufferSize * 2];
			var			leftChannel = new double[bufferSize];
			var			rightChannel = new double[bufferSize];

			foreach( var file in mFiles ) {
				if( File.Exists( file.FilePath ) ) {
					try {
						var channel = Bass.BASS_StreamCreateFile( file.FilePath, 0, 0, BASSFlag.BASS_STREAM_DECODE );

						if( channel != 0 ) {
							while( Bass.BASS_ChannelIsActive( channel ) > 0 ) {
								var read = Bass.BASS_ChannelGetData( channel, buffer, bufferSize * 2 );

								var index = 0;
								var channelIndex = 0;
								while( index < read ) {
									leftChannel[channelIndex] = buffer[index];
									index++;

									rightChannel[channelIndex] = buffer[index];
									index++;
									channelIndex++;
								}

								ReplayGain.AnalyzeSamples( leftChannel, rightChannel, new UIntPtr( (ulong)bufferSize ), 2 );
							}

							file.SetTrackGain( ReplayGain.GetTitleGain() );
						}
					}
					catch( Exception ex ) {
						file.SetTrackFailure( ex.Message );

						retValue = false;
					}
				}
				else {
					retValue = false;
				}
			}

			if( retValue ) {
				AlbumGain = ReplayGain.GetAlbumGain();
			}

			return ( retValue );
		}
	}
}
