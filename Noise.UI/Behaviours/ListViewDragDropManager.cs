// Copyright (C) Josh Smith - January 2007
// from: http://www.codeproject.com/kb/WPF/ListViewDragDropManager.aspx
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Input;
using Noise.UI.Support;

namespace Noise.UI.Behaviours {
	/// <summary>
	/// Manages the dragging and dropping of ListViewItems in a ListView.
	/// The ItemType type parameter indicates the type of the objects in
	/// the ListView's items source.  The ListView's ItemsSource must be 
	/// set to an instance of ObservableCollection of ItemType, or an 
	/// Exception will be thrown.
	/// </summary>
	/// <typeparam name="TItemType">The type of the ListView's items.</typeparam>
	public class ListViewDragDropManager<TItemType> where TItemType : class {
		private	bool		mCanInitiateDrag;
		private	DragAdorner mDragAdorner;
		private	double		mDragAdornerOpacity;
		private	int			mIndexToSelect;
		private	TItemType	mItemUnderDragCursor;
		private	ListView	mListView;
		private	Point		mPtMouseDown;
		private	bool		mShowDragAdorner;

		/// <summary>
		/// Returns true if there is currently a drag operation being managed.
		/// </summary>
		public	bool		IsDragInProgress { get; private set; }

		/// <summary>
		/// Initializes a new instance of ListViewDragManager.
		/// </summary>
		public ListViewDragDropManager() {
			mCanInitiateDrag = false;
			mDragAdornerOpacity = 0.7;
			mIndexToSelect = -1;
			mShowDragAdorner = true;
		}

		/// <summary>
		/// Initializes a new instance of ListViewDragManager.
		/// </summary>
		/// <param name="listView"></param>
		public ListViewDragDropManager( ListView listView )
			: this() {
			ListView = listView;
		}

		/// <summary>
		/// Initializes a new instance of ListViewDragManager.
		/// </summary>
		/// <param name="listView"></param>
		/// <param name="dragAdornerOpacity"></param>
		public ListViewDragDropManager( ListView listView, double dragAdornerOpacity )
			: this( listView ) {
			DragAdornerOpacity = dragAdornerOpacity;
		}

		/// <summary>
		/// Initializes a new instance of ListViewDragManager.
		/// </summary>
		/// <param name="listView"></param>
		/// <param name="showDragAdorner"></param>
		public ListViewDragDropManager( ListView listView, bool showDragAdorner )
			: this( listView ) {
			ShowDragAdorner = showDragAdorner;
		}

		/// <summary>
		/// Gets/sets the opacity of the drag adorner.  This property has no
		/// effect if ShowDragAdorner is false. The default value is 0.7
		/// </summary>
		public double DragAdornerOpacity {
			get { return mDragAdornerOpacity; }
			set {
				if( IsDragInProgress )
					throw new InvalidOperationException( "Cannot set the DragAdornerOpacity property during a drag operation." );

				if( value < 0.0 || value > 1.0 )
					throw new ArgumentOutOfRangeException( "DragAdornerOpacity", value, "Must be between 0 and 1." );

				mDragAdornerOpacity = value;
			}
		}

		/// <summary>
		/// Gets/sets the ListView whose dragging is managed.  This property
		/// can be set to null, to prevent drag management from occuring.  If
		/// the ListView's AllowDrop property is false, it will be set to true.
		/// </summary>
		public ListView ListView {
			get { return mListView; }
			set {
				if( IsDragInProgress )
					throw new InvalidOperationException( "Cannot set the ListView property during a drag operation." );

				if( mListView != null ) {
					mListView.PreviewMouseLeftButtonDown -= OnListViewPreviewMouseLeftButtonDown;
					mListView.PreviewMouseMove -= OnListViewPreviewMouseMove;
					mListView.DragOver -= OnListViewDragOver;
					mListView.DragLeave -= OnListViewDragLeave;
					mListView.DragEnter -= OnListViewDragEnter;
					mListView.Drop -= OnListViewDrop;
				}

				mListView = value;

				if( mListView != null ) {
					if( !mListView.AllowDrop )
						mListView.AllowDrop = true;
					mListView.PreviewMouseLeftButtonDown += OnListViewPreviewMouseLeftButtonDown;
					mListView.PreviewMouseMove += OnListViewPreviewMouseMove;
					mListView.DragOver += OnListViewDragOver;
					mListView.DragLeave += OnListViewDragLeave;
					mListView.DragEnter += OnListViewDragEnter;
					mListView.Drop += OnListViewDrop;
				}
			}
		}

