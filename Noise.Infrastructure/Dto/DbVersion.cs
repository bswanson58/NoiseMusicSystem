using System;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace Noise.Infrastructure.Dto {
	[DebuggerDisplay( "Version = {DatabaseVersion}" )]
	public class DbVersion : DbBase {
		public static long	DatabaseVersionDbId = 1L;

		public	Int16		DatabaseVersion { get; protected set; }
		public	long		DatabaseCreationTicks { get; protected set; }
		public	long		DatabaseId { get; protected set; }

		protected DbVersion() :
			this( 0 ) { }

		public DbVersion( Int16 databaseVersion ) :
			base( DatabaseVersionDbId ) {
			DatabaseVersion = databaseVersion;

			DatabaseCreationTicks = DateTime.Now.Ticks;
			DatabaseId = BitConverter.ToInt64( Guid.NewGuid().ToByteArray(), 0 );
		}

		public bool IsOlderVersion( DbVersion version ) {
			return( DatabaseVersion < version.DatabaseVersion );
		}

		public DateTime DatabaseCreation {
			get{ return( new DateTime( DatabaseCreationTicks )); }
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbVersion )); }
		}
	}
}
