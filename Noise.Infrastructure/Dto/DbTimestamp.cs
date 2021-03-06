﻿namespace Noise.Infrastructure.Dto {
	public class DbTimestamp {
		public	string		ComponentId { get; protected set; }
		public	long		Timestamp { get; protected set; }

		protected DbTimestamp() :
			this( string.Empty ) { }

		public DbTimestamp( string componentId ) {
			ComponentId = componentId;

			Timestamp = 0;
		}

		public void SetTimestamp( long ticks ) {
			Timestamp = ticks;
		}
	}
}