		/// <summary>
		/// Raised when a drop occurs.  By default the dropped item will be moved
		/// to the target index.  Handle this event if relocating the dropped item
		/// requires custom behavior.  Note, if this event is handled the default
		/// item dropping logic will not occur.
		/// </summary>
		public event EventHandler<ProcessDropEventArgs<TItemType>> ProcessDrop;

		/// <summary>
		/// Gets/sets whether a visual representation of the ListViewItem being dragged
		/// follows the mouse cursor during a drag operation.  The default value is true.
		/// </summary>
		public bool ShowDragAdorner {
			get { return mShowDragAdorner; }
			set {
				if( IsDragInProgress )
					throw new InvalidOperationException( "Cannot set the ShowDragAdorner property during a drag operation." );

				mShowDragAdorner = value;
			}
		}

		private void OnListViewPreviewMouseLeftButtonDown( object sender, MouseButtonEventArgs e ) {
			if( IsMouseOverScrollbar ) {
				// 4/13/2007 - Set the flag to false when cursor is over scrollbar.
				mCanInitiateDrag = false;
				return;
			}

			int index = IndexUnderDragCursor;
			mCanInitiateDrag = index > -1;

			if( mCanInitiateDrag ) {
				// Remember the location and index of the ListViewItem the user clicked on for later.
				mPtMouseDown = MouseUtilities.GetMousePosition( mListView );
				mIndexToSelect = index;
			}
			else {
				mPtMouseDown = new Point( -10000, -10000 );
				mIndexToSelect = -1;
			}
		}

		private void OnListViewPreviewMouseMove( object sender, MouseEventArgs e ) {
			if( !CanStartDragOperation )
				return;

			// Select the item the user clicked on.
			mListView.SelectedIndex = mIndexToSelect;

			// If the item at the selected index is null, there's nothing
			// we can do, so just return;
			if( mListView.SelectedItem == null ) {
				return;
			}

			ListViewItem itemToDrag = GetListViewItem( mListView.SelectedIndex );
			if( itemToDrag == null ) {
				return;
			}

			AdornerLayer adornerLayer = ShowDragAdornerResolved ? InitializeAdornerLayer( itemToDrag ) : null;

			InitializeDragOperation( itemToDrag );
			PerformDragOperation();
			FinishDragOperation( itemToDrag, adornerLayer );
		}

		private void OnListViewDragOver( object sender, DragEventArgs e ) {
			e.Effects = DragDropEffects.Move;

			if( ShowDragAdornerResolved )
				UpdateDragAdornerLocation();

			// Update the item which is known to be currently under the drag cursor.
			int index = IndexUnderDragCursor;
			ItemUnderDragCursor = index < 0 ? null : ListView.Items[index] as TItemType;
		}

		private void OnListViewDragLeave( object sender, DragEventArgs e ) {
			if( !IsMouseOver( mListView ) ) {
				ItemUnderDragCursor = null;

				if( mDragAdorner != null )
					mDragAdorner.Visibility = Visibility.Collapsed;
			}
		}

		private void OnListViewDragEnter( object sender, DragEventArgs e ) {
			if( mDragAdorner != null && mDragAdorner.Visibility != Visibility.Visible ) {
				// Update the location of the adorner and then show it.				
				UpdateDragAdornerLocation();
				mDragAdorner.Visibility = Visibility.Visible;
			}
		}

