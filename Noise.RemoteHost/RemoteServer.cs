﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteDto;
using Noise.Infrastructure.RemoteHost;

namespace Noise.RemoteHost {
	[ServiceBehavior( InstanceContextMode = InstanceContextMode.Single )]
	public class RemoteServer : INoiseRemote,
								IHandle<Events.PlayQueueChanged>, IHandle<Events.PlaybackTrackStarted>,
								IHandle<Events.RemoteTransportUpdate>, IHandle<Events.PlayHistoryChanged> {
		private readonly ILibraryConfiguration				mLibraryConfiguration;
		private readonly IPlayController					mPlayController;
		private readonly IAudioPlayer						mAudioPlayer;
		private readonly RemoteHostConfiguration			mHostConfiguration;
		private readonly INoiseLog							mLog;
		private	readonly Dictionary<string, ClientEvents>	mClientList;

		public RemoteServer( IEventAggregator eventAggregator, ILibraryConfiguration libraryConfiguration,
							 IPlayController playController, IAudioPlayer audioPlayer, RemoteHostConfiguration hostConfiguration, INoiseLog log ) {
			mLibraryConfiguration = libraryConfiguration;
			mPlayController = playController;
			mAudioPlayer = audioPlayer;
			mHostConfiguration = hostConfiguration;
			mLog = log;
			mClientList = new Dictionary<string, ClientEvents>();

			eventAggregator.Subscribe( this );
		}

		public ServerVersion GetServerVersion() {
			var assemblyName = Assembly.GetAssembly( GetType()).GetName();

			return( new ServerVersion { Major = assemblyName.Version.Major,
										Minor = assemblyName.Version.Minor,
										Build = assemblyName.Version.Build,
										Revision = assemblyName.Version.Revision });
		}

		public RoServerInformation GetServerInformation() {
			var	retValue = new RoServerInformation { ApiVersion = (Int16)mHostConfiguration.ApiVersion,
													 ServerVersion = GetServerVersion(),
													 ServerName = mHostConfiguration.ServerName };

			if( mLibraryConfiguration.Current != null ) {
				retValue.LibraryId = mLibraryConfiguration.Current.LibraryId;
				retValue.LibraryName = mLibraryConfiguration.Current.LibraryName;
				retValue.LibraryCount = (Int16)mLibraryConfiguration.Libraries.Count();
			}

			retValue.AudioDevices = mAudioPlayer.GetDeviceList().Select( device => new RoAudioDevice( device )).ToArray();
			if( mAudioPlayer.GetCurrentDevice() != null ) {
				retValue.CurrentAudioDevice = mAudioPlayer.GetCurrentDevice().DeviceId;
			}

			return (retValue);
		}

		public BaseResult SetOutputDevice( int outputDevice ) {
			var retValue = new BaseResult();

			try {
				var device = mAudioPlayer.GetDeviceList().FirstOrDefault( d => d.DeviceId == outputDevice );

				if( device != null ) {
					mPlayController.Stop();
					mAudioPlayer.SetDevice( device );

					retValue.Success = true;
				}
				else {
					retValue.ErrorMessage = "Audio output device cound not be found.";
				}
			}
			catch( Exception ex ) {
				mLog.LogException( "SetOutputDevice:", ex );

				retValue.ErrorMessage = ex.Message;
			}

			return( retValue );
		}

		public BaseResult RequestEvents( string address ) {
			var		retValue = new BaseResult();

			if( mClientList.ContainsKey( address )) {
				var client = mClientList[address];

				try {
					client.Close();
				}
				catch( Exception ex ) {
					mLog.LogException( string.Format( "Closing client \"{0}\"", address ), ex );
				}

				mClientList.Remove( address );
			}

			if(!mClientList.ContainsKey( address )) {
				var client = new ClientEvents( new WebHttpBinding(), new EndpointAddress( address ));

				mClientList.Add( address, client );

				retValue.Success = true;
				mLog.LogMessage( string.Format( "Added remote client: {0}", address ));
			}
			else {
				retValue.ErrorMessage = "Remote client address already registered.";
			}

			return( retValue );
		}

		public BaseResult RevokeEvents( string address ) {
			var		retValue = new BaseResult();

			if( mClientList.ContainsKey( address )) {
				mClientList.Remove( address );

				retValue.Success = true;
				mLog.LogMessage( string.Format( "Removed remote client: {0}", address ));
			}
			else {
				retValue.ErrorMessage = "Client address not located in map.";
			}

			return( retValue );
		}

		public void Handle( Events.PlayQueueChanged eventArgs ) {
			// decouple from the event thread.
			new Task( OnQueueChangedTask ).Start();
		}

		public void Handle( Events.PlaybackTrackStarted eventArgs ) {
			// decouple from the event thread.
			new Task( OnQueueChangedTask ).Start();
		}

		public void Handle( Events.RemoteTransportUpdate args ) {
			new Task( () => OnTransportChanged( args.TransportState )).Start();
		}

		public void Handle( Events.PlayHistoryChanged args ) {
			new Task( () => OnDataChanged( LibraryStateChanged.PlayHistory )).Start();
		}

		private void OnQueueChangedTask() {
			foreach( var client in mClientList.Values ) {
				client.EventInQueue();
			}
		}

		private void OnTransportChanged( RoTransportState transportState ) {
			foreach( var client in mClientList.Values ) {
				client.EventInTransport( transportState );
			}
		}

		private void OnDataChanged( LibraryStateChanged stateChanged ) {
			foreach( var client in mClientList.Values ) {
				client.EventInLibrary( stateChanged );
			}
		}
	}
}
