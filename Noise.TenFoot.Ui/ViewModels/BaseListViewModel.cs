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

		private void SetSelectedItem( int index ) {
			var itemCount = mItemList.Count;

			if( itemCount > 0 ) {
				if( index < 0 ) {
					index = itemCount + index;
				}

				if( index >= itemCount ) {
					index = index % itemCount;
				}

				if( index < itemCount ) {
					SelectedItem = ItemList[index];
				}
			}
		}

		protected virtual void NextItem() {
			SetSelectedItem((int)SelectedItemIndex + 1 );
		}

		protected virtual void PreviousItem() {
			SetSelectedItem((int)SelectedItemIndex - 1 );
		}

		protected virtual void Done() {
			if( Parent is INavigate ) {
				var controller = Parent as INavigate;

				controller.NavigateReturn( this, true );
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
