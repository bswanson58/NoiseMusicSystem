using System;
using Eloquera.Client;
using Noise.Infrastructure.Interfaces;

namespace Noise.Infrastructure.Dto {
	public abstract class DbBase : IDbBase {
		[Index]
		public long	DbId { get; protected set; }

		protected DbBase() {
			DbId = BitConverter.ToInt64( Guid.NewGuid().ToByteArray(), 0 );
		}

		protected DbBase( long id ) {
			DbId = id;
		}
	}
}
