﻿using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Windows.Threading;
using Noise.Infrastructure.RemoteDto;
using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	public class ClientEvents : ClientBase<INoiseEvents> {
		private INoiseEvents	mService;

		public ClientEvents( Binding binding, EndpointAddress address ) :
			base( binding, address ) {

			var factory = new ChannelFactory<INoiseEvents>( binding, address );
			factory.Endpoint.Behaviors.Add(new WebHttpBehavior());
			mService = factory.CreateChannel();

			// Set the server response to messages to extend up to five minutes.
			((IContextChannel)mService).OperationTimeout = new TimeSpan( 0, 5, 0 );
		}

		public bool CloseClient() {
			var retValue =true;

			if( mService != null ) {
				if(((IContextChannel)mService).State == CommunicationState.Opened ) {
					try {
						((IContextChannel)mService).Close();
					}
					catch {
						retValue = false;
					}
					finally {
						mService = null;
					}
				}
			}

			return( retValue );
		}

		// EventInQueue
		public event EventHandler<ClientCallbackArgs<VoidCallbackArgs>> EventCompleted;
		public void EventInQueue() {
			lock( this ) {
				mService.BeginEventInQueue( EventInQueueComplete, null );
			}
		}
		private void EventInQueueComplete( IAsyncResult result ) {
			ServiceCallbackHandler( mService.EndEventInQueue, result, EventCompleted );
		}

		private int mSequence;

		// EventInTransport
		public void EventInTransport( RoTransportState transportState ) {
			lock( this ) {
				mService.BeginEventInTransport( mSequence++, transportState.PlayState, transportState.ServerTime, transportState.CurrentTrack,
												transportState.CurrentTrackPosition, transportState.CurrentTrackLength, EventInTransportComplete, null );
			}
		}
		private void EventInTransportComplete( IAsyncResult result ) {
			ServiceCallbackHandler( mService.EndEventInTransport, result, EventCompleted );
		}

		// EventInLibrary
		public void EventInLibrary( LibraryStateChanged stateChanged ) {
			lock( this ) {
				mService.BeginEventInLibrary( stateChanged.ToString(), EventInLibraryComplete, null );
			}
		}
		private void EventInLibraryComplete( IAsyncResult result ) {
			ServiceCallbackHandler( mService.EndEventInLibrary, result, EventCompleted );
		}

		protected void ServiceCallbackHandler<T>( Func<IAsyncResult, T> method, IAsyncResult result, EventHandler<ClientCallbackArgs<T>> notifier ) where T : class {
			if ( notifier != null ) {
				Exception exception = null;
				T callResult = null;

				try {
					callResult = method( result );
				}
				catch ( Exception e ) {
					exception = e;
				}
				finally {
					if(  Dispatcher.CurrentDispatcher.CheckAccess()) {
						notifier( this, new ClientCallbackArgs<T>( callResult, exception ));
					}
					else {
						Dispatcher.CurrentDispatcher.BeginInvoke( notifier, this, new ClientCallbackArgs<T>( callResult, exception ));
					}

					if( State == CommunicationState.Faulted ) {
						Abort();
					}
				}
			}
		}
	}

	public class ClientCallbackArgs<T> : EventArgs where T : class {
		public readonly T			Result;
		public readonly Exception	Error;

		public ClientCallbackArgs( T result, Exception e ) {
			Result = result;
			Error = e;
		}
	}
}
