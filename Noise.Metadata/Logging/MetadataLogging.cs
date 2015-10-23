using System;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;

namespace Noise.Metadata.Logging {
	internal class MetadataLogging : BaseLogger, ILogMetadata {
		private readonly LoggingPreferences		mPreferences;

		private const string	cModuleName = "Metadata";
		private const string	cPhaseName = "Providers";

		public MetadataLogging( LoggingPreferences preferences, IPlatformLog logger ) :
		base( logger ) {
			mPreferences = preferences;
		}

		private void LogMetadataMessage( string format, params object[] parameters ) {
			LogMessage( cModuleName, cPhaseName, format, parameters );
		}

		public void LoadedMetadata( string provider, string forArtist ) {
			LogOnCondition( mPreferences.MetadataSupport || mPreferences.BasicActivity, 
							() => LogMetadataMessage( string.Format( "Provider {0} loaded metadata for artist \"{1}\"", provider, forArtist )));
		}

		public void ArtistNotFound( string provider, string forArtist ) {
			LogOnCondition( mPreferences.MetadataSupport || mPreferences.BasicActivity, 
							() => LogMetadataMessage( string.Format( "Provider {0} unable to match artist \"{1}\"", provider, forArtist )));
		}

		public void DownloadedArtwork( string provider, string forArtist, int pieceCount ) {
			LogOnCondition( mPreferences.MetadataSupport || mPreferences.BasicActivity, 
							() => LogMetadataMessage( string.Format( "Downloaded {0} pieces of artwork for artist \"{1}\" from provider \"{2}\"", pieceCount, forArtist, provider )));
		}

		public void LogException( string message, Exception exception, string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, cPhaseName, exception, callerName, "{0}", message ));
		}
	}
}
