using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Practices.Composite.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Adapters;
using Noise.UI.Support;

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

			using( var streams = mNoiseManager.DataProvider.GetStreamList()) {
				mStreams.AddRange( from stream in streams.List select new StreamViewNode( mEventAggregator, stream ));
			}

			mStreams.ResumeNotification();
		}

		public void Execute_AddStream( object sender ) {
			var	stream = new DbInternetStream();

			var	dialogService = mContainer.Resolve<IDialogService>();
			if( dialogService.ShowDialog( DialogNames.InternetStreamEdit, stream ) == true ) {
				mNoiseManager.DataProvider.InsertItem( stream );

				UpdateStreams();
			}
		}

		public void Execute_EditStream( object sender ) {
			if( CurrentStream != null ) {
				EditStream( CurrentStream.Stream );
			}			
		}

		private void EditStream( DbInternetStream stream ) {
			var	dialogService = mContainer.Resolve<IDialogService>();
			if( dialogService.ShowDialog( DialogNames.InternetStreamEdit, stream ) == true ) {
				mNoiseManager.DataProvider.UpdateItem( stream );

				UpdateStreams();
			}
		}

		[DependsUpon( "CurrentStream" )]
		public bool CanExecute_EditStream( object sender ) {
			return( CurrentStream != null );
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
