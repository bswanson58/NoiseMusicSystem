using System;
using Noise.EntityFrameworkDatabase.Interfaces;

namespace Noise.EntityFrameworkDatabase {
	public abstract class EfProviderShell {
		protected	IDbContext		mContext;

		protected EfProviderShell( IDbContext context ) {
			mContext = context;
		}

		public void Dispose() {
			GC.SuppressFinalize( this );

			if( mContext != null ) {
				mContext.Dispose();
				mContext = null;
			}
		}
	}
}
