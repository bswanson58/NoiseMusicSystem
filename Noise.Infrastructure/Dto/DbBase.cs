using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.Infrastructure.Dto {
	public abstract class DbBase : IDbBase {
		public long	DbId { get; protected set; }

		protected DbBase() {
			DbId = DatabaseIdentityProvider.Current.NewIdentityAsLong();
		}

		protected DbBase( long id ) {
			DbId = id;
		}
	}
}
