using System;
using System.Collections.Generic;
using Noise.Infrastructure;
using Noise.Infrastructure.Interfaces;

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
			foreach( var module in mInitializeList ) {
				try {
					module.Initialize();
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "LifecycleManager:Initialize", ex );	
				}
			}

			mInitializeList.Clear();
		}

		public void Shutdown() {
			foreach( var module in mShutdownList ) {
				try {
					module.Shutdown();
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "LifecycleManager:Shutdown", ex );
				}
			}

			mShutdownList.Clear();
		}
	}
}
