using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase {
	public class EloqueraUpdateShell<T> : EloqueraProviderBase, IDataUpdateShell<T> {
		public	T	Item { get; private set; }

		public EloqueraUpdateShell( IDatabaseShell databaseShell, T item ) :
			base( databaseShell ) {
			Item = item;
		}

		public virtual void Update() {
			if( mDatabaseShell != null ) {
				mDatabaseShell.Database.UpdateItem( Item );
			}
		}
	}
}
