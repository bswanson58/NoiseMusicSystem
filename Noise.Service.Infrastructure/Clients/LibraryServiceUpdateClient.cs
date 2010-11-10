using System;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.Practices.Unity;
using Noise.Infrastructure.Interfaces;
using Noise.Service.Infrastructure.ServiceContracts;

namespace Noise.Service.Infrastructure.Clients {
	public class LibraryServiceUpdateClient : ILibraryBuilder {
		private readonly IUnityContainer		mContainer;
		private readonly ILibraryUpdateService	mLibraryService;
		private	readonly ILog					mLog;
		private readonly ChannelFactory<ILibraryUpdateService>	mServiceProxy;

		public LibraryServiceUpdateClient( IUnityContainer container ) {
			mContainer = container;
			mLog = mContainer.Resolve<ILog>();

			try {
				mServiceProxy = new ChannelFactory<ILibraryUpdateService>( "Noise.ServiceImpl.LibraryUpdate.LibraryUpdateService" );
				mServiceProxy.Open();
				mLibraryService = mServiceProxy.CreateChannel();		
			}
			catch( Exception ex ) {
				mLog.LogException( "Exception - LibraryServiceUpdateClient:", ex );
			}
		}

		public void StartLibraryUpdate() {
			if( mLibraryService != null ) {
				mLibraryService.StartLibraryUpdate();
			}
		}

		public void StopLibraryUpdate() {
			if(( mLibraryService != null ) &&
			   ( mServiceProxy.State == CommunicationState.Opened )) {
				mLibraryService.StopLibraryUpdate();
			}
		}

		public bool LibraryUpdateInProgress {
			get {
				var retValue = false;

				if( mLibraryService != null ) {
					retValue = mLibraryService.IsLibraryUpdateInProgress();
				}

				return( retValue );
			}
		}

		public bool LibraryUpdatePaused {
			get{ return( false ); }
		}

		public void LogLibraryStatistics() {
		}

		public IEnumerable<string> RootFolderList() {
			return( new List<string>());
		}
	}
}
