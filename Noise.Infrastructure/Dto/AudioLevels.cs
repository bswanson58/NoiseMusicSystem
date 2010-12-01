namespace Noise.Infrastructure.Dto {
	public class AudioLevels {
		public	double		LeftLevel { get; private set; }
		public	double		RightLevel { get; private set; }

		public AudioLevels() :
			this( 0.0, 0.0 ) {
		}

		public AudioLevels( double left, double right ) {
			LeftLevel = left;
			RightLevel = right;
		}
	}
}
