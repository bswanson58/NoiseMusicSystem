using Noise.Infrastructure.Interfaces;

namespace Noise.Librarian.ViewModels {
	public class BackupDatabaseViewModel {
		private readonly ILibraryConfiguration	mLibraryConfiguration;

		public BackupDatabaseViewModel( ILibraryConfiguration libraryConfiguration ) {
			mLibraryConfiguration = libraryConfiguration;
		}
	}
}