		private void OnListViewDrop( object sender, DragEventArgs e ) {
			ItemUnderDragCursor = null;

			e.Effects = DragDropEffects.None;

			if( !e.Data.GetDataPresent( typeof( TItemType ) ) )
				return;

			// Get the data object which was dropped.
			var data = e.Data.GetData( typeof( TItemType ) ) as TItemType;
			if( data == null )
				return;

			// Get the ObservableCollection<ItemType> which contains the dropped data object.
			var itemsSource = mListView.ItemsSource as ObservableCollection<TItemType>;
			if( itemsSource == null )
				throw new Exception(
					"A ListView managed by ListViewDragManager must have its ItemsSource set to an ObservableCollection<ItemType>." );

			int oldIndex = itemsSource.IndexOf( data );
			int newIndex = IndexUnderDragCursor;

			if( newIndex < 0 ) {
				// The drag started somewhere else, and our ListView is empty
				// so make the new item the first in the list.
				if( itemsSource.Count == 0 )
					newIndex = 0;

				// The drag started somewhere else, but our ListView has items
				// so make the new item the last in the list.
				else if( oldIndex < 0 )
					newIndex = itemsSource.Count;

				// The user is trying to drop an item from our ListView into
				// our ListView, but the mouse is not over an item, so don't
				// let them drop it.
				else
					return;
			}

			// Dropping an item back onto itself is not considered an actual 'drop'.
			if( oldIndex == newIndex )
				return;

			if( ProcessDrop != null ) {
				// Let the client code process the drop.
				var args = new ProcessDropEventArgs<TItemType>( itemsSource, data, oldIndex, newIndex, e.AllowedEffects );
				ProcessDrop( this, args );
				e.Effects = args.Effects;
			}
			else {
				// Move the dragged data object from it's original index to the
				// new index (according to where the mouse cursor is).  If it was
				// not previously in the ListBox, then insert the item.
				if( oldIndex > -1 )
					itemsSource.Move( oldIndex, newIndex );
				else
					itemsSource.Insert( newIndex, data );

				// Set the Effects property so that the call to DoDragDrop will return 'Move'.
				e.Effects = DragDropEffects.Move;
			}
		}

		private bool CanStartDragOperation {
			get {
				if( Mouse.LeftButton != MouseButtonState.Pressed )
					return false;

				if( !mCanInitiateDrag )
					return false;

				if( mIndexToSelect == -1 )
					return false;

				if( !HasCursorLeftDragThreshold )
					return false;

				return true;
			}
		}

		private void FinishDragOperation( ListViewItem draggedItem, AdornerLayer adornerLayer ) {
			// Let the ListViewItem know that it is not being dragged anymore.
			ListViewItemDragState.SetIsBeingDragged( draggedItem, false );

			IsDragInProgress = false;

			ItemUnderDragCursor = null;

			// Remove the drag adorner from the adorner layer.
			if( adornerLayer != null ) {
				adornerLayer.Remove( mDragAdorner );
				mDragAdorner = null;
			}
		}

		private ListViewItem GetListViewItem( int index ) {
			if( mListView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated )
				return null;

			return mListView.ItemContainerGenerator.ContainerFromIndex( index ) as ListViewItem;
		}

		private ListViewItem GetListViewItem( TItemType dataItem ) {
			if( mListView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated )
				return null;

			return mListView.ItemContainerGenerator.ContainerFromItem( dataItem ) as ListViewItem;
		}

		private bool HasCursorLeftDragThreshold {
			get {
				if( mIndexToSelect < 0 )
					return false;

				ListViewItem item = GetListViewItem( mIndexToSelect );
				Rect bounds = VisualTreeHelper.GetDescendantBounds( item );
				Point ptInItem = mListView.TranslatePoint( mPtMouseDown, item );

				// In case the cursor is at the very top or bottom of the ListViewItem
				// we want to make the vertical threshold very small so that dragging
				// over an adjacent item does not select it.
				double topOffset = Math.Abs( ptInItem.Y );
				double btmOffset = Math.Abs( bounds.Height - ptInItem.Y );
				double vertOffset = Math.Min( topOffset, btmOffset );

				double width = SystemParameters.MinimumHorizontalDragDistance * 2;
				double height = Math.Min( SystemParameters.MinimumVerticalDragDistance, vertOffset ) * 2;
				var szThreshold = new Size( width, height );

				var rect = new Rect( mPtMouseDown, szThreshold );
				rect.Offset( szThreshold.Width / -2, szThreshold.Height / -2 );
				Point ptInListView = MouseUtilities.GetMousePosition( mListView );
				return !rect.Contains( ptInListView );
			}
		}

		/// <summary>
		/// Returns the index of the ListViewItem underneath the
		/// drag cursor, or -1 if the cursor is not over an item.
		/// </summary>
		private int IndexUnderDragCursor {
			get {
				int index = -1;
				for( int i = 0; i < mListView.Items.Count; ++i ) {
					ListViewItem item = GetListViewItem( i );
					if( IsMouseOver( item ) ) {
						index = i;
						break;
					}
				}
				return index;
			}
		}

