using System.Windows;
using System.Windows.Input;

namespace Noise.TenFoot.Ui.Controls.LoopingListBox {
	public delegate void BeginDragEventHandler( object sender, BeginDragEventArgs e );

	public class BeginDragEventArgs : RoutedEventArgs {
		public BeginDragEventArgs( InputDevice device, Point dragOrigin, Point currentPosition ) {
			Device = device;
			DragOrigin = dragOrigin;
			CurrentPosition = currentPosition;
		}

		public InputDevice Device { get; private set; }
		public Point DragOrigin { get; private set; }
		public Point CurrentPosition { get; private set; }
	}
}
