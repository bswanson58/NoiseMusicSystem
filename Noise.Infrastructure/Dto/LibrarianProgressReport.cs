namespace Noise.Infrastructure.Dto {
	public class LibrarianProgressReport {
		public	string		CurrentPhase { get; }
		public	string		CurrentItem { get; }
		public	int			Progress { get; }
		public	bool		Completed { get; }

		public LibrarianProgressReport( string phase, string item, int progress ) {
			CurrentPhase = phase;
			CurrentItem = item;
			Progress = progress;

			Completed = false;
		}

		public LibrarianProgressReport( string phase, string item ) :
			this( phase, item, 100 ) {
			Completed = true;
		}
	}
}
