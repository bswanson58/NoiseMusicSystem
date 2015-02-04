using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Logging {
	public class LogLibraryBuildingSummary : BaseLogger, ILogLibraryBuildingSummary {
		private readonly LoggingPreferences		mPreferences;

		private const string	cModuleName = "Library Building";
		private const string	cPhaseName = "Summary";

		public LogLibraryBuildingSummary( IPreferences preferences, IPlatformLog logger ) :
		base( logger ) {
			mPreferences = preferences.Load<LoggingPreferences>();
		}

		private void LogSummaryMessage( string format, params object[] parameters ) {
			LogMessage( cModuleName, cPhaseName, format, parameters );
		}

		public void LogSummaryBuildingStarted() {
			LogOnCondition( mPreferences.LogAnyBuildingSummary, () => LogSummaryMessage( "Starting library summary building" ));
		}

		public void LogSummaryBuildingCompleted() {
			LogOnCondition( mPreferences.LogAnyBuildingSummary, () => LogSummaryMessage( "Completed library summary building" ));
		}

		public void LogSummaryArtistStarted( DbArtist artist ) {
			LogOnCondition( mPreferences.BuildingSummaryArtists, () => LogSummaryMessage( "Starting summary building for:{0}", artist ));
		}

		public void LogSummaryArtistCompleted( DbArtist artist ) {
			LogOnCondition( mPreferences.BuildingSummaryArtists, () => LogSummaryMessage( "Completed summary building for:{0}", artist ));
		}

		public void LogSummaryAlbumStarted( DbAlbum album ) {
			LogOnCondition( mPreferences.BuildingSummaryAlbums, () => LogSummaryMessage( "Starting summary building for:{0}", album ));
		}

		public void LogSummaryAlbumCompleted( DbAlbum album ) {
			LogOnCondition( mPreferences.BuildingSummaryAlbums, () => LogSummaryMessage( "Completed summary building for:{0}", album ));
		}

		public void LogSummaryBuildingException( string message, Exception exception, string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, cPhaseName, exception, callerName, "{0}", message ));
		}
	}
}
