using Noise.TenFoot.Ui.Input;

namespace Noise.TenFoot.Ui.Interfaces {
	public interface IHome : ITitledScreen {
		void	ProcessInput( InputEvent inputEvent );
	}
}
