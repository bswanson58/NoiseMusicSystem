using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using Microsoft.Practices.Unity;
using Noise.AppSupport;
using ReusableBits.Service;

namespace Noise.Headless {
	internal sealed class SingleThreadSynchronizationContext : SynchronizationContext {
		private readonly BlockingCollection<KeyValuePair<SendOrPostCallback, object>>
			mQueue = new BlockingCollection<KeyValuePair<SendOrPostCallback, object>>();

		public override void Post( SendOrPostCallback d, object state ) {
			mQueue.Add( new KeyValuePair<SendOrPostCallback, object>( d, state ));
		}

		public void RunOnCurrentThread() {
			KeyValuePair<SendOrPostCallback, object> workItem;
			while( mQueue.TryTake( out workItem, Timeout.Infinite )) {
				workItem.Key( workItem.Value );
			}
		}

		public void Complete() {
			mQueue.CompleteAdding();
		}
	}

	static class ServiceMain {
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main( string[] args ) {
			var iocConfig = new IocConfiguration();

			iocConfig.InitializeIoc( ApplicationUsage.Server );
			iocConfig.Container.RegisterType<IWindowsService, HeadlessService>();

			var syncCtx = new SingleThreadSynchronizationContext();
			SynchronizationContext.SetSynchronizationContext( syncCtx );
	
			using( var serviceImpl = iocConfig.Container.Resolve<IWindowsService>()) {
				// if install was a command line flag, then run the installer at runtime.
				if( args.Contains( "-install", StringComparer.InvariantCultureIgnoreCase )) {
					WindowsServiceInstaller.InstallService( serviceImpl );
				}
				else if( args.Contains( "-uninstall", StringComparer.InvariantCultureIgnoreCase )) {
					WindowsServiceInstaller.UnInstallService( serviceImpl );
				}
				else {
					// if started from console, file explorer, etc, run as console app.
					if( Environment.UserInteractive ) {
						ConsoleServiceHarness.Run( args, serviceImpl );
					}

					// otherwise run as a windows service
					else {
						ServiceBase.Run( new WindowsServiceHarness( serviceImpl ));
					}
				}
			}
		}
	}
}
