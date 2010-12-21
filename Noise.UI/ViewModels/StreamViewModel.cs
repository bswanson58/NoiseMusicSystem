using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	class StreamViewModel : ViewModelBase {
		private IUnityContainer		mContainer;
		private IEventAggregator	mEvents;
		private INoiseManager		mNoiseManager;
		private readonly ObservableCollectionEx<UiInternetStream>	mStreams;

		public StreamViewModel() {
			mStreams = new ObservableCollectionEx<UiInternetStream>();
		}

		[Dependency]
		public IUnityContainer Container {
			get { return( mContainer ); }
			set {
				mContainer = value;
				mEvents = mContainer.Resolve<IEventAggregator>();
				mNoiseManager = mContainer.Resolve<INoiseManager>();

				UpdateStreams();
			}
		}

		public ObservableCollection<UiInternetStream> StreamList {
			get{ return( mStreams ); }
		}

		public UiInternetStream CurrentStream {
			get{ return( Get( () => CurrentStream  )); }
			set{ Set( () => CurrentStream, value ); }
		}

		private UiInternetStream MapStream( DbInternetStream dbStream ) {
			var retValue = new UiInternetStream( OnWebsiteClick, OnStreamPlay );

			Mapper.DynamicMap( dbStream, retValue );

			return( retValue );
		}

		private void UpdateStreams() {
			mStreams.SuspendNotification();
			mStreams.Clear();

			using( var streams = mNoiseManager.DataProvider.GetStreamList()) {
				mStreams.AddRange( from stream in streams.List select MapStream( stream ));
			}

			mStreams.ResumeNotification();

			RaiseCanExecuteChangedEvent( "CanExecute_ExportStreams" );
		}

		private void OnWebsiteClick( UiInternetStream stream ) {
			if(( stream != null ) &&
			   (!string.IsNullOrWhiteSpace( stream.Website ))) {
				mEvents.GetEvent<Events.WebsiteRequest>().Publish( stream.Website );
			}
		}

		private void OnStreamPlay( long streamId ) {
			GlobalCommands.PlayStream.Execute( mNoiseManager.DataProvider.GetStream( streamId ));
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
				EditStream( CurrentStream );
			}			
		}

		private void EditStream( UiInternetStream stream ) {
			if( stream != null ) {
				var	dialogService = mContainer.Resolve<IDialogService>();
				using( var dbStream = mNoiseManager.DataProvider.GetStreamForUpdate( stream.DbId )) {
					if(( dbStream != null ) &&
					   ( dbStream.Item != null )) {
						if( dialogService.ShowDialog( DialogNames.InternetStreamEdit, dbStream.Item ) == true ) {
							dbStream.Update();

							UpdateStreams();
						}
					}
				}
			}
		}

		[DependsUpon( "CurrentStream" )]
		public bool CanExecute_EditStream( object sender ) {
			return( CurrentStream != null );
		}

		public void Execute_DeleteStream( object sender ) {
			if( CurrentStream != null ) {
				var dbStream = mNoiseManager.DataProvider.GetStream( CurrentStream.DbId );

				if( dbStream != null ) {
					mNoiseManager.DataProvider.DeleteItem( dbStream );
				}

				UpdateStreams();
			}
		}

		[DependsUpon( "CurrentStream" )]
		public bool CanExecute_DeleteStream( object sender ) {
			return( CurrentStream != null );
		}

		public void Execute_ExportStreams() {
			var dialogService = mContainer.Resolve<IDialogService>();
			var fileName = "";

			if( dialogService.SaveFileDialog( "Export Radio Streams", Constants.ExportFileExtension, "Export Files|*" + Constants.ExportFileExtension, out fileName ) == true ) {
				mNoiseManager.DataExchangeMgr.ExportStreams( fileName );
			}
		}

		public bool CanExecute_ExportStreams() {
			return( mStreams.Count > 0 );
		}
	}
}
