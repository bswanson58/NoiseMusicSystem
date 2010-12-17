using System;

namespace Noise.Core.DataExchange.Dto {
	internal class CloudSyncEntry {
		public long		SequenceNumber { get; set; }
		public long		CommitTimeTicks { get; set; }

		public CloudSyncEntry() {
			CommitTimeTicks = DateTime.Now.Ticks;
		}
	}
}
