using Caliburn.Micro;
using Noise.TenFoot.Ui.Input;

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
		protected virtual void EnqueueItem() { }
		protected virtual void DequeueItem() { }

		protected IEventAggregator EventAggregator {
			get{ return( mEventAggregator ); }
		}

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

		protected void SetSelectedItem( int index ) {
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

		protected virtual void Left() { }
		protected virtual void Right() { }

		protected virtual void Done() {
			EventAggregator.PublishOnUIThread( new Events.NavigateReturn( this, true ));
		}

		public virtual void Handle( InputEvent message ) {
			switch( message.Command ) {
				case InputCommand.Up:
					PreviousItem();
					break;

				case InputCommand.Down:
					NextItem();
					break;

				case InputCommand.Left:
					Left();
					break;

				case InputCommand.Right:
					Right();
					break;

				case InputCommand.Back:
					Done();
					break;

				case InputCommand.Select:
					if( SelectedItem != null ) {
						DisplayItem();
					}
					break;

				case InputCommand.Enqueue:
					if( SelectedItem != null ) {
						EnqueueItem();
					}
					break;

				case InputCommand.Dequeue:
					if( SelectedItem != null ) {
						DequeueItem();
					}
					break;
			}
		}
	}
}
