namespace Noise.TenFoot.Ui.Input {
	public enum InputCommand {
		Up,
		Down,
		Left,
		Right
	}

	public class InputEvent {
		public InputCommand	Command { get; private set; }

		public InputEvent( InputCommand command ) {
			Command = command;
		}
	}
}
