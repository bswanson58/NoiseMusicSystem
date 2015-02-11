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
			LogOnCondition( mPreferences.SidecarSupport, () => LogBuildingMessage( "Loaded {0} for {1}", sidecar, album ));
		}

		public void LogWriteSidecar( ScAlbum scAlbum ) {
			LogOnCondition( mPreferences.SidecarSupport, () => LogBuildingMessage( "Writing {0}", scAlbum ));
		}

		public void LogUpdatedSidecar( StorageSidecar sidecar, DbAlbum album ) {
			LogOnCondition( mPreferences.SidecarSupport, () => LogBuildingMessage( "Updated {0} for {1}", sidecar, album ));
		}

		public void LogUpdated( DbArtist dbArtist, ScArtist scArtist ) {
			LogOnCondition( mPreferences.SidecarSupport, () => LogBuildingMessage( "Updated {0} from {1}", dbArtist, scArtist ));
		}

		public void LogUpdated( DbAlbum dbAlbum, ScAlbum scAlbum ) {
			LogOnCondition( mPreferences.SidecarSupport, () => LogBuildingMessage( "Updated {0} from {1}", dbAlbum, scAlbum ));
		}

		public void LogUnknownArtistSidecar( StorageSidecar sidecar ) {
			LogOnCondition( mPreferences.SidecarSupport, () => LogBuildingMessage( "Unable to locate artist for {0}", sidecar ));
		}

		public void LogUnknownAlbumSidecar( StorageSidecar sidecar ) {
			LogOnCondition( mPreferences.SidecarSupport, () => LogBuildingMessage( "Unable to locate album for {0}", sidecar ));
		}

		public void LogUnknownTrack( DbAlbum album, ScTrack track ) {
			LogOnCondition( mPreferences.SidecarSupport, () => LogBuildingMessage( "Unable to locate {0} in {1}", track, album ));
		}

		public void LogException( string message, Exception exception, string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, cPhaseName, exception, callerName, "{0}", message ));
		}
	}
}
