﻿using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Unity;
using Noise.AppSupport.Support;
using Noise.Infrastructure;
using Noise.Infrastructure.Configuration;
using Noise.Infrastructure.Dto;

namespace Noise.AppSupport {
	public class HotkeyManager {
		private readonly IUnityContainer	mContainer;
		private readonly IEventAggregator	mEvents;
		private	KeyboardHook				mKeyboardHook;
		private bool						mInstalled;

		public HotkeyManager( IUnityContainer container ) {
			mContainer = container;
			mEvents = mContainer.Resolve<IEventAggregator>();

			mEvents.GetEvent<Events.SystemShutdown>().Subscribe( OnShutdown );
		}

		public void Initialize() {
			var	systemConfig = mContainer.Resolve<ISystemConfiguration>();
			var configuration = systemConfig.RetrieveConfiguration<ExplorerConfiguration>( ExplorerConfiguration.SectionName );

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
				mEvents.GetEvent<Events.GlobalUserEvent>().Publish( new GlobalUserEventArgs( action ));
			}
		}

		private void OnShutdown( object unused ) {
			if(( mKeyboardHook != null ) &&
			   ( mInstalled )) {
				mKeyboardHook.Uninstall();

				mInstalled = false;
			}
		}
	}
}
