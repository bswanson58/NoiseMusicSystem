using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Support;

namespace Noise.UI.Adapters {
	class StreamViewNode : ViewModelBase {
		private bool						mIsSelected;
		public	DbInternetStream			Stream { get; private set; }

		public StreamViewNode( DbInternetStream stream ) {
			Stream = stream;
		}

		public bool IsSelected {
			get { return mIsSelected; }
			set {
				if( value != mIsSelected ) {
					mIsSelected = value;

					RaisePropertyChanged( () => IsSelected );

					if( mIsSelected ) {
//						mEventAggregator.GetEvent<Events.TrackSelected>().Publish( Stream );
					}
				}
			}
		}

		public void Execute_PlayStream( object sender ) {
			var stream = sender as DbInternetStream;

			if( stream != null ) {
				GlobalCommands.PlayStream.Execute( stream );
			}
		}
	}
}
