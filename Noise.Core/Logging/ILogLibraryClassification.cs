using System;
using System.Runtime.CompilerServices;
using Noise.Core.FileProcessor;
using Noise.Infrastructure.Dto;

namespace Noise.Core.Logging {
	public interface ILogLibraryClassification {
		void	LogClassificationStarted();
		void	LogClassificationCompleted();

		void	LogFileClassificationStarting( StorageFile file );
		void	LogFileClassificationCompleted( StorageFile file );

		void	LogClassificationStepStarting( StorageFile file, ePipelineStep step );
		void	LogClassificationStepCompleted( StorageFile file, ePipelineStep step );

		void	LogArtistAdded( StorageFile file, DbArtist artist );
		void	LogArtistFound( StorageFile file, DbArtist artist );
		void	LogArtistNotFound( StorageFile file );
		void	LogAlbumAdded( StorageFile file, DbAlbum album );
		void	LogAlbumFound( StorageFile file, DbAlbum album );
		void	LogAlbumNotFound( StorageFile file );
		void	LogTrackAdded( StorageFile file, DbTrack track );
		void	LogTrackInfo( StorageFile file, DbTrack track, string property, string value );
		void	LogTrackNotFound( StorageFile file );

		void	LogArtworkAdded( StorageFile file, DbArtwork artwork );
		void	LogTextInfoAdded( StorageFile file, DbTextInfo textInfo );
		void	LogUnknownFile( StorageFile file );

		void	LogFileTypeDetermined( StorageFile file );

		void	LogClassificationException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
