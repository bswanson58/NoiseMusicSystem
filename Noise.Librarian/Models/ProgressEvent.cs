namespace Noise.Librarian.Models {
	public class ProgressEvent {
		public double	Progress {  get; }
		public bool		IsActive { get; }

		public ProgressEvent( double progress, bool isActive ) {
			Progress = progress;
			IsActive = isActive;
		}
	}
}
