using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

// from: http://archive.msdn.microsoft.com/getwpfcode/Release/ProjectReleases.aspx?ReleaseId=1445

namespace Noise.UI.Behaviours {
	/// <summary>
	///     Provides data that should be useful to templates displaying a preview.
	/// </summary>
	public class ScrollingPreviewData : INotifyPropertyChanged {
		private double mOffset;
		private double mViewport;
		private double mExtent;
		private object mFirstItem;
		private object mLastItem;

		/// <summary>
		///     The ScrollBar's offset.
		/// </summary>
		public double Offset {
			get {
				return mOffset;
			}
			internal set {
				mOffset = value;
				OnPropertyChanged( "Offset" );
			}
		}

		/// <summary>
		///     The size of the current viewport.
		/// </summary>
		public double Viewport {
			get {
				return mViewport;
			}
			internal set {
				mViewport = value;
				OnPropertyChanged( "Viewport" );
			}
		}

		/// <summary>
		///     The entire scrollable range.
		/// </summary>
		public double Extent {
			get {
				return mExtent;
			}
			internal set {
				mExtent = value;
				OnPropertyChanged( "Extent" );
			}
		}

		/// <summary>
		///     The first visible item in the viewport.
		/// </summary>
		public object FirstItem {
			get {
				return mFirstItem;
			}
			private set {
				mFirstItem = value;
				OnPropertyChanged( "FirstItem" );
			}
		}

		/// <summary>
		///     The last visible item in the viewport.
		/// </summary>
		public object LastItem {
			get {
				return mLastItem;
			}
			private set {
				mLastItem = value;
				OnPropertyChanged( "LastItem" );
			}
		}

		/// <summary>
		///     Updates Offset, Viewport, and Extent.
		/// </summary>
		internal void UpdateScrollingValues( ScrollBar scrollBar ) {
			Offset = scrollBar.Value;
			Viewport = scrollBar.ViewportSize;
			Extent = scrollBar.Maximum - scrollBar.Minimum;
		}

		/// <summary>
		///     Updates FirstItem and LastItem based on the
		///     Offset and Viewport properties.
		/// </summary>
		internal void UpdateItem( ItemsControl itemsControl, bool vertical ) {
			if( itemsControl != null ) {
				int numItems = itemsControl.Items.Count;
				if( numItems > 0 ) {
					// This section does not appear to work with a virtualized tree view - WCS - 02-24-11
/*					if( VirtualizingStackPanel.GetIsVirtualizing( itemsControl )) {
						// Items scrolling (value == index)
						var	firstIndex = (int)mOffset;
						var	lastIndex = (int)mOffset + (int)mViewport - 1;

						if( ( firstIndex >= 0 ) && ( firstIndex < numItems ) ) {
							FirstItem = itemsControl.Items[firstIndex];
						}
						else {
							FirstItem = null;
						}

						if( ( lastIndex >= 0 ) && ( lastIndex < numItems ) ) {
							LastItem = itemsControl.Items[lastIndex];
						}
						else {
							LastItem = null;
						}
					}
					else { */
						// Pixel scrolling (no virtualization)
						// This will do a linear search through all of the items.
						// It will assume that the first item encountered that is within view is
						// the first visible item and the last item encountered that is
						// within view is the last visible item.
						// Improvements could be made to this algorithm depending on the
						// number of items in the collection and the their order relative
						// to each other on-screen.
						bool	foundFirstItem = false;
						int		bestLastItemIndex = -1;
						object	firstVisibleItem = null;
						object	lastVisibleItem = null;

						for( int i = 0; i < numItems; i++ ) {
							var	child = itemsControl.ItemContainerGenerator.ContainerFromIndex( i ) as UIElement;

							if( child != null ) {
								var scp = FindParent<ScrollContentPresenter>( child );

								if( scp == null ) {
									// Not in a ScrollViewer that we understand
									return;
								}

								// Transform the origin of the child element to see if it is within view
								GeneralTransform t = child.TransformToAncestor( scp );
								var	p = t.Transform( foundFirstItem ? new Point( child.RenderSize.Width, child.RenderSize.Height ) : new Point() );

								if(( !foundFirstItem ) && 
								   (( vertical ? p.Y : p.X ) >= 0.0 )) {
									// Found the first visible item
									firstVisibleItem = itemsControl.Items[i];
									bestLastItemIndex = i;
									foundFirstItem = true;
								}
								else if(( foundFirstItem ) &&
									   (( vertical ? p.Y : p.X ) < scp.ActualHeight )) {
									// Found a candidate for the last visible item
									bestLastItemIndex = i;
								}
							}
//						}

						if( bestLastItemIndex >= 0 ) {
							lastVisibleItem = itemsControl.Items[bestLastItemIndex];
						}

						// Update the item properties
						FirstItem = firstVisibleItem;
						LastItem = lastVisibleItem;
					}
				}
			}

		}

