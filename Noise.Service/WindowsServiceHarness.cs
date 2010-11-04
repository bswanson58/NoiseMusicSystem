using System.ServiceProcess;

namespace Noise.Service {
	partial class WindowsServiceHarness : ServiceBase {
		private readonly IWindowsService	mImplementer;

		public WindowsServiceHarness() {
			InitializeComponent();
		}

		// public constructor to take in the implementation to delegate to
		public WindowsServiceHarness( IWindowsService implementation )
			: this() {
			mImplementer = implementation;
		}

		protected override void OnStart( string[] args ) {
			if( mImplementer != null ) {
				mImplementer.OnStart( args );
			}
		}

		protected override void OnStop() {
			if( mImplementer != null ) {
				mImplementer.OnStop();
			}
		}

		protected override void OnPause() {
			if( mImplementer != null ) {
				mImplementer.OnPause();
			}
		}

		protected override void OnContinue() {
			if( mImplementer != null ) {
				mImplementer.OnContinue();
			}
		}

		protected override void OnShutdown() {
			if( mImplementer != null ) {
				mImplementer.OnShutdown();
			}
		}
	}
}
