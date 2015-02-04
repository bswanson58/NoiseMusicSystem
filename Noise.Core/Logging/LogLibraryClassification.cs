using System;
using Noise.Core.FileProcessor;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Logging;

namespace Noise.Core.Logging {
	public class LogLibraryClassification : BaseLogger, ILogLibraryClassification {
		private readonly LoggingPreferences		mPreferences;

		private const string	cModuleName = "Library Building";
		private const string	cPhaseName = "Classification";

		public LogLibraryClassification( IPreferences preferences, IPlatformLog logger ) :
		base( logger ) {
			mPreferences = preferences.Load<LoggingPreferences>();
		}

		private void LogClassificationMessage( string format, params object[] parameters ) {
			LogMessage( cModuleName, cPhaseName, format, parameters );
		}

		public void LogClassificationStarted() {
			LogOnCondition( mPreferences.LogAnyClassification, () => LogClassificationMessage( "Starting Library classification" ));
		}

		public void LogClassificationCompleted() {
			LogOnCondition( mPreferences.LogAnyClassification, () => LogClassificationMessage( "Completed Library classification" ));
		}

		public void LogFileClassificationStarting( StorageFile file ) {
			LogOnCondition( mPreferences.FileClassificationFiles, () => LogClassificationMessage( string.Format( "Starting file: {0}", file )));
		}

		public void LogFileClassificationCompleted( StorageFile file ) {
			LogOnCondition( mPreferences.FileClassificationFiles, () => LogClassificationMessage( string.Format( "Completed file: {0}", file )));
		}

		public void LogClassificationStepStarting( StorageFile file, ePipelineStep step ) {
			LogOnCondition( mPreferences.FileClassificationSteps, () => LogClassificationMessage( string.Format( "Starting step:{0} for {1}", step, file )));
		}

		public void LogClassificationStepCompleted( StorageFile file, ePipelineStep step ) {
			LogOnCondition( mPreferences.FileClassificationSteps, () => LogClassificationMessage( string.Format( "Completed step: {0} for {1}", step, file )));
		}

		public void LogArtistAdded( StorageFile file, DbArtist artist ) {
			LogOnCondition( mPreferences.FileClassificationArtists, () => LogClassificationMessage( string.Format( "Added Artist: {0} for {1}", artist, file )));
		}

		public void LogArtistFound( StorageFile file, DbArtist artist ) {
			LogOnCondition( mPreferences.FileClassificationArtists, () => LogClassificationMessage( string.Format( "Found Artist: {0} for {1}", artist, file )));
		}

		public void LogArtistNotFound( StorageFile file ) {
			LogOnCondition( mPreferences.FileClassificationArtists, () => LogClassificationMessage( string.Format( "Unable to determine Artist: {0}", file )));
		}

		public void LogAlbumAdded( StorageFile file, DbAlbum album ) {
			LogOnCondition( mPreferences.FileClassificationAlbums, () => LogClassificationMessage( string.Format( "Added Album: {0} for {1}", album, file )));
		}

		public void LogAlbumFound( StorageFile file, DbAlbum album ) {
			LogOnCondition( mPreferences.FileClassificationAlbums, () => LogClassificationMessage( string.Format( "Found Album: {0} for {1}", album, file )));
		}

		public void LogAlbumNotFound( StorageFile file ) {
			LogOnCondition( mPreferences.FileClassificationAlbums, () => LogClassificationMessage( string.Format( "Unable to determine Album: {0}", file )));
		}

		public void LogTrackAdded( StorageFile file, DbTrack track ) {
			LogOnCondition( mPreferences.FileClassificationTracks, () => LogClassificationMessage( string.Format( "Added Track: {0} for {1}", track, file )));
		}

		public void LogTrackInfo( StorageFile file, DbTrack track, string property, string value ) {
			LogOnCondition( mPreferences.FileClassificationTracks, () => LogClassificationMessage( string.Format( "Set Track info: {0} property:\"{1}\" to:\"{2}\"", track, property, value )));
		}

		public void LogTrackNotFound( StorageFile file ) {
			LogOnCondition( mPreferences.FileClassificationTracks, () => LogClassificationMessage( string.Format( "Unable to determine Track: {0}", file )));
		}

		public void LogArtworkAdded( StorageFile file, DbArtwork artwork ) {
			LogOnCondition( mPreferences.FileClassificationArtwork, () => LogClassificationMessage( string.Format( "Added Artwork: {0} for {1}", artwork, file )));
		}

		public void LogTextInfoAdded( StorageFile file, DbTextInfo textInfo ) {
			LogOnCondition( mPreferences.FileClassificationTextInfo, () => LogClassificationMessage( string.Format( "Added TextInfo: {0} for {1}", textInfo, file )));
		}

		public void LogUnknownFile( StorageFile file ) {
			LogOnCondition( mPreferences.FileClassificationUnknown, () => LogClassificationMessage( string.Format( "Unclassified: {0}", file )));
		}

		public void LogFileTypeDetermined( StorageFile file ) {
			LogOnCondition( mPreferences.FileClassificationSteps, () => LogClassificationMessage( string.Format( "Determined type:{0}", file )));
		}

		public void LogClassificationException( string message, Exception exception, string callerName = "" ) {
			LogOnCondition( mPreferences.LogExceptions, () => LogException( cModuleName, cPhaseName, exception, callerName, "{0}", message ));
		}
	}
}
