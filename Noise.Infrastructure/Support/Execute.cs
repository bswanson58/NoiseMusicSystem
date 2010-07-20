using System;
using System.Windows;

namespace Noise.Infrastructure.Support {
	public static class Execute {
		private static Action<Action> mExecutor = action => action();

		public static void InitializeWithDispatcher() {
			var dispatcher = Application.Current.Dispatcher;

			mExecutor = action => {
				if( dispatcher.CheckAccess()) {
					action();
				}
				else {
					dispatcher.BeginInvoke( action );
				}
			};
		}

		public static void OnUiThread( this Action action ) {
			mExecutor( action );
		}
	}
}
