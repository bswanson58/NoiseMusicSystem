namespace Noise.Infrastructure.Dto {
	public class AudioChannelStatus {
		public int Channel { get; private set; }
		public ePlaybackStatus Status { get; private set; }

		public AudioChannelStatus( int channel, ePlaybackStatus status ) {
			Channel = channel;
			Status = status;
		}
	}
}
