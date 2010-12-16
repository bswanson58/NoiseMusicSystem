using System;
using System.ComponentModel.Composition;
using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	public class DbVersion {
		public	UInt16		MajorVersion { get; private set; }
		public	UInt16		MinorVersion { get; private set; }
		public	long		DatabaseCreationTicks { get; private set; }
		public	long		DatabaseId { get; private set; }

		public DbVersion( UInt16 majorVersion, UInt16 minorVersion ) {
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
