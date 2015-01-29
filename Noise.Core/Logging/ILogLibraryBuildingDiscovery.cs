using System;
using System.Runtime.CompilerServices;
using Noise.Infrastructure.Dto;

namespace Noise.Core.Logging {
	interface ILogLibraryBuildingDiscovery {
		void	LogDiscoveryStarted( RootFolder folder );
		void	LogDiscoveryCompleted( RootFolder folder );
		void	LogLibraryNotFound( RootFolder folder );

		void	LogFolderFound( StorageFolder folder );
		void	LogFolderNotFound( StorageFolder folder );

		void	LogFileFound( StorageFile file );
		void	LogFileNotFound( StorageFile file );
		void	LogFileUpdated( StorageFile file );

		void	LogDiscoveryException( string message, Exception exception, [CallerMemberName] string callerName = "" );
		void	LogDiscoveryException( RootFolder folder, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
