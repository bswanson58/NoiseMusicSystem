using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.UI.Adapters;

namespace Noise.UI.ViewModels {
	public class LibraryExplorerViewModel {
		private IUnityContainer		mContainer;
		private IEventAggregator	mEventAggregator;
		private	INoiseManager		mNoiseManager;

		private readonly ObservableCollection<ExplorerTreeNode>	mTreeItems;

		public LibraryExplorerViewModel() {
			mTreeItems = new ObservableCollection<ExplorerTreeNode>();
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEventAggregator = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				PopulateTreeData();
			}
		}

		private void PopulateTreeData() {
			mTreeItems.Clear();

			var artistList = from artist in mNoiseManager.DataProvider.GetArtistList() orderby artist.Name select artist;
			foreach( DbArtist artist in artistList ) {
				var parent = new ExplorerTreeNode( mEventAggregator, artist );
				parent.SetChildren( from album in mNoiseManager.DataProvider.GetAlbumList( artist ) orderby album.Name select new ExplorerTreeNode( mEventAggregator, parent, album ));
				mTreeItems.Add( parent );
			}
		}

		public object TreeData {
			get{ return( mTreeItems ); }
		}
	}
}
