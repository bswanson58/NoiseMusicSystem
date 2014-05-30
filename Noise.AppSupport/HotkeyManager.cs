using Caliburn.Micro;
using Noise.AppSupport.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;
using Noise.Infrastructure.Interfaces;

namespace Noise.AppSupport {
	public class HotkeyManager : IHandle<Events.SystemShutdown> {
		private readonly IEventAggregator	mEvents;
		private readonly IPreferences		mPreferences;
		private	KeyboardHook				mKeyboardHook;
		private bool						mInstalled;

		public HotkeyManager( IEventAggregator eventAggregator, IPreferences preferences ) {
			mEvents = eventAggregator;
			mPreferences = preferences;

			mEvents.Subscribe( this );
		}

		public void Initialize() {
			var configuration = mPreferences.Load<UserInterfacePreferences>();

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
