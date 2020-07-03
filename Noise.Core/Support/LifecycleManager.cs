using System;
using System.Collections.Generic;
using Noise.Infrastructure.Interfaces;

namespace Noise.Core.Support {
	internal class LifecycleManager : ILifecycleManager {
		private readonly INoiseLog						mLog;
		private readonly List<IRequireInitialization>	mInitializeList;
		private readonly List<IRequireInitialization>	mShutdownList;
		private bool									mIsInitialized;

		public LifecycleManager( INoiseLog log ) {
			mLog = log;
			mInitializeList = new List<IRequireInitialization>();
			mShutdownList = new List<IRequireInitialization>();

			mIsInitialized = false;
		}

		public void RegisterForInitialize( IRequireInitialization module ) {
			if( mIsInitialized ) {
				InitializeModule( module );
            }
			else {
                mInitializeList.Add( module );
            }
		}

		public void RegisterForShutdown( IRequireInitialization module ) {
			mShutdownList.Add( module );
		}

		public void Initialize() {
			foreach( var module in mInitializeList ) {
				InitializeModule( module );
			}

			mInitializeList.Clear();
			mIsInitialized = true;
		}

		private void InitializeModule( IRequireInitialization module ) {
            try {
                module.Initialize();
            }
            catch( Exception ex ) {
                mLog.LogException( $"Initialize of :{module.GetType()}", ex );	
            }
        }

		public void Shutdown() {
			foreach( var module in mShutdownList ) {
				try {
					module.Shutdown();
				}
				catch( Exception ex ) {
					mLog.LogException( "Attempting shutdown", ex );
				}
			}

			mShutdownList.Clear();
		}
	}
}
