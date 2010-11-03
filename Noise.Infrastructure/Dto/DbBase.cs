using System;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Dto {
	public class DbBase : IDbBase {
		public long	DbId { get; private set; }

		protected DbBase() {
			DbId = BitConverter.ToInt64( Guid.NewGuid().ToByteArray(), 0 );
		}

		protected DbBase( long id ) {
			DbId = id;
		}
	}
}
