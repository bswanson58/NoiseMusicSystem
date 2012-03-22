using System;
using System.ComponentModel.Composition;
using Eloquera.Client;

namespace Noise.Infrastructure.Dto {
	public class DbPlayHistory : DbBase {
		public	long		PlayedOnTicks { get; set; }
		public	long		StorageFileId { get; protected set; }
		public	long		TrackId { get; protected set; }

		protected DbPlayHistory() { }

		public DbPlayHistory( StorageFile track ) {
			StorageFileId = track.DbId;
			TrackId = track.MetaDataPointer;
			PlayedOnTicks = DateTime.Now.Ticks;
		}

		[Ignore]
		public DateTime PlayedOn {
			get{ return( new DateTime( PlayedOnTicks )); }
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbPlayHistory )); }
		}
	}
}