		private AdornerLayer InitializeAdornerLayer( ListViewItem itemToDrag ) {
			// Create a brush which will paint the ListViewItem onto
			// a visual in the adorner layer.
			var brush = new VisualBrush( itemToDrag );

			// Create an element which displays the source item while it is dragged.
			mDragAdorner = new DragAdorner( mListView, itemToDrag.RenderSize, brush );

			// Set the drag adorner's opacity.		
			mDragAdorner.Opacity = DragAdornerOpacity;

			AdornerLayer layer = AdornerLayer.GetAdornerLayer( mListView );
			layer.Add( mDragAdorner );

			// Save the location of the cursor when the left mouse button was pressed.
//			mPtMouseDown = MouseUtilities.GetMousePosition( mListView );

			return layer;
		}

		private void InitializeDragOperation( ListViewItem itemToDrag ) {
			// Set some flags used during the drag operation.
			IsDragInProgress = true;
			mCanInitiateDrag = false;

			// Let the ListViewItem know that it is being dragged.
			ListViewItemDragState.SetIsBeingDragged( itemToDrag, true );
		}

		private static bool IsMouseOver( Visual target ) {
			// We need to use MouseUtilities to figure out the cursor
			// coordinates because, during a drag-drop operation, the WPF
			// mechanisms for getting the coordinates behave strangely.

			Rect bounds = VisualTreeHelper.GetDescendantBounds( target );
			Point mousePos = MouseUtilities.GetMousePosition( target );
			return bounds.Contains( mousePos );
		}

		/// <summary>
		/// Returns true if the mouse cursor is over a scrollbar in the ListView.
		/// </summary>
		private bool IsMouseOverScrollbar {
			get {
				Point ptMouse = MouseUtilities.GetMousePosition( mListView );
				HitTestResult res = VisualTreeHelper.HitTest( mListView, ptMouse );
				if( res == null )
					return false;

				DependencyObject depObj = res.VisualHit;
				while( depObj != null ) {
					if( depObj is ScrollBar )
						return true;

					// VisualTreeHelper works with objects of type Visual or Visual3D.
					// If the current object is not derived from Visual or Visual3D,
					// then use the LogicalTreeHelper to find the parent element.
					if( depObj is Visual || depObj is System.Windows.Media.Media3D.Visual3D )
						depObj = VisualTreeHelper.GetParent( depObj );
					else
						depObj = LogicalTreeHelper.GetParent( depObj );
				}

				return false;
			}
		}

		private TItemType ItemUnderDragCursor {
			set {
				if( mItemUnderDragCursor == value )
					return;

				// The first pass handles the previous item under the cursor.
				// The second pass handles the new one.
				for( int i = 0; i < 2; ++i ) {
					if( i == 1 )
						mItemUnderDragCursor = value;

					if( mItemUnderDragCursor != null ) {
						ListViewItem listViewItem = GetListViewItem( mItemUnderDragCursor );
						if( listViewItem != null )
							ListViewItemDragState.SetIsUnderDragCursor( listViewItem, i == 1 );
					}
				}
			}
		}

		private void PerformDragOperation() {
			var selectedItem = mListView.SelectedItem as TItemType;

			if( selectedItem != null ) {
				const DragDropEffects allowedEffects = DragDropEffects.Move | DragDropEffects.Move | DragDropEffects.Link;

				if( DragDrop.DoDragDrop( mListView, selectedItem, allowedEffects ) != DragDropEffects.None ) {
					// The item was dropped into a new location,
					// so make it the new selected item.
					mListView.SelectedItem = selectedItem;
				}
			}
		}

		private bool ShowDragAdornerResolved {
			get { return ShowDragAdorner && DragAdornerOpacity > 0.0; }
		}

		private void UpdateDragAdornerLocation() {
			if( mDragAdorner != null ) {
				Point ptCursor = MouseUtilities.GetMousePosition( ListView );

				double left = ptCursor.X - mPtMouseDown.X;

				// 4/13/2007 - Made the top offset relative to the item being dragged.
				ListViewItem itemBeingDragged = GetListViewItem( mIndexToSelect );
				Point itemLoc = itemBeingDragged.TranslatePoint( new Point( 0, 0 ), ListView );
				double top = itemLoc.Y + ptCursor.Y - mPtMouseDown.Y;

				mDragAdorner.SetOffsets( left, top );
			}
		}
	}

