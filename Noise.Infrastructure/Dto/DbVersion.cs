using System;
using System.ComponentModel.Composition;
using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	public class DbVersion : DbBase {
		public	UInt16		MajorVersion { get; protected set; }
		public	UInt16		MinorVersion { get; protected set; }
		public	long		DatabaseCreationTicks { get; protected set; }
		public	long		DatabaseId { get; protected set; }

		protected DbVersion() :
			this( 0, 0 ) { }

		public DbVersion( UInt16 majorVersion, UInt16 minorVersion ) :
			base( 1L ) {
			MajorVersion = majorVersion;
			MinorVersion = minorVersion;

			DatabaseCreationTicks = DateTime.Now.Ticks;
			DatabaseId = BitConverter.ToInt64( Guid.NewGuid().ToByteArray(), 0 );
		}

		[Ignore]
		public DateTime DatabaseCreation {
			get{ return( new DateTime( DatabaseCreationTicks )); }
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbVersion )); }
		}
	}
}