		/// <summary>
		///     Returns the parent of the specified type.
		/// </summary>
		private static T FindParent<T>( Visual v ) where T : Visual {
			v = VisualTreeHelper.GetParent( v ) as Visual;

			while( v != null ) {
				var	correctlyTyped = v as T;

				if( correctlyTyped != null ) {
					return correctlyTyped;
				}

				v = VisualTreeHelper.GetParent( v ) as Visual;
			}

			return null;
		}

		/// <summary>
		///     Notifies listeners of changes to properties on this object.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		///     Raises the PropertyChanged event.
		/// </summary>
		/// <param name="name">The name of the property.</param>
		protected void OnPropertyChanged( string name ) {
			if( PropertyChanged != null ) {
				PropertyChanged( this, new PropertyChangedEventArgs( name ) );
			}
		}
	}


	/// <summary>
	///     Displays a ToolTip next to the ScrollBar thumb while it is being dragged.
	/// </summary>
	public static class ScrollingPreview {
		// Keep one instance of a ToolTip and re-use it
		[ThreadStatic]
		private static ToolTip mPreviewToolTip;

		public static DataTemplate GetVerticalScrollingPreviewTemplate( DependencyObject obj ) {
			return (DataTemplate)obj.GetValue( VerticalScrollingPreviewTemplateProperty );
		}

		public static void SetVerticalScrollingPreviewTemplate( DependencyObject obj, DataTemplate value ) {
			obj.SetValue( VerticalScrollingPreviewTemplateProperty, value );
		}

