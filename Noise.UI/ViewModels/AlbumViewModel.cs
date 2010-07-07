using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.UI.ViewModels {
	internal class AlbumViewModel : ViewModelBase {
		private IUnityContainer		mContainer;
		private IEventAggregator	mEvents;
		private INoiseManager		mNoiseManager;
		private DbAlbum				mCurrentAlbum;

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				mEvents.GetEvent<Events.AlbumFocusRequested>().Subscribe( OnAlbumFocus );
			}
		}

		private AlbumSupportInfo SupportInfo {
			get{ return( Get( () => SupportInfo )); }
			set{ Set( () => SupportInfo, value );  }
		}

		public void OnAlbumFocus( DbAlbum album ) {
			mCurrentAlbum = album;

			if( mCurrentAlbum != null ) {
				SupportInfo = mNoiseManager.DataProvider.GetAlbumSupportInfo( mCurrentAlbum );
			}
		}

	}
}
