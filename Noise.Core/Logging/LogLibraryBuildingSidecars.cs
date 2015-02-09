using System;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;

namespace Noise.Core.Logging {
	public class LogLibraryBuildingSidecars : BaseLogger, ILogLibraryBuildingSidecars {
		private readonly LoggingPreferences		mPreferences;

		private const string	cModuleName = "Library Building";
		private const string	cPhaseName = "Sidecars";

		public LogLibraryBuildingSidecars( IPreferences preferences, IPlatformLog logger ) :
		base( logger ) {
			mPreferences = preferences.Load<LoggingPreferences>();
		}

		private void LogBuildingMessage( string format, params object[] parameters ) {
			LogMessage( cModuleName, cPhaseName, format, parameters );
		}

		public void LogSidecarBuildingStarted() {
			LogOnCondition( mPreferences.SidecarSupport, () => LogBuildingMessage( "Started sidecar loading" ));
		}

		public void LogSidecarBuildingCompleted() {
			LogOnCondition( mPreferences.SidecarSupport, () => LogBuildingMessage( "Completed sidecar loading" ));
		}

		public void LogLoadedSidecar( StorageSidecar sidecar, DbAlbum album ) {
			LogOnCondition( mPreferences.SidecarSupport, () => LogBuildingMessage( "Load sidecar {0} for album {1}", sidecar, album ));
		}

		public void LogUpdatedSidecar( StorageSidecar sidecar, DbAlbum album ) {
			LogOnCondition( mPreferences.SidecarSupport, () => LogBuildingMessage( "Updated sidecar {0} for Album {1}", sidecar, album ));
		}

		public void LogUpdatedAlbum( StorageSidecar sidecar, DbAlbum album ) {
			LogOnCondition( mPreferences.SidecarSupport, () => LogBuildingMessage( "Updated Album {0} from sidecar {1}", album, sidecar ));
		}

		public void LogUnknownAlbumSidecar( StorageSidecar sidecar ) {
			LogOnCondition( mPreferences.SidecarSupport, () => LogBuildingMessage( "Unable to locate album for {0}", sidecar ));
		}

		public void LogException( string message, Exception exception, string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, cPhaseName, exception, callerName, "{0}", message ));
		}
	}
}
