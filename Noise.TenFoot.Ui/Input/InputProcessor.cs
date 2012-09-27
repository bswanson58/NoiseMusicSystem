using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Caliburn.Micro;

namespace Noise.TenFoot.Ui.Input {
	public class InputProcessor {
		private readonly IEventAggregator	mEventAggregator;
		private readonly RawKeyboardHandler	mKeyboardhandler;
		private readonly List<InputBinding>	mInputBindings;
		private bool						mShiftPressed;
		private bool						mControlPressed;

		public InputProcessor( IEventAggregator eventAggregator, RawKeyboardHandler keyboardHandler ) {
			mEventAggregator = eventAggregator;
			mKeyboardhandler = keyboardHandler;
			mInputBindings = new List<InputBinding>();
		}

		public bool Initialize( IntPtr hwnd ) {
			mKeyboardhandler.Initialize( hwnd );
			mKeyboardhandler.OnRawInput += OnKeyboardInput;

			mInputBindings.Add( new InputBinding { Command = InputCommand.Up, Key = Keys.Up });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Down, Key = Keys.Down });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Left, Key = Keys.Left });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Right, Key = Keys.Right });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Select, Key = Keys.Select });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Select, Key = Keys.Enter });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Back, Key = Keys.Back });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Home, Key = Keys.Home });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Play, Key = Keys.P });

			return( true );
		}

		private void OnKeyboardInput( object sender, Keys key, bool isPressed, bool isSystemKey ) {
			if( isPressed ) {
				switch( key ) {
					case Keys.ShiftKey:
						mShiftPressed = true;
						break;

					case Keys.ControlKey:
						mControlPressed = true;
						break;
				}

				var binding = mInputBindings.FirstOrDefault( b => b.Key == key && 
																  b.Shift == mShiftPressed && 
																  b.Control == mControlPressed );
				if( binding != null ) {
					mEventAggregator.Publish( new InputEvent( binding.Command ));
				}
			}
			else {
				switch( key ) {
					case Keys.ShiftKey:
						mShiftPressed = false;
						break;

					case Keys.ControlKey:
						mControlPressed = false;
						break;
				}
			}
		}
	}
}
