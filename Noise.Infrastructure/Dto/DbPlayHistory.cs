using System;

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

		public DateTime PlayedOn {
			get{ return( new DateTime( PlayedOnTicks )); }
		}
	}
}
