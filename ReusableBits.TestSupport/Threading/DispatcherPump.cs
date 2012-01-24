﻿using System.Security.Permissions;
using System.Windows.Threading;

namespace ReusableBits.TestSupport.Threading {
	public class DispatcherPump {
		[SecurityPermission( SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode )]
		public static void DoEvents() {
			var frame = new DispatcherFrame();

			Dispatcher.CurrentDispatcher.BeginInvoke( DispatcherPriority.Background, new DispatcherOperationCallback( ExitFrame ), frame );
			Dispatcher.PushFrame( frame );
		}

		private static object ExitFrame( object frame ) {
			((DispatcherFrame)frame ).Continue = false;

			return null;
		}
	}
}
