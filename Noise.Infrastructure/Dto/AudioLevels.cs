namespace Noise.Infrastructure.Dto {
	public class AudioLevels {
		public	double		LeftLevel { get; private set; }
		public	double		RightLevel { get; private set; }

		public AudioLevels( double left, double right ) {
			LeftLevel = left;
			RightLevel = right;
		}
	}
}
