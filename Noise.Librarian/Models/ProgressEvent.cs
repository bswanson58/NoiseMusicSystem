using System;

namespace Noise.Librarian.Models {
	public class ProgressEvent {
		public double	Progress {  get; private set; }
		public bool		IsActive { get; private set; }

		public ProgressEvent( double progress, bool isActive ) {
			Progress = progress;
			IsActive = isActive;
		}
	}
}
