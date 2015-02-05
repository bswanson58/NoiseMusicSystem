using System;
using System.Runtime.CompilerServices;
using Noise.Core.Database;
using Noise.Infrastructure.Dto;

namespace Noise.Core.Logging {
	internal interface ILogLibraryBuilding {
		void	BuildingStarted();
		void	BuildingCompleted( DatabaseChangeSummary summary );
		void	DatabaseStatistics( DatabaseStatistics statistics );

		void	LogException( string message, Exception exception, [CallerMemberName] string callerName = "" );
	}
}
