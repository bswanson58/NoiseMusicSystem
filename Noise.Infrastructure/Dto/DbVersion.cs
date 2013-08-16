using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay( "Version = {MajorVersion}.{MinorVersion}" )]
	public class DbVersion : DbBase {
		public static long	DatabaseVersionDbId = 1L;

		public	Int16		MajorVersion { get; protected set; }
		public	Int16		MinorVersion { get; protected set; }
		public	long		DatabaseCreationTicks { get; protected set; }
		public	long		DatabaseId { get; protected set; }

		protected DbVersion() :
			this( 0, 0 ) { }

		public DbVersion( Int16 majorVersion, Int16 minorVersion ) :
			base( DatabaseVersionDbId ) {
			MajorVersion = majorVersion;
			MinorVersion = minorVersion;

			DatabaseCreationTicks = DateTime.Now.Ticks;
			DatabaseId = BitConverter.ToInt64( Guid.NewGuid().ToByteArray(), 0 );
		}

		public bool IsOlderVersion( DbVersion version ) {
			var retValue = false;

			if( MajorVersion < version.MajorVersion ) {
				retValue = true;
			}
			else {
				if( MajorVersion == version.MajorVersion ) {
					if( MinorVersion < version.MinorVersion ) {
						retValue = true;
					}
				}
			}

			return( retValue );
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
