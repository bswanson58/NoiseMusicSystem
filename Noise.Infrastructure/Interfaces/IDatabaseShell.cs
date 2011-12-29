using System;

namespace Noise.Infrastructure.Interfaces {
	public interface IDatabaseShell : IDisposable {
		IDatabase	Database { get; }

		void		FreeDatabase();
	}
}
