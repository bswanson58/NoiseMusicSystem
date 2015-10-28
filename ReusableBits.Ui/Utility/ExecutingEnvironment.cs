using System;
using System.Windows;

namespace ReusableBits.Ui.Utility {
	public class ExecutingEnvironment {
		public static void ResizeWindowIntoVisibility( Window window ) {
			var workArea = SystemParameters.WorkArea;

			var screenLeft = SystemParameters.VirtualScreenLeft;
			var screenRight = SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth;
			var screenTop = Math.Max( SystemParameters.VirtualScreenTop, workArea.Top );
			var screenBottom = Math.Min( workArea.Bottom, SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight );

			window.Left = Math.Max( window.Left, screenLeft );
			window.Left = Math.Min( window.Left, screenRight - 800 );
			window.Top = Math.Max( window.Top, screenTop );
			window.Top = Math.Min( window.Top, screenBottom - 600 );

			window.Width = Math.Min( window.Width, screenRight - window.Left );
			window.Height = Math.Min( window.Height, screenBottom - window.Top );
		}

		public static void MoveWindowIntoVisibility( Window window ) {
			var workArea = SystemParameters.WorkArea;

			var screenLeft = SystemParameters.VirtualScreenLeft;
			var screenRight = SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth;
			var screenTop = Math.Max( SystemParameters.VirtualScreenTop, workArea.Top );
			var screenBottom = Math.Min( workArea.Bottom, SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight );

			window.Left = Math.Max( window.Left, screenLeft );
			window.Top = Math.Max( window.Top, screenTop );

			window.Left = Math.Min( window.Left, screenRight - window.Width );
			window.Top = Math.Min( window.Top, screenBottom - window.Height );
		}
	}
}
