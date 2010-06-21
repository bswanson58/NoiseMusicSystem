using System.Collections.ObjectModel;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;

namespace Noise.UI.ViewModels {
	class LibraryExplorerViewModel {
		private readonly IUnityContainer			mContainer;
		private	readonly INoiseManager				mNoiseManager;
		private readonly ObservableCollection<DbArtist>		mTreeItems;

		public LibraryExplorerViewModel( IUnityContainer container ) {
			mContainer = container;
			mNoiseManager = mContainer.Resolve<INoiseManager>();

			mTreeItems = new ObservableCollection<DbArtist>( mNoiseManager.DataProvider.GetArtistList());
		}

		public ObservableCollection<DbArtist> TreeData {
			get{ return( mTreeItems ); }
		}
	}
}