	/// <summary>
	/// Exposes attached properties used in conjunction with the ListViewDragDropManager class.
	/// Those properties can be used to allow triggers to modify the appearance of ListViewItems
	/// in a ListView during a drag-drop operation.
	/// </summary>
	public static class ListViewItemDragState {
		/// <summary>
		/// Identifies the ListViewItemDragState's IsBeingDragged attached property.  
		/// This field is read-only.
		/// </summary>
		public static readonly DependencyProperty IsBeingDraggedProperty =
			DependencyProperty.RegisterAttached(
				"IsBeingDragged",
				typeof( bool ),
				typeof( ListViewItemDragState ),
				new UIPropertyMetadata( false ) );

		/// <summary>
		/// Returns true if the specified ListViewItem is being dragged, else false.
		/// </summary>
		/// <param name="item">The ListViewItem to check.</param>
		public static bool GetIsBeingDragged( ListViewItem item ) {
			return (bool)item.GetValue( IsBeingDraggedProperty );
		}

		/// <summary>
		/// Sets the IsBeingDragged attached property for the specified ListViewItem.
		/// </summary>
		/// <param name="item">The ListViewItem to set the property on.</param>
		/// <param name="value">Pass true if the element is being dragged, else false.</param>
		internal static void SetIsBeingDragged( ListViewItem item, bool value ) {
			item.SetValue( IsBeingDraggedProperty, value );
		}

		/// <summary>
		/// Identifies the ListViewItemDragState's IsUnderDragCursor attached property.  
		/// This field is read-only.
		/// </summary>
		public static readonly DependencyProperty IsUnderDragCursorProperty =
			DependencyProperty.RegisterAttached(
				"IsUnderDragCursor",
				typeof( bool ),
				typeof( ListViewItemDragState ),
				new UIPropertyMetadata( false ) );

		/// <summary>
		/// Returns true if the specified ListViewItem is currently underneath the cursor 
		/// during a drag-drop operation, else false.
		/// </summary>
		/// <param name="item">The ListViewItem to check.</param>
		public static bool GetIsUnderDragCursor( ListViewItem item ) {
			return (bool)item.GetValue( IsUnderDragCursorProperty );
		}

		/// <summary>
		/// Sets the IsUnderDragCursor attached property for the specified ListViewItem.
		/// </summary>
		/// <param name="item">The ListViewItem to set the property on.</param>
		/// <param name="value">Pass true if the element is underneath the drag cursor, else false.</param>
		internal static void SetIsUnderDragCursor( ListViewItem item, bool value ) {
			item.SetValue( IsUnderDragCursorProperty, value );
		}
	}

	/// <summary>
	/// Event arguments used by the ListViewDragDropManager.ProcessDrop event.
	/// </summary>
	/// <typeparam name="TItemType">The type of data object being dropped.</typeparam>
	public class ProcessDropEventArgs<TItemType> : EventArgs where TItemType : class {
		private	readonly ObservableCollection<TItemType> mItemsSource;
		private	readonly TItemType			mDataItem;
		private	readonly int				mOldIndex;
		private	readonly int				mNewIndex;
		private	readonly DragDropEffects	mAllowedEffects = DragDropEffects.None;
		private	DragDropEffects				mEffects = DragDropEffects.None;

		internal ProcessDropEventArgs( ObservableCollection<TItemType> itemsSource, TItemType dataItem, int oldIndex, int newIndex, DragDropEffects allowedEffects ) {
			mItemsSource = itemsSource;
			mDataItem = dataItem;
			mOldIndex = oldIndex;
			mNewIndex = newIndex;
			mAllowedEffects = allowedEffects;
		}

		/// <summary>
		/// The items source of the ListView where the drop occurred.
		/// </summary>
		public ObservableCollection<TItemType> ItemsSource {
			get { return mItemsSource; }
		}

		/// <summary>
		/// The data object which was dropped.
		/// </summary>
		public TItemType DataItem {
			get { return mDataItem; }
		}

		/// <summary>
		/// The current index of the data item being dropped, in the ItemsSource collection.
		/// </summary>
		public int OldIndex {
			get { return mOldIndex; }
		}

		/// <summary>
		/// The target index of the data item being dropped, in the ItemsSource collection.
		/// </summary>
		public int NewIndex {
			get { return mNewIndex; }
		}

		/// <summary>
		/// The drag drop effects allowed to be performed.
		/// </summary>
		public DragDropEffects AllowedEffects {
			get { return mAllowedEffects; }
		}

		/// <summary>
		/// The drag drop effect(s) performed on the dropped item.
		/// </summary>
		public DragDropEffects Effects {
			get { return mEffects; }
			set { mEffects = value; }
		}
	}
}