using System;
using System.Windows.Forms;
using Caliburn.Micro;

namespace Noise.TenFoot.Ui.Input {
	public class InputProcessor {
		private readonly IEventAggregator	mEventAggregator;
		private readonly RawKeyboardHandler	mKeyboardhandler;

		public InputProcessor( IEventAggregator eventAggregator, RawKeyboardHandler keyboardHandler ) {
			mEventAggregator = eventAggregator;
			mKeyboardhandler = keyboardHandler;
		}

		public bool Initialize( IntPtr hwnd ) {
			mKeyboardhandler.Initialize( hwnd );
			mKeyboardhandler.OnRawInput += OnKeyboardInput;

			return( true );
		}

		private void OnKeyboardInput( object sender, Keys key, bool isPressed, bool isSystemKey ) {
			if( isPressed ) {
				switch( key ) {
					case Keys.Up:
						mEventAggregator.Publish( new InputEvent( InputCommand.Up ));
						break;

					case Keys.Down:
						mEventAggregator.Publish( new InputEvent( InputCommand.Down ));
						break;

					case Keys.Left:
						mEventAggregator.Publish( new InputEvent( InputCommand.Left ));
						break;

					case Keys.Right:
						mEventAggregator.Publish( new InputEvent( InputCommand.Right ));
						break;

					case Keys.Back:
						mEventAggregator.Publish( new InputEvent( InputCommand.Back ));
						break;

					case Keys.Return:
						mEventAggregator.Publish( new InputEvent( InputCommand.Select ));
						break;
				}
			}
		}
	}
}
