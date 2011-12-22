namespace Noise.Infrastructure.Interfaces {
	public interface ITimestampProvider {
		long	GetTimestamp( string componentId );
		void	SetTimestamp( string componentId, long ticks );
	}
}
