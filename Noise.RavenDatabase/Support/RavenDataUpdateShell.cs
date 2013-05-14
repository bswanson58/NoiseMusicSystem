using System;
using Noise.Infrastructure.Interfaces;

namespace Noise.RavenDatabase.Support {
	public class RavenDataUpdateShell<T> : IDataUpdateShell<T> where T : class {
		private Action<T>	mUpdate;
 
		public RavenDataUpdateShell( Action<T> update, T item ) {
			mUpdate = update;

			Item = item;
		} 		
		public T Item { get; private set; }

		public void Dispose() {
			mUpdate = null;
		}

		public void Update() {
			if(( Item != null ) &&
			   ( mUpdate != null )) {
				mUpdate( Item );
			}
		}
	}
}
