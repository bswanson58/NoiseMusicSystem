using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;

namespace Noise.Core.Logging {
	public class LogLibraryCleaning : BaseLogger, ILogLibraryCleaning {
		private readonly LoggingPreferences		mPreferences;

		private const string	cModuleName = "Library Building";
		private const string	cPhaseName = "Cleaning";

		public LogLibraryCleaning( IPreferences preferences, ILog logger ) :
		base( logger ) {
			mPreferences = preferences.Load<LoggingPreferences>();
		}

		private void LogCleaningMessage( string format, params object[] parameters ) {
			LogMessage( cModuleName, cPhaseName, format, parameters );
		}

		public void LogCleaningStarted() {
			LogOnCondition( mPreferences.LogAnyCleaning, () => LogCleaningMessage( "Starting Library metadata cleaning" ));
		}

		public void LogCleaningCompleted() {
			LogOnCondition( mPreferences.LogAnyCleaning, () => LogCleaningMessage( "Completed Library metadata cleaning" ));
		}

		public void LogRemovingFolder( StorageFolder folder ) {
			LogOnCondition( mPreferences.MetadataCleaningFileObjects, ()=> LogCleaningMessage( "Removing folder: {0}", folder ));
		}

		public void LogRemovingFile( StorageFile file ) {
			LogOnCondition( mPreferences.MetadataCleaningFileObjects, ()=> LogCleaningMessage( "Removing file: {0}", file ));
		}

		public void LogRemovingTrack( DbTrack track ) {
			LogOnCondition( mPreferences.MetadataCleaningDomainObjects, ()=> LogCleaningMessage( "Removing track: {0}", track ));
		}

		public void LogRemovingAlbum( DbAlbum album ) {
			LogOnCondition( mPreferences.MetadataCleaningDomainObjects, ()=> LogCleaningMessage( "Removing album: {0}", album ));
		}

		public void LogRemovingArtist( DbArtist artist ) {
			LogOnCondition( mPreferences.MetadataCleaningDomainObjects, ()=> LogCleaningMessage( "Removing artist: {0}", artist ));
		}

		public void LogRemovingArtwork( DbArtwork artwork ) {
			LogOnCondition( mPreferences.MetadataCleaningDomainObjects, ()=> LogCleaningMessage( "Removing artwork: {0}", artwork ));
		}

		public void LogRemovingTextInfo( DbTextInfo textInfo ) {
			LogOnCondition( mPreferences.MetadataCleaningDomainObjects, ()=> LogCleaningMessage( "Removing info: {0}", textInfo ));
		}

		public void LogCleaningException( string message, Exception exception, string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, cPhaseName, exception, callerName, "{0}", message ));
		}
	}
}
