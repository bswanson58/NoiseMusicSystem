using System;

namespace Noise.Infrastructure.Dto {
	public class DbPlayHistory {
		public	DateTime	PlayedOn { get; private set; }
		public	StorageFile	Track { get; private set; }

		public DbPlayHistory( StorageFile track ) {
			Track = track;
			PlayedOn = DateTime.Now;
		}
	}
}
