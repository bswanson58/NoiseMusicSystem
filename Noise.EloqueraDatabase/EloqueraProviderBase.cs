using System;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase {
	public abstract class EloqueraProviderBase : IDisposable {
		protected readonly IDatabaseShell	mDatabaseShell;

		protected EloqueraProviderBase( IDatabaseShell databaseShell ) {
			mDatabaseShell = databaseShell;
		}

		public void Dispose() {
			if( mDatabaseShell != null ) {
				mDatabaseShell.Dispose();
			}
		}
	}
}
