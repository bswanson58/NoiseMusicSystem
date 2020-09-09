using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Interfaces;
using Noise.Infrastructure.RemoteHost;
using ReusableBits.Platform;

namespace Noise.Core {
	public class NoiseManager : INoiseManager, IHandle<Events.DatabaseOpened>, IHandle<Events.DatabaseClosing> {
		private	readonly IEventAggregator		mEvents;
		private readonly INoiseLog				mLog;
		private readonly ILifecycleManager		mLifecycleManager;
		private readonly IPreferences			mPreferences;
		private readonly IRemoteServer			mRemoteServer;
		private readonly ILibraryBuilder		mLibraryBuilder;
		private readonly IDatabaseManager		mDatabaseManager;
		private readonly IPlayController		mPlayController;

		public NoiseManager( IEventAggregator eventAggregator,
							 INoiseLog log,
							 ILifecycleManager lifecycleManager,
							 IDatabaseManager databaseManager,
							 ILibraryBuilder libraryBuilder,
							 IRemoteServer remoteServer,
							 IPreferences preferences,
                             IPlayController playController,
							 // components that just need to be referenced.
							 // ReSharper disable UnusedParameter.Local
							 IAudioController audioController,
							 ILibraryBackupManager backupManager,
							 ISearchProvider searchProvider,
							 ITagManager tagManager,
							 IMetadataManager metadataManager,
							 IEnumerable<IRequireConstruction> backgroundComponents ) {
							 // ReSharper restore UnusedParameter.Local
			mEvents = eventAggregator;
			mLog = log;
			mLifecycleManager = lifecycleManager;
			mRemoteServer = remoteServer;
			mDatabaseManager = databaseManager;
			mLibraryBuilder = libraryBuilder;
			mPreferences = preferences;
			mPlayController = playController;
		}

		public Task<bool> AsyncInitialize() {
			return Task.Run( InitializeAndNotify );
		}

		public bool InitializeAndNotify() {
			var retValue = Initialize();

			mEvents.BeginPublishOnUIThread( new Events.NoiseSystemReady( this, retValue ));

			return( retValue );
		}

		public bool Initialize() {
			var isInitialized = false;

			mLog.LogMessage( $"Initializing {VersionInformation.ProductName} v{VersionInformation.Version}" );

			try {
			    // add support for TLS v1.2
			    System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12;

				mLifecycleManager.Initialize();
				mEvents.Subscribe( this );

				// we're initialized regardless of the status of the remote server.
				isInitialized = true;

				var preferences = mPreferences.Load<NoiseCorePreferences>();

				if( preferences.EnableRemoteAccess ) {
					mRemoteServer.OpenRemoteServer();
				}

				mLog.LogMessage( "Noise Music System initialized" );
			}
			catch( Exception ex ) {
				mLog.LogException( "Initialization failed", ex );
			}

			return ( isInitialized );
		}

		public bool CanShutDown( out string reason ) {
			var retValue = true;

			reason = String.Empty;

			if( mLibraryBuilder.LibraryUpdateInProgress ) {
				reason = "A library update is in progress.";
				retValue = false;
            }
			else if( mPlayController.CanStop ) {
				reason = "Music is currently playing.";
				retValue = false;
            }

			return retValue;
        }

		public void Shutdown() {
			mEvents.PublishOnUIThread( new Events.SystemShutdown());

			mRemoteServer.CloseRemoteServer();
			mLibraryBuilder.StopLibraryUpdate();

			mLifecycleManager.Shutdown();

			mDatabaseManager.Shutdown();
		}

		public void Handle( Events.DatabaseOpened arg ) {
			mLibraryBuilder.LogLibraryStatistics();
		}

		public void Handle( Events.DatabaseClosing args ) {
			mLibraryBuilder.StopLibraryUpdate();
		}
	}
}
