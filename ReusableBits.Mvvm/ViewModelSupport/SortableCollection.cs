using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;

namespace ReusableBits.Mvvm.ViewModelSupport {
	public class SortableCollection<T> : BindableCollection<T> {
		public void Sort<TKey>( Func<T, TKey> keySelector, ListSortDirection direction ) {
			switch( direction ) {
				case ListSortDirection.Ascending: {
						ApplySort( Items.OrderBy( keySelector ));
						break;
					}
				case ListSortDirection.Descending: {
						ApplySort( Items.OrderByDescending( keySelector ) );
						break;
					}
			}
		}

		public void Sort<TKey>( Func<T, TKey> keySelector, IComparer<TKey> comparer ) {
			ApplySort( Items.OrderBy( keySelector, comparer ));
		}

		private void ApplySort( IEnumerable<T> sortedItems ) {
			var sortedItemsList = sortedItems.ToList();
			var previousNotificationSetting = IsNotifying;
            IsNotifying = false;

			

			foreach( var item in sortedItemsList ) {
				Move( IndexOf( item ), sortedItemsList.IndexOf( item ));
			}

			IsNotifying = previousNotificationSetting;
		}
	}
}
