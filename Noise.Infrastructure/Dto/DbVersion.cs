﻿using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbVersion {
		public	UInt16		MajorVersion { get; private set; }
		public	UInt16		MinorVersion { get; private set; }
		public	DateTime	DatabaseCreation { get; private set; }
		public	long		DatabaseId { get; private set; }

		public DbVersion( UInt16 majorVersion, UInt16 minorVersion ) {
			MajorVersion = majorVersion;
			MinorVersion = minorVersion;

			DatabaseCreation = DateTime.Now;
			DatabaseId = BitConverter.ToInt64( Guid.NewGuid().ToByteArray(), 0 );
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbVersion )); }
		}
	}
}
