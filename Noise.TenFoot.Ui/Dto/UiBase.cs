using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.TenFoot.Ui.Dto {
	public abstract class UiBase : PropertyChangeBase {
		public	long	DbId { get; set; }
		public	bool	IsFavorite { get; set; }
	}
}
