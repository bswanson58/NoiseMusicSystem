using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;

namespace Noise.Core.Logging {
	internal class LogBackgroundTasks : BaseLogger, ILogBackgroundTasks {
		private readonly LoggingPreferences		mPreferences;

		private const string	cModuleName = "BackgroundTasks";
		private const string	cPhaseName = "";

		public LogBackgroundTasks( IPreferences preferences, IPlatformLog logger ) :
		base( logger ) {
			mPreferences = preferences.Load<LoggingPreferences>();
		}

		private void LogTaskMessage( string format, params object[] parameters ) {
			LogMessage( cModuleName, cPhaseName, format, parameters );
		}

		public void LogTasksStarting( int count ) {
			LogOnCondition( mPreferences.BackgroundTasks, () => LogTaskMessage( "Starting {0} background tasks.", count ));
		}

		public void LogTasksStopping() {
			LogOnCondition( mPreferences.BackgroundTasks, () => LogTaskMessage( "Stopping background tasks." ));
		}

		public void StartingDecadeTagBuilding( DbArtist artist ) {
			LogOnCondition( mPreferences.BackgroundTasks, () => LogTaskMessage( "Starting decade tag building for {0}", artist ));
		}

		public void CompletedDecadeTagBuilding( DbArtist artist ) {
			LogOnCondition( mPreferences.BackgroundTasks, () => LogTaskMessage( "Completed decade tag building for {0}", artist ));
		}

		public void StartingDiscographyExploring( DbArtist artist ) {
			LogOnCondition( mPreferences.BackgroundTasks, () => LogTaskMessage( "Starting discography exploring for {0}", artist ));
		}

		public void UpdatedFromDiscography( DbAlbum album ) {
			LogOnCondition( mPreferences.BackgroundTasks, () => LogTaskMessage( "Updated {0}, Published: {1}", album, album.PublishedYear ));
		}

		public void CompletedDiscographyExploring( DbArtist artist ) {
			LogOnCondition( mPreferences.BackgroundTasks, () => LogTaskMessage( "Completed discography exploring for {0}", artist ));
		}

		public void ReplayGainScanCompleted( DbArtist artist, DbAlbum album ) {
			LogOnCondition( mPreferences.BackgroundTasks || mPreferences.BasicActivity, () => LogTaskMessage( "ReplayGain updated: {0} {1} - Album gain: {2:N2}", artist, album, album.ReplayGainAlbumGain ));
		}

		public void ReplayGainScanFailed( DbArtist artist, DbAlbum album ) {
			LogOnCondition( mPreferences.BackgroundTasks, () => LogTaskMessage( "ReplayGain failed {0} {1}", artist, album ));
		}

		public void StartingSearchBuilding( DbArtist artist ) {
			LogOnCondition( mPreferences.BackgroundTasks, () => LogTaskMessage( "Starting search data building for {0}", artist ));
		}

		public void CompletedSearchBuilding( DbArtist artist ) {
			LogOnCondition( mPreferences.BackgroundTasks || mPreferences.BasicActivity, () => LogTaskMessage( "Completed search data building for {0}", artist ));
		}

		public void LogException( string message, Exception exception, string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, cPhaseName, exception, callerName, "{0}", message ));
		}
	}
}
