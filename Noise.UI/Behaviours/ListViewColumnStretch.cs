using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Noise.UI.Behaviours {
	public class ListViewColumns : DependencyObject {
		/// <summary>
		/// IsStretched Dependancy property which can be attached to gridview columns.
		/// </summary>
		public static readonly DependencyProperty StretchProperty =
			DependencyProperty.RegisterAttached( "Stretch",
			typeof( bool ),
			typeof( ListViewColumns ),
			new UIPropertyMetadata( true, null, OnCoerceStretch ) );

		/// <summary>
		/// Gets the stretch.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public static bool GetStretch( DependencyObject obj ) {
			return (bool)obj.GetValue( StretchProperty );
		}

		/// <summary>
		/// Sets the stretch.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <param name="value">if set to <c>true</c> [value].</param>
		public static void SetStretch( DependencyObject obj, bool value ) {
			obj.SetValue( StretchProperty, value );
		}

		/// <summary>
		/// Called when [coerce stretch].
		/// </summary>
		/// <remarks>If this callback seems unfamilar then please read
		/// the great blog post by Paul jackson found here. 
		/// http://compilewith.net/2007/08/wpf-dependency-properties.html</remarks>
		/// <param name="source">The source.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static object OnCoerceStretch( DependencyObject source, object value ) {
			var lv = ( source as ListView );

			//Ensure we dont have an invalid dependancy object of type ListView.
			if( lv == null ) {
				throw new ArgumentException( "This property may only be used on ListViews" );
			}

			//Setup our event handlers for this list view.
			lv.Loaded += OnListViewLoaded;
			lv.SizeChanged += OnSizeChanged;
			return value;
		}

		/// <summary>
		/// Handles the SizeChanged event of the lv control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
		private static void OnSizeChanged( object sender, SizeChangedEventArgs e ) {
			var lv = ( sender as ListView );

			if(( lv != null ) &&
			   ( lv.IsLoaded )) {
				//Set our initial widths.
				SetColumnWidths( lv );
			}
		}

		/// <summary>
		/// Handles the Loaded event of the lv control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		private static void OnListViewLoaded( object sender, RoutedEventArgs e ) {
			var lv = ( sender as ListView );

			//Set our initial widths.
			if( lv != null ) {
				SetColumnWidths( lv );
			}
		}

		/// <summary>
		/// Sets the column widths.
		/// </summary>
		private static void SetColumnWidths( ListView listView ) {
			//Pull the stretch columns fromt the tag property.
			var columns = ( listView.Tag as List<GridViewColumn> );
			var gridView = listView.View as GridView;

			double specifiedWidth = 0;

			if( gridView != null ) {
				if( columns == null ) {
					//Instance if its our first run.
					columns = new List<GridViewColumn>();
					// Get all columns with no width having been set.
					foreach( var column in gridView.Columns ) {
						if( !( column.Width >= 0 )) {
							columns.Add( column );
						}
						else {
							specifiedWidth += column.ActualWidth;
						}
					}
				}
				else {
					// Get all columns with no width having been set.
					specifiedWidth += gridView.Columns.Where( column => !columns.Contains( column )).Sum( column => column.ActualWidth );
				}

				// Add in the width of a vertical scroll bar if one is needed.
				var scrollBarWidth = SystemParameters.VerticalScrollBarWidth;
//				var scrollBarWidth = 0.0;
//				var border = VisualTreeHelper.GetChild( listView, 0 ) as Decorator;
//				if( border != null ) {
//					var scrollViewer = border.Child as ScrollViewer;
//					if( scrollViewer != null ) {
//						scrollBarWidth = scrollViewer.ScrollableHeight > 0 ? SystemParameters.VerticalScrollBarWidth : 0;
//					}
//				}

				// Allocate remaining space equally.
				foreach( GridViewColumn column in columns ) {
					double newWidth = ( listView.ActualWidth - scrollBarWidth - specifiedWidth ) / columns.Count - 10;
					if( newWidth >= 0 ) {
						column.Width = newWidth;
					}
				}

				//Store the columns in the TAG property for later use. 
				listView.Tag = columns;
			}
		}
	}
}
