namespace Noise.Infrastructure.Dto {
	public class AudioLevels {
		public	double		LeftLevel { get; }
		public	double		RightLevel { get; }

		public AudioLevels() :
			this( 0.0, 0.0 ) {
		}

		public AudioLevels( double left, double right ) {
			LeftLevel = left;
			RightLevel = right;
		}
	}
}
