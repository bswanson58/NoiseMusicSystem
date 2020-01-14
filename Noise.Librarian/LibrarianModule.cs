using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;
using Noise.Librarian.Models;

namespace Noise.Librarian {
	public class LibrarianModule : IModule {
		private readonly IUnityContainer    mContainer;

		public LibrarianModule( IUnityContainer container ) {
			mContainer = container;
		}
		public void Initialize() {
			mContainer.RegisterType<ILibrarian, LibrarianModel>( new HierarchicalLifetimeManager());
			mContainer.RegisterType<IDirectoryArchiver, DirectoryArchiver>();
		}
	}
}
