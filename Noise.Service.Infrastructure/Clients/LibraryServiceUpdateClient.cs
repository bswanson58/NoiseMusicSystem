using System;
using System.Collections.Generic;
using System.ServiceModel;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Service.Infrastructure.ServiceContracts;

namespace Noise.Service.Infrastructure.Clients {
	public class LibraryServiceUpdateClient : ILibraryBuilder {
		private readonly ILibraryUpdateService	mLibraryService;
		private readonly ChannelFactory<ILibraryUpdateService>	mServiceProxy;

		public LibraryServiceUpdateClient() {
			try {
				mServiceProxy = new ChannelFactory<ILibraryUpdateService>( "Noise.ServiceImpl.LibraryUpdate.LibraryUpdateService" );
				mServiceProxy.Open();
				mLibraryService = mServiceProxy.CreateChannel();		
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - LibraryServiceUpdateClient:", ex );
			}
		}

		public void StartLibraryUpdate() {
			if( mLibraryService != null ) {
				try {
					mLibraryService.StartLibraryUpdate();
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - LibraryServiceUpdateClient:StartLibraryUpdate ", ex );
				}
			}
		}

		public void StopLibraryUpdate() {
/*
			if(( mLibraryService != null ) &&
			   ( mLibraryUpdating ) &&
			   ( mServiceProxy.State == CommunicationState.Opened )) {
				try {
					mLibraryService.StopLibraryUpdate();

					mLibraryUpdating = false;
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - LibraryServiceUpdateClient:StopLibraryUpdate ", ex );
				}
			}
*/		}

		public bool EnableUpdateOnLibraryChange {
			get { return( true ); }
			set { }
		}
		public bool LibraryUpdateInProgress {
			get {
				var retValue = false;

				if( mLibraryService != null ) {
					try {
						retValue = mLibraryService.IsLibraryUpdateInProgress();
					}
					catch( Exception ex ) {
						NoiseLogger.Current.LogException( "Exception - LibraryServiceUpdateClient:LibraryUpdateInProgress ", ex );
					}
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
