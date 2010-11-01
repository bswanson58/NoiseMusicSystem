namespace Noise.Infrastructure.Interfaces {
	public interface ILibraryBuilder {
		void	StartLibraryUpdate();
		void	PauseLibraryUpdate();
		void	ResumeLibraryUpdate();
		void	StopLibraryUpdate();

		bool	LibraryUpdateInProgress { get; }
		bool	LibraryUpdatePaused { get; }

		void	LogLibraryStatistics();
	}
}
