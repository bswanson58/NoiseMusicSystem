using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace Noise.UI.Behaviours {
	public class BindableCollectionViewSource : CollectionViewSource {
		private ObservableCollection<SortDescription>	mCurrentSorts;
 
		// The bindable collection of sort descriptions
		public ObservableCollection<SortDescription> BindableSortDescriptions {
			get{ return( (ObservableCollection<SortDescription>)GetValue( BindableSortDescriptionsProperty )); }
			set{ SetValue( BindableSortDescriptionsProperty, value ); }
		}

		public static readonly DependencyProperty BindableSortDescriptionsProperty =
			DependencyProperty.Register( "BindableSortDescriptions", typeof( ObservableCollection<SortDescription> ),
																	 typeof( BindableCollectionViewSource ),
																	 new UIPropertyMetadata( null, OnSortDescriptionsPropertyChanged ));

		private static void OnSortDescriptionsPropertyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args ) {
			if( sender is BindableCollectionViewSource ) {
				(sender as BindableCollectionViewSource).UpdateSortsCollection();
			}
		}

		public void UpdateSortsCollection() {
			if( mCurrentSorts != null ) {
				mCurrentSorts.CollectionChanged -= OnSortsChanged;
			}

			mCurrentSorts = BindableSortDescriptions;

			if( mCurrentSorts != null ) {
				mCurrentSorts.CollectionChanged += OnSortsChanged;

				UpdateSorts();
			}
		}

		private void OnSortsChanged( object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs ) {
			UpdateSorts();
		}

		private void UpdateSorts() {
			SortDescriptions.Clear();
			
			if( mCurrentSorts != null ) {
				foreach( var sortDescrition in mCurrentSorts ) {
					SortDescriptions.Add( sortDescrition );
				}
			}
		}
	}
}
