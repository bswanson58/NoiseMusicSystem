using System.Collections.Generic;

namespace Noise.Infrastructure.Interfaces {
	public interface ILibraryBuilder {
		void	StartLibraryUpdate();
//		void	PauseLibraryUpdate();
//		void	ResumeLibraryUpdate();
		void	StopLibraryUpdate();

		bool	EnableUpdateOnLibraryChange { get; set; }

		bool	LibraryUpdateInProgress { get; }
		bool	LibraryUpdatePaused { get; }

		void	LogLibraryStatistics();

		IEnumerable<string>	RootFolderList();
	}
}
