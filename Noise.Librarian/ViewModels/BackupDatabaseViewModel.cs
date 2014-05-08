using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

namespace Noise.Librarian.ViewModels {
	public class BackupDatabaseViewModel : IHandle<Events.SystemInitialized> {
		private readonly ILibraryConfiguration	mLibraryConfiguration;

		public BackupDatabaseViewModel( IEventAggregator eventAggregator, ILibraryConfiguration libraryConfiguration ) {
			mLibraryConfiguration = libraryConfiguration;

			eventAggregator.Subscribe( this );
		}

		public void Handle( Events.SystemInitialized message ) {
		}
	}
}
