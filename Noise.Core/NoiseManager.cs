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

		public NoiseManager( IEventAggregator eventAggregator,
							 INoiseLog log,
							 ILifecycleManager lifecycleManager,
							 IDatabaseManager databaseManager,
							 ILibraryBuilder libraryBuilder,
							 IRemoteServer remoteServer,
							 IPreferences preferences,
							 // components that just need to be referenced.
							 // ReSharper disable UnusedParameter.Local
							 IAudioController audioController,
							 IPlayController playController,
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
		}

		public async Task<bool> AsyncInitialize() {
			var initTask = new Task<bool>(() => ( InitializeAndNotify()));

			initTask.Start();

			return( await initTask );
		}

		public bool InitializeAndNotify() {
			var retValue = Initialize();

			mEvents.PublishOnUIThreadAsync( new Events.NoiseSystemReady( this, retValue ));

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
