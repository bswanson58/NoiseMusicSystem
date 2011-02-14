using System;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;

namespace Noise.UI.ViewModels {
	public class ArtistTracksViewModel : ViewModelBase, IActiveAware {
		private IUnityContainer				mContainer;
		private IEventAggregator			mEvents;
		private INoiseManager				mNoiseManager;
		private DbArtist					mCurrentArtist;
		private	bool						mIsActive;

		public	event EventHandler			IsActiveChanged;

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();
			}
		}

		public bool IsActive {
			get { return( mIsActive ); }
			set {
				mIsActive = value;

				if( mIsActive ) {
					mEvents.GetEvent<Events.ArtistFocusRequested>().Subscribe( OnArtistFocus );
				}
				else {
					mEvents.GetEvent<Events.ArtistFocusRequested>().Unsubscribe( OnArtistFocus );
				}
			}
		}

		private void OnArtistFocus( DbArtist artist ) {
		}

		public void Execute_SwitchView() {
			mEvents.GetEvent<Events.NavigationRequest>().Publish( new NavigationRequestArgs( ViewNames.ArtistTracksView ));
		}
	}
}
