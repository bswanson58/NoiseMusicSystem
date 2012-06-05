using System;
using System.Security.Cryptography;
using ReusableBits.Interfaces;

namespace ReusableBits.Support {
	public class IdentityProvider : IIdentityProvider {
		private	readonly RNGCryptoServiceProvider mCryptoServiceProvider;
		public	IdentityType	IdentityType { get; set; }

		public IdentityProvider() {
			mCryptoServiceProvider = new RNGCryptoServiceProvider();
		}

		public Guid NewIdentityAsGuid() {
			return( NewIdentity());
		}

		public long NewIdentityAsLong() {
			return( BitConverter.ToInt64( NewIdentity().ToByteArray(), 0 ));
		}

		public string NewIdentityAsString() {
			return( NewIdentity( true ).ToString());
		}

		private Guid NewIdentity() {
			return( NewIdentity( false ));
		}

		private Guid NewIdentity( bool asString ) {
			if( IdentityType == IdentityType.Guid ) {
				return( Guid.NewGuid());
			}

			return( NewSequentialIdentity( asString ));
		}

		private Guid NewSequentialIdentity( bool asString ) {
			long	timestamp = DateTime.Now.Ticks / 10000L;
			var		timestampBytes = BitConverter.GetBytes( timestamp );
			var		randomBytes = new byte[10];

			mCryptoServiceProvider.GetBytes( randomBytes );

			if( BitConverter.IsLittleEndian ) {
				Array.Reverse( timestampBytes );
			}

			var guidBytes = new byte[16];

			if( IdentityType == IdentityType.SequentialEndingGuid ) {
				Buffer.BlockCopy( randomBytes, 0, guidBytes, 0, 10 );
				Buffer.BlockCopy( timestampBytes, 2, guidBytes, 10, 6 );
			}
			else {
				Buffer.BlockCopy( timestampBytes, 2, guidBytes, 0, 6 );
				Buffer.BlockCopy( randomBytes, 0, guidBytes, 6, 10 );

				// If formatting as a string, we have to reverse the order
				// of the Data1 and Data2 blocks on little-endian systems.
				if( asString && BitConverter.IsLittleEndian ) {
					Array.Reverse( guidBytes, 0, 4 );
					Array.Reverse( guidBytes, 4, 2 );
				}
			}

			return new Guid( guidBytes );
		}
	}
}
