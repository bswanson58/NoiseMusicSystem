using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;

namespace Noise.Core.Logging {
	public class LogPlayQueue : BaseLogger, ILogPlayQueue {
		private readonly LoggingPreferences		mPreferences;

		private const string	cModuleName = "Playback";
		private const string	cPhaseName = "PlayQueue";

		public LogPlayQueue( IPreferences preferences, IPlatformLog logger ) :
		base( logger ) {
			mPreferences = preferences.Load<LoggingPreferences>();
		}

		private void LogQueueMessage( string format, params object[] parameters ) {
			LogMessage( cModuleName, cPhaseName, format, parameters );
		}

		public void AddedTrack( PlayQueueTrack track ) {
			LogOnCondition( mPreferences.PlayQueueAddRemove, ()=> LogQueueMessage( "Added: {0}", track ));
		}

		public void RemovedTrack( PlayQueueTrack track ) {
			LogOnCondition( mPreferences.PlayQueueAddRemove, ()=> LogQueueMessage( "Removed: {0}", track ));
		}

		public void ClearedQueue() {
			LogOnCondition( mPreferences.PlayQueueAddRemove, ()=> LogQueueMessage( "Cleared queue" ));
		}

		public void StatusChanged( PlayQueueTrack track ) {
			var status = string.Format( "{0}{1}{2}", track.HasPlayed ? "Played" : string.Empty, track.IsPlaying ? "Playing" : string.Empty, track.IsFaulted ? "Faulted" : string.Empty );
			LogOnCondition( mPreferences.PlayQueueStatusChange, ()=> LogQueueMessage( "Status changed ({0}) {1}", status, track ));
		}

		public void StrategySet( ePlayStrategy strategy, IPlayStrategyParameters parameters ) {
			LogOnCondition( mPreferences.PlayQueueAddRemove, () => LogQueueMessage( "Playback strategy set: {0}", strategy ));
		}

		public void LogQueueException( string message, Exception exception, string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, cPhaseName, exception, callerName, "{0}", message ));
		}
	}
}
