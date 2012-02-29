using System;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IDatabaseInfo {
		long		DatabaseId { get; }
		DbVersion	DatabaseVersion { get; }

		void		InitializeDatabaseVersion( Int16 majorVersion, Int16 minorVersion );
	}
}
