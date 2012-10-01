using Noise.TenFoot.Ui.Dto;

namespace Noise.TenFoot.Ui.Interfaces {
	public interface IHomeScreen : ITitledScreen {
		eMainMenuCommand	MenuCommand { get; }
		int					ScreenOrder { get; }
	}
}
