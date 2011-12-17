using System;
using System.Collections.Generic;
using Noise.Infrastructure;

namespace Noise.Core.Support {
	internal class LifecycleManager : ILifecycleManager {
		private readonly List<IRequireInitialization>	mInitializeList;
		private readonly List<IRequireInitialization>	mShutdownList;

		public LifecycleManager() {
			mInitializeList = new List<IRequireInitialization>();
			mShutdownList = new List<IRequireInitialization>();
		}

		public void RegisterForInitialize( IRequireInitialization module ) {
			mInitializeList.Add( module );
		}

		public void RegisterForShutdown( IRequireInitialization module ) {
			mShutdownList.Add( module );
		}

		public void Initialize() {
			try {
				mInitializeList.ForEach( module => module.Initialize());

				mInitializeList.Clear();
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "LifecycleManager:Initialize", ex );	
			}
		}

		public void Shutdown() {
			try {
				mShutdownList.ForEach( module => module.Shutdown());

				mShutdownList.Clear();
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "LifecycleManager:Shutdown", ex );
			}
		}
	}
}
