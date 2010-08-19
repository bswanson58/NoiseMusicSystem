using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;

namespace Noise.UI.ViewModels {
	class StreamViewModel : ViewModelBase {
		private IUnityContainer		mContainer;
		private IEventAggregator	mEventAggregator;
		private INoiseManager		mNoiseManager;
		private readonly ObservableCollectionEx<StreamViewNode>	mStreams;

		public StreamViewModel() {
			mStreams = new ObservableCollectionEx<StreamViewNode>();
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;

				mEventAggregator = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				UpdateStreams();
			}
		}

		public ObservableCollection<StreamViewNode> StreamList {
			get{ return( mStreams ); }
		}

		public StreamViewNode CurrentStream {
			get{ return( Get( () => CurrentStream  )); }
			set{ Set( () => CurrentStream, value ); }
		}

		private void UpdateStreams() {
			mStreams.SuspendNotification();
			mStreams.Clear();
			mStreams.AddRange( from stream in mNoiseManager.DataProvider.GetStreamList() select new StreamViewNode( mEventAggregator, stream ));
			mStreams.ResumeNotification();
		}

		public void Execute_AddStream( object sender ) {
			var stream = new DbInternetStream { Name = "WXRT", Description = "WXRT in Chicago", Url = "http://provisioning.streamtheworld.com/pls/WXRTFMAAC.pls" };
			mNoiseManager.DataProvider.UpdateItem( stream );

			UpdateStreams();
		}

		public void Execute_DeleteStream( object sender ) {
			if( CurrentStream != null ) {
				mNoiseManager.DataProvider.DeleteItem( CurrentStream.Stream );

				UpdateStreams();
			}
		}

		[DependsUpon( "CurrentStream" )]
		public bool CanExecute_DeleteStream( object sender ) {
			return( CurrentStream != null );
		}
	}
}
