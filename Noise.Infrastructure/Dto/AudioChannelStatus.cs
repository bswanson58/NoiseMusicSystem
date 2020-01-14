namespace Noise.Infrastructure.Dto {
	public class AudioChannelStatus {
		public int Channel { get; }
		public ePlaybackStatus Status { get; }

		public AudioChannelStatus( int channel, ePlaybackStatus status ) {
			Channel = channel;
			Status = status;
		}

		public override string ToString() {
			return( $"Audio channel:{Channel} status:{Status}" );
		}
	}
}
