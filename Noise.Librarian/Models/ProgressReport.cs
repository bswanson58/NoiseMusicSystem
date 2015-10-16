namespace Noise.Librarian.Models {
	public class ProgressReport {
		public	string		CurrentPhase { get; private set; }
		public	string		CurrentItem { get; private set; }
		public	int			Progress { get; private set; }
		public	bool		Completed { get; private set; }

		public ProgressReport( string phase, string item, int progress ) {
			CurrentPhase = phase;
			CurrentItem = item;
			Progress = progress;

			Completed = false;
		}

		public ProgressReport( string phase, string item ) :
			this( phase, item, 100 ) {
			Completed = true;
		}
	}
}
