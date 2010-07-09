using System;
using System.ComponentModel.Composition;

namespace Noise.Infrastructure.Dto {
	public class DbPlayHistory {
		public	DateTime	PlayedOn { get; set; }
		public	StorageFile	Track { get; private set; }

		public DbPlayHistory( StorageFile track ) {
			Track = track;
			PlayedOn = DateTime.Now;
		}

		[Export("PersistenceType")]
		public static Type PersistenceType {
			get{ return( typeof( DbPlayHistory )); }
		}
	}
}
