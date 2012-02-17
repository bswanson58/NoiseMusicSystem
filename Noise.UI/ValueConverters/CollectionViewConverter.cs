using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace Noise.UI.ValueConverters {
	public class CollectionViewConverter : IMultiValueConverter {
		private readonly CollectionViewSource	mViewSource;

		public CollectionViewConverter() {
			mViewSource = new CollectionViewSource();
		}

		public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture ) {
			if( values.Length == 2 ) {
				if( values[0] is IEnumerable<SortDescription> ) {
					SetSortDescriptions( values[0] as IEnumerable<SortDescription>);
				}
				else {
					mViewSource.Source = values[0];
				}

				if( values[1] is IEnumerable<SortDescription> ) {
					SetSortDescriptions( values[1] as IEnumerable<SortDescription>);
				}
				else {
					mViewSource.Source = values[1];
				}
			}

			return( mViewSource.View );
		}

		private void SetSortDescriptions( IEnumerable<SortDescription> sortDescriptions ) {
			mViewSource.SortDescriptions.Clear();

			if( sortDescriptions != null ) {
				foreach( var sortDescription in sortDescriptions ) {
					mViewSource.SortDescriptions.Add( sortDescription );
				}
			}
		} 

		public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture ) {
			throw new NotImplementedException();
		}
	}
}
