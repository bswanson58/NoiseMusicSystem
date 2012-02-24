using System;

namespace Noise.EloqueraDatabase.Interfaces {
	public interface IDatabaseShell : IDisposable {
		IDatabase	Database { get; }

		void		FreeDatabase();
	}
}
