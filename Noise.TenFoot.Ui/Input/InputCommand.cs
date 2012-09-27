using System.Windows.Forms;

namespace Noise.TenFoot.Ui.Input {
	public enum InputCommand {
		Up,
		Down,
		Left,
		Right,
		Back,
		Home,
		Select,
		Play
	}

	public class InputBinding {
		public InputCommand		Command { get; set; }
		public Keys				Key { get; set; }
		public bool				Shift { get; set; }
		public bool				Control { get; set; }
	}

	public class InputEvent {
		public InputCommand	Command { get; private set; }

		public InputEvent( InputCommand command ) {
			Command = command;
		}
	}
}
