using Caliburn.Micro;
using Noise.TenFoot.Ui.Input;
using ReusableBits.Mvvm.CaliburnSupport;

namespace Noise.TenFoot.Ui.ViewModels {
	public abstract class BaseListViewModel : Screen, IHandle<InputEvent> {
		protected virtual void NextItem() { }
		protected virtual void PreviousItem() { }
		protected virtual void DisplayItem() { }

		protected virtual void Done() {
			if( Parent is INavigate ) {
				var controller = Parent as INavigate;

				controller.NavigateReturn( this, true );
			}
		}

		protected virtual void Home() {
			if( Parent is INavigate ) {
				var controller = Parent as INavigate;

				controller.NavigateHome();
			}
		}

		public void Handle( InputEvent message ) {
			switch( message.Command ) {
				case InputCommand.Up:
					PreviousItem();
					break;

				case InputCommand.Down:
					NextItem();
					break;

				case InputCommand.Left:
					break;

				case InputCommand.Right:
					break;

				case InputCommand.Back:
					Done();
					break;

				case InputCommand.Home:
					Home();
					break;

				case InputCommand.Select:
					DisplayItem();
					break;
			}
		}
	}
}
