using System;
using Noise.Infrastructure.Dto;

namespace Noise.Infrastructure.Interfaces {
	public interface IDatabaseInfo {
		bool		IsOpen { get; }

		long		DatabaseId { get; }
		DbVersion	DatabaseVersion { get; }

		void		InitializeDatabaseVersion( Int16 databaseVersion );
	}
}
