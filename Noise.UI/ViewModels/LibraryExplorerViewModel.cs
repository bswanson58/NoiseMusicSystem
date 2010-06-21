using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.UI.Adapters;

namespace Noise.UI.ViewModels {
	class LibraryExplorerViewModel {
		private readonly IUnityContainer			mContainer;
		private	readonly INoiseManager				mNoiseManager;

		private ObservableCollection<ExplorerTreeNode>	mTreeItems;

		public LibraryExplorerViewModel( IUnityContainer container ) {
			mContainer = container;
			mNoiseManager = mContainer.Resolve<INoiseManager>();

			mTreeItems = new ObservableCollection<ExplorerTreeNode>();

			PopulateTreeData();
		}

		private void PopulateTreeData() {
//			mTreeItems = new ObservableCollection<ExplorerTreeNode>((from artist in mNoiseManager.DataProvider.GetArtistList() orderby artist.Name select new ExplorerTreeNode( artist, emptyList )).ToList());

			mTreeItems.Clear();

			var artistList = from artist in mNoiseManager.DataProvider.GetArtistList() orderby artist.Name select artist;
			foreach( DbArtist artist in artistList ) {
				var albumList = from album in mNoiseManager.DataProvider.GetAlbumList( artist ) orderby album.Name select album;

				mTreeItems.Add( new ExplorerTreeNode( artist, albumList ));
			}
		}

		public object TreeData {
			get{ return( mTreeItems ); }
		}
	}
}
