using System;
using System.Windows.Forms;
using System.Windows.Interop;
using SlimDX.Multimedia;
using SlimDX.RawInput;

namespace Noise.TenFoot.Ui.Input {
	public class RawKeyboardHandler {
		public delegate		void RawInput( object sender, Keys key, bool isPressed, bool isSystemKey );
		public event		RawInput OnRawInput = delegate { };

		public void Initialize( IntPtr hwnd ) {
			Device.RegisterDevice( UsagePage.Generic, UsageId.Keyboard, DeviceFlags.None, hwnd, true );
			Device.KeyboardInput += OnKeyboard;

			AddThreadFilter();
		}

		private void OnKeyboard( object sender, KeyboardInputEventArgs args ) {
			OnRawInput( this, args.Key,
							  args.State == KeyState.Pressed || args.State == KeyState.SystemKeyReleased,
							  args.State == KeyState.SystemKeyPressed );
		}

		private void AddThreadFilter() {
			ComponentDispatcher.ThreadFilterMessage += (( ref MSG msg, ref bool handled ) => {
				if(!handled ) {
					if( msg.message == 0x00FF /* WM_INPUT */ ) {
						Device.HandleMessage( msg.lParam );

						handled = true;
					}
				}
			} );
		}
	}
}
