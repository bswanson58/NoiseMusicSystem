using Noise.Infrastructure.Dto;

namespace Noise.Librarian.Interfaces {
	public interface ILibrarian {
		bool	Initialize();
		void	Shutdown();

		void	BackupDatabase( LibraryConfiguration library );
	}
}