		/// <summary>
		///     Allows for specifying a ContentTemplate for a ToolTip that will appear next to the 
		///     vertical ScrollBar while dragging the thumb.
		/// </summary>
		public static readonly DependencyProperty VerticalScrollingPreviewTemplateProperty =
            DependencyProperty.RegisterAttached( "VerticalScrollingPreviewTemplate", typeof( DataTemplate ), typeof( ScrollingPreview ), new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnVerticalScrollingPreviewTemplateChanged ) ) );

		private static void OnVerticalScrollingPreviewTemplateChanged( DependencyObject obj, DependencyPropertyChangedEventArgs e ) {
			if(( e.OldValue == null ) &&
			   ( e.NewValue != null )) {
				PostAttachToEvents( obj, (DataTemplate)e.NewValue, true );
			}
		}

		public static DataTemplate GetHorizontalScrollingPreviewTemplate( DependencyObject obj ) {
			return (DataTemplate)obj.GetValue( HorizontalScrollingPreviewTemplateProperty );
		}

		public static void SetHorizontalScrollingPreviewTemplate( DependencyObject obj, DataTemplate value ) {
			obj.SetValue( HorizontalScrollingPreviewTemplateProperty, value );
		}

		/// <summary>
		///     Allows for specifying a ContentTemplate for a ToolTip that will appear next to the 
		///     horizontal ScrollBar while dragging the thumb.
		/// </summary>
		public static readonly DependencyProperty HorizontalScrollingPreviewTemplateProperty =
            DependencyProperty.RegisterAttached( "HorizontalScrollingPreviewTemplate", typeof( DataTemplate ), typeof( ScrollingPreview ), new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnHorizontalScrollingPreviewTemplateChanged ) ) );

		private static void OnHorizontalScrollingPreviewTemplateChanged( DependencyObject obj, DependencyPropertyChangedEventArgs e ) {
			if(( e.OldValue == null ) && 
			   ( e.NewValue != null )) {
				PostAttachToEvents( obj, (DataTemplate)e.NewValue, false );
			}
		}

		private static void PostAttachToEvents( DependencyObject obj, DataTemplate dataTemplate, bool vertical ) {
			// Most likely, the control hasn't expanded its template, wait until
			// Loaded priority is reached before looking for elements.
			obj.Dispatcher.BeginInvoke( (NoParamCallback)( () => AttachToEvents( obj, dataTemplate, vertical )), DispatcherPriority.Loaded );
		}

		private delegate void NoParamCallback();

		private static void AttachToEvents( DependencyObject obj, DataTemplate dataTemplate, bool vertical ) {
			DependencyObject	source = obj;
			var					scrollViewer = FindElementOfType<ScrollViewer>( obj as FrameworkElement );

			if( scrollViewer != null ) {
				var	scrollBarPartName = vertical ? "PART_VerticalScrollBar" : "PART_HorizontalScrollBar";
				var	scrollBar = FindName<ScrollBar>( scrollBarPartName, scrollViewer );

				if( scrollBar != null ) {
					var	track = FindName<Track>( "PART_Track", scrollBar );

					if( track != null ) {
						var	thumb = track.Thumb;

						if( thumb != null ) {
							// Attach to DragStarted to open the tooltip
							thumb.DragStarted += delegate {
								ScrollingPreviewData data;

								if( mPreviewToolTip == null ) {
									// Create the ToolTip if this is the first time it's been used.
									mPreviewToolTip = new ToolTip();

									data = new ScrollingPreviewData();
									mPreviewToolTip.Content = data;
								}
								else {
									data = mPreviewToolTip.Content as ScrollingPreviewData;
								}

								if( data != null ) {
									// Update the content in the ToolTip
									data.UpdateScrollingValues( scrollBar );
									data.UpdateItem( source as ItemsControl, vertical );

									// Set the Placement and the PlacementTarget
									mPreviewToolTip.PlacementTarget = thumb;
									mPreviewToolTip.Placement = vertical ? PlacementMode.Left : PlacementMode.Top;

									mPreviewToolTip.VerticalOffset = 0.0;
									mPreviewToolTip.HorizontalOffset = 0.0;

									mPreviewToolTip.ContentTemplate = dataTemplate;
									mPreviewToolTip.IsOpen = true;
								}
							};

							// Attach to DragDelta to update the ToolTip's position
							thumb.DragDelta += delegate {
								if(( mPreviewToolTip != null ) &&
									// Check that we're within the range of the ScrollBar
								   ( scrollBar.Value > scrollBar.Minimum ) &&
								   ( scrollBar.Value < scrollBar.Maximum )) {
									var data = mPreviewToolTip.Content as ScrollingPreviewData;

									if( data != null ) {
										data.UpdateScrollingValues( scrollBar );
										data.UpdateItem( source as ItemsControl, vertical );
									}

									// This is a little trick to cause the ToolTip to update its position next to the Thumb
									if( vertical ) {
										mPreviewToolTip.VerticalOffset = mPreviewToolTip.VerticalOffset == 0.0 ? 0.001 : 0.0;
									}
									else {
										mPreviewToolTip.HorizontalOffset = mPreviewToolTip.HorizontalOffset == 0.0 ? 0.001 : 0.0;
									}
								}
							};

							// Attach to DragCompleted to close the ToolTip
							thumb.DragCompleted += delegate {
								if( mPreviewToolTip != null ) {
									mPreviewToolTip.IsOpen = false;
								}
							};

							// Attach to the Scroll event to update the ToolTip content
							scrollBar.Scroll += delegate {
								if( mPreviewToolTip != null ) {
									// The ScrollBar's value isn't updated quite yet, so
									// wait until Input priority
									scrollBar.Dispatcher.BeginInvoke( (NoParamCallback)delegate {
										var data = (ScrollingPreviewData)mPreviewToolTip.Content;
										data.UpdateScrollingValues( scrollBar );
										data.UpdateItem( source as ItemsControl, vertical );
									}, DispatcherPriority.Input );
								}
							};

							return;
						}
					}
				}

				// At this point, something wasn't found. If the computed visibility is not visible,
				// then add a handler to wait for it to become visible.
				if( ( vertical ? scrollViewer.ComputedVerticalScrollBarVisibility : scrollViewer.ComputedHorizontalScrollBarVisibility ) != Visibility.Visible ) {
					var propertyDescriptor = DependencyPropertyDescriptor.FromProperty( vertical ? ScrollViewer.ComputedVerticalScrollBarVisibilityProperty : ScrollViewer.ComputedHorizontalScrollBarVisibilityProperty, typeof( ScrollViewer ) );
					if( propertyDescriptor != null ) {
						var	handler = (EventHandler)delegate {
							if(( vertical ? scrollViewer.ComputedVerticalScrollBarVisibility : scrollViewer.ComputedHorizontalScrollBarVisibility ) == Visibility.Visible ) {
								EventHandler storedHandler = GetWaitForVisibleScrollBar( source );
								propertyDescriptor.RemoveValueChanged( scrollViewer, storedHandler );
								PostAttachToEvents( obj, dataTemplate, vertical );
							}
						};
						SetWaitForVisibleScrollBar( source, handler );
						propertyDescriptor.AddValueChanged( scrollViewer, handler );
					}
				}
			}
		}

		private static EventHandler GetWaitForVisibleScrollBar( DependencyObject obj ) {
			return (EventHandler)obj.GetValue( WaitForVisibleScrollBarProperty );
		}

		private static void SetWaitForVisibleScrollBar( DependencyObject obj, EventHandler value ) {
			obj.SetValue( WaitForVisibleScrollBarProperty, value );
		}

		/// <summary>
		///     Storage for the property change handler if waiting for a ScrollBar to appear
		///     for the first time.
		/// </summary>
		private static readonly DependencyProperty WaitForVisibleScrollBarProperty =
            DependencyProperty.RegisterAttached( "WaitForVisibleScrollBar", typeof( EventHandler ), typeof( ScrollingPreview ), new UIPropertyMetadata( null ) );

		/// <summary>
		///     Returns the template element of the given name within the Control.
		/// </summary>
		private static T FindName<T>( string name, Control control ) where T : FrameworkElement {
			var template = control.Template;

			if( template != null ) {
				return template.FindName( name, control ) as T;
			}

			return null;
		}

		/// <summary>
		///     Searches the subtree of an element (including that element) 
		///     for an element of a particluar type.
		/// </summary>
		private static T FindElementOfType<T>( FrameworkElement element ) where T : FrameworkElement {
			var	correctlyTyped = element as T;

			if( correctlyTyped != null ) {
				return correctlyTyped;
			}

			if( element != null ) {
				int numChildren = VisualTreeHelper.GetChildrenCount( element );

				for( int i = 0; i < numChildren; i++ ) {
					var	child = FindElementOfType<T>( VisualTreeHelper.GetChild( element, i ) as FrameworkElement );

					if( child != null ) {
						return child;
					}
				}

				// Popups continue in another window, jump to that tree
				var	popup = element as Popup;

				if( popup != null ) {
					return FindElementOfType<T>( popup.Child as FrameworkElement );
				}
			}

			return null;
		}
	}
}
