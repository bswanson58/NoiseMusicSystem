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
		private Action<InputEvent>			mInputHandler;
		private bool						mShiftPressed;
		private bool						mControlPressed;

		public InputProcessor( IEventAggregator eventAggregator, RawKeyboardHandler keyboardHandler ) {
			mEventAggregator = eventAggregator;
			mKeyboardhandler = keyboardHandler;
			mInputBindings = new List<InputBinding>();
			mInputHandler = input => { };
		}

		public bool Initialize( IntPtr hwnd, Action<InputEvent> inputHandler ) {
			if( inputHandler != null ) {
				mInputHandler = inputHandler;
			}

			return( Initialize( hwnd ));
		}

		public bool Initialize( IntPtr hwnd ) {
			mKeyboardhandler.Initialize( hwnd );
			mKeyboardhandler.OnRawInput += OnKeyboardInput;

			mInputBindings.Add( new InputBinding { Command = InputCommand.Up, Key = Keys.Up });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Down, Key = Keys.Down });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Left, Key = Keys.Left });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Right, Key = Keys.Right });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Back, Key = Keys.Back });

			mInputBindings.Add( new InputBinding { Command = InputCommand.Select, Key = Keys.Select });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Select, Key = Keys.Enter });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Enqueue, Key = Keys.Insert });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Enqueue, Key = Keys.PageUp });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Dequeue, Key = Keys.Delete });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Dequeue, Key = Keys.Next });

			mInputBindings.Add( new InputBinding { Command = InputCommand.Home, Key = Keys.Home });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Home, Key = Keys.T, Shift = true, Control = true });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Library, Key = Keys.L });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Library, Key = Keys.M, Control = true });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Queue, Key = Keys.Q });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Queue, Key = Keys.I, Control = true });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Favorites, Key = Keys.F });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Favorites, Key = Keys.E, Control = true });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Configuration, Key = Keys.C });

			mInputBindings.Add( new InputBinding { Command = InputCommand.Play, Key = Keys.MediaPlayPause });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Pause, Key = Keys.MediaPlayPause });
			mInputBindings.Add( new InputBinding { Command = InputCommand.Stop, Key=Keys.MediaStop });
			mInputBindings.Add( new InputBinding { Command = InputCommand.NextTrack, Key = Keys.MediaNextTrack });
			mInputBindings.Add( new InputBinding { Command = InputCommand.PreviousTrack, Key = Keys.MediaPreviousTrack });

			mInputBindings.Add( new InputBinding { Command = InputCommand.Mute, Key = Keys.VolumeMute });
			mInputBindings.Add( new InputBinding { Command = InputCommand.VolumeUp, Key = Keys.VolumeUp });
			mInputBindings.Add( new InputBinding { Command = InputCommand.VolumeDown, Key = Keys.VolumeDown });

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
					var input = new InputEvent( binding.Command );

					mEventAggregator.Publish( input );
					mInputHandler( input );
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
