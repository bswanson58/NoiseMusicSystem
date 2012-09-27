using Caliburn.Micro;
using Noise.TenFoot.Ui.Input;
using ReusableBits.Mvvm.CaliburnSupport;

namespace Noise.TenFoot.Ui.ViewModels {
	public abstract class BaseListViewModel<TItem> : Screen, IHandle<InputEvent> where TItem : class {
		private	readonly IEventAggregator			mEventAggregator;
		private	readonly BindableCollection<TItem>	mItemList;
		private	TItem								mSelectedItem;
		private	double								mSelectedItemIndex;

		protected BaseListViewModel( IEventAggregator eventAggregator ) {
			mEventAggregator = eventAggregator;

			mItemList = new BindableCollection<TItem>();
		}

		protected virtual void NextItem() { }
		protected virtual void PreviousItem() { }
		protected virtual void DisplayItem() { }
		protected virtual void PlayItem() { }

		protected override void OnActivate() {
			base.OnActivate();

			mEventAggregator.Subscribe( this );
		}

		protected override void OnDeactivate( bool close ) {
			base.OnDeactivate( close );

			mEventAggregator.Unsubscribe( this );
		}

		public BindableCollection<TItem> ItemList {
			get{ return( mItemList ); }
		}
 
		public TItem SelectedItem {
			get{ return( mSelectedItem ); }
			set {
				mSelectedItem = value;

				SelectedItemIndex = mSelectedItem != null ? mItemList.IndexOf( mSelectedItem ) : 0.0d;

				NotifyOfPropertyChange( () => SelectedItem );
			}
		}

		public double SelectedItemIndex {
			get{ return( mSelectedItemIndex ); }
			set {
				mSelectedItemIndex = value;

				NotifyOfPropertyChange( () => SelectedItemIndex );
			}
		}

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
					if( SelectedItem != null ) {
						DisplayItem();
					}
					break;

				case InputCommand.Play:
					if( SelectedItem != null ) {
						PlayItem();
					}
					break;
			}
		}
	}
}
