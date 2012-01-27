using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.Support;
using Noise.UI.Dto;
using Noise.UI.Support;

namespace Noise.UI.ViewModels {
	class StreamViewModel : ViewModelBase {
		private readonly IEventAggregator			mEventAggregator;
		private readonly IInternetStreamProvider	mStreamProvider;
		private readonly IDataExchangeManager		mDataExchangeMgr;
		private readonly IDialogService				mDialogService;
		private readonly ObservableCollectionEx<UiInternetStream>	mStreams;

		public StreamViewModel( IEventAggregator eventAggregator, IDialogService dialogService,
								IInternetStreamProvider streamProvider, IDataExchangeManager dataExchangeManager ) {
			mEventAggregator = eventAggregator;
			mStreamProvider = streamProvider;
			mDataExchangeMgr = dataExchangeManager;
			mDialogService = dialogService;

			mStreams = new ObservableCollectionEx<UiInternetStream>();

			UpdateStreams();
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

			using( var streams = mStreamProvider.GetStreamList()) {
				mStreams.AddRange( from stream in streams.List select MapStream( stream ));
			}

			mStreams.ResumeNotification();

			RaiseCanExecuteChangedEvent( "CanExecute_ExportStreams" );
		}

		private void OnWebsiteClick( UiInternetStream stream ) {
			if(( stream != null ) &&
			   (!string.IsNullOrWhiteSpace( stream.Website ))) {
				mEventAggregator.Publish( new Events.UrlLaunchRequest( stream.Website ));
			}
		}

		private void OnStreamPlay( long streamId ) {
			GlobalCommands.PlayStream.Execute( mStreamProvider.GetStream( streamId ));
		}

		public void Execute_AddStream( object sender ) {
			var	stream = new DbInternetStream();

			if( mDialogService.ShowDialog( DialogNames.InternetStreamEdit, stream ) == true ) {
				mStreamProvider.AddStream( stream );

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
				using( var dbStream = mStreamProvider.GetStreamForUpdate( stream.DbId )) {
					if(( dbStream != null ) &&
					   ( dbStream.Item != null )) {
						if( mDialogService.ShowDialog( DialogNames.InternetStreamEdit, dbStream.Item ) == true ) {
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
				var dbStream = mStreamProvider.GetStream( CurrentStream.DbId );

				if( dbStream != null ) {
					mStreamProvider.DeleteStream( dbStream );
				}

				UpdateStreams();
			}
		}

		[DependsUpon( "CurrentStream" )]
		public bool CanExecute_DeleteStream( object sender ) {
			return( CurrentStream != null );
		}

		public void Execute_ExportStreams() {
			string fileName;

			if( mDialogService.SaveFileDialog( "Export Radio Streams", Constants.ExportFileExtension, "Export Files|*" + Constants.ExportFileExtension, out fileName ) == true ) {
				mDataExchangeMgr.ExportStreams( fileName );
			}
		}

		public bool CanExecute_ExportStreams() {
			return( mStreams.Count > 0 );
		}
	}
}
