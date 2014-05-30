using System;
using Caliburn.Micro;
using Noise.Infrastructure;

namespace Noise.AppSupport {
	public class ApplicationSupport : IHandle<Events.UrlLaunchRequest>, IHandle<Events.LaunchRequest> {
		private readonly IEventAggregator	mEventAggregator;
		private readonly HotkeyManager		mHotkeyManager;

		public ApplicationSupport( IEventAggregator eventAggregator, HotkeyManager hotkeyManager ) {
			mEventAggregator = eventAggregator;
			mHotkeyManager = hotkeyManager;
		}

		public bool Initialize() {
			mHotkeyManager.Initialize();

			mEventAggregator.Subscribe( this );

			return( true );
		}

		public void Shutdown() {
		}

		public void Handle( Events.UrlLaunchRequest eventArgs ) {
			try {
				System.Diagnostics.Process.Start( eventArgs.Url );
			}
			catch( Exception ) {
				try {
					var startInfo = new System.Diagnostics.ProcessStartInfo( "IExplore.exe", eventArgs.Url );

					System.Diagnostics.Process.Start( startInfo );
				}
				catch( Exception ex ) {
					NoiseLogger.Current.LogException( "Exception - OnWebsiteRequested:", ex );
				}
			}
		}

		public void Handle( Events.LaunchRequest eventArgs ) {
			try {
				System.Diagnostics.Process.Start( eventArgs.Target );
			}
			catch( Exception ex ) {
				NoiseLogger.Current.LogException( "Exception - OnLaunchRequest:", ex );
			}
		}
	}
}
