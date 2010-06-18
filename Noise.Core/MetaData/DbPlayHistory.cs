using System;
using Noise.Core.FileStore;

namespace Noise.Core.MetaData {
	public class DbPlayHistory {
		public	DateTime	PlayedOn { get; private set; }
		public	StorageFile	Track { get; private set; }

		public DbPlayHistory( StorageFile track ) {
			Track = track;
			PlayedOn = DateTime.Now;
		}
	}
}
