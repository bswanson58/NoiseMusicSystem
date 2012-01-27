using Caliburn.Micro;
using Noise.AppSupport.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;

namespace Noise.AppSupport {
	public class HotkeyManager : IHandle<Events.SystemShutdown> {
		private readonly ICaliburnEventAggregator	mEvents;
		private	KeyboardHook				mKeyboardHook;
		private bool						mInstalled;

		public HotkeyManager( ICaliburnEventAggregator eventAggregator ) {
			mEvents = eventAggregator;

			mEvents.Subscribe( this );
		}

		public void Initialize() {
			var configuration = NoiseSystemConfiguration.Current.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

			if( configuration.EnableGlobalHotkeys ) {
				mKeyboardHook = new KeyboardHook();
				mKeyboardHook.KeyDown += OnKeyDown;
				mKeyboardHook.Install();
				mInstalled = true;
			}
		}

		private void OnKeyDown( KeyboardHook.VKeys key ) {
			var action = UserEventAction.None;

			switch( key ) {
				case KeyboardHook.VKeys.F9:
					action = UserEventAction.PausePlay;
					break;

				case KeyboardHook.VKeys.F10:
					action = UserEventAction.PlayPreviousTrack;
					break;

				case KeyboardHook.VKeys.F11:
					action = UserEventAction.PlayNextTrack;
					break;

				case KeyboardHook.VKeys.F12:
					action = UserEventAction.StartPlay;
					break;
			}

			if( action != UserEventAction.None ) {
				mEvents.Publish( new Events.GlobalUserEvent( new GlobalUserEventArgs( action )));
			}
		}

		public void Handle( Events.SystemShutdown eventArgs ) {
			if(( mKeyboardHook != null ) &&
			   ( mInstalled )) {
				mKeyboardHook.Uninstall();

				mInstalled = false;
			}
		}
	}
}
