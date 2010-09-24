using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Noise.Infrastructure.Support {
	public class ObservableCollectionEx<T> : ObservableCollection<T> {
		private bool mSuppressNotification;
		private bool mNotificationPending;

		protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs e ) {
			if( !mSuppressNotification ) {
				base.OnCollectionChanged( e );
			}

			mNotificationPending = true;
		}

		public void SuspendNotification() {
			mSuppressNotification = true;
		}

		public void ResumeNotification() {
			mSuppressNotification = false;

			if( mNotificationPending ) {
				OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
				mNotificationPending = false;
			}
		}

		public void AddRange( IEnumerable<T> list ) {
			if( list == null ) {
				throw new ArgumentNullException( "list" );
			}

			SuspendNotification();

			foreach( T item in list ) {
				Add( item );
			}

			ResumeNotification();
		}

		public void Sort<TKey>( Func<T, TKey> keySelector, System.ComponentModel.ListSortDirection direction ) {
			switch( direction ) {
				case System.ComponentModel.ListSortDirection.Ascending: {
						ApplySort( Items.OrderBy( keySelector ) );
						break;
					}
				case System.ComponentModel.ListSortDirection.Descending: {
						ApplySort( Items.OrderByDescending( keySelector ) );
						break;
					}
			}
		}

		public void Sort<TKey>( Func<T, TKey> keySelector, IComparer<TKey> comparer ) {
			ApplySort( Items.OrderBy( keySelector, comparer ) );
		}

		private void ApplySort( IEnumerable<T> sortedItems ) {
			var sortedItemsList = sortedItems.ToList();

			SuspendNotification();

			foreach( var item in sortedItemsList ) {
				Move( IndexOf( item ), sortedItemsList.IndexOf( item ) );
			}

			ResumeNotification();
		}
	}
}
