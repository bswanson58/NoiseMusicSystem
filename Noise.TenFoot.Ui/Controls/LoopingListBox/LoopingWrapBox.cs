using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Noise.TenFoot.Ui.Controls.LoopingListBox {
	public class LoopingWrapBox : ListBox {
		private		Point			mLastDragPosition;
		protected	InputDevice		CapturedDevice { get; set; }
		protected	LoopWrapPanel	LoopPanel { get; private set; }

		static LoopingWrapBox() {
			DefaultStyleKeyProperty.OverrideMetadata( typeof( LoopingWrapBox ), new FrameworkPropertyMetadata( typeof( LoopingWrapBox )));
			EventManager.RegisterClassHandler( typeof( LoopingWrapBox ), BeginDragEvent, new BeginDragEventHandler( OnBeginDrag ));
		}

		private static Visual GetDescendantByType( Visual element, Type type ) {
			if( element == null ) {
				return null;
			}

			if( element.GetType() == type || element.GetType().IsSubclassOf( type )) {
				return element;
			}

			Visual foundElement = null;
			if( element is FrameworkElement ) {
				( element as FrameworkElement ).ApplyTemplate();
			}

			for( int i = 0; i < VisualTreeHelper.GetChildrenCount( element ); i++ ) {
				var visual = VisualTreeHelper.GetChild( element, i ) as Visual;

				foundElement = GetDescendantByType( visual, type );
				if( foundElement != null ) {
					break;
				}
			}

			return foundElement;
		}

		private static void OnDragHandleLeftButtonDown( object sender, MouseButtonEventArgs e ) {
			var uie = sender as UIElement;
			
			if( uie != null ) {
				SetIsDragging( uie, true );
				SetDragOrigin( uie, e.GetPosition( uie ) );
				uie.CaptureMouse();
			}
		}

		private static void OnDragHandleLeftButtonUp( object sender, MouseButtonEventArgs e ) {
			var uie = sender as UIElement;

			if( uie != null ) {
				uie.ClearValue( DragOriginProperty );
				uie.ClearValue( IsDraggingProperty );
				if( Mouse.Captured == uie ) {
					uie.ReleaseMouseCapture();
				}
			}
		}

		private static void OnDragHandleMouseLeave( object sender, MouseEventArgs e ) {
			var uie = sender as UIElement;

			if( uie != null ) {
				uie.ClearValue( DragOriginProperty );
				uie.ClearValue( IsDraggingProperty );
				if( Mouse.Captured == uie ) {
					uie.ReleaseMouseCapture();
				}
			}
		}

		private static void OnDragHandleMouseMove( object sender, MouseEventArgs e ) {
			var uie = sender as UIElement;
			if( uie != null ) {
				if( GetIsDragging( uie )) {
					SetIsDragging( uie, false );
					Point dragOrigin = GetDragOrigin( uie );
					e.Handled = true;
					if( Mouse.Captured == uie ) {
						uie.ReleaseMouseCapture();
					}
					RaiseBeginDragEvent( uie, e.Device, dragOrigin, e.GetPosition( uie ) );
				}
			}
		}

		private static void OnBeginDrag( object sender, BeginDragEventArgs e ) {
			var element = e.OriginalSource as UIElement;
			var loopingWrapBox = sender as LoopingWrapBox;

			if(( loopingWrapBox != null ) &&
			   ( loopingWrapBox.LoopPanel != null ) &&
			   ( element != null )) {
				var origin = element.TranslatePoint( e.DragOrigin, loopingWrapBox.LoopPanel );
				var current = element.TranslatePoint( e.CurrentPosition, loopingWrapBox.LoopPanel );

				loopingWrapBox.BeginDragOperation( e.Device, origin );
				loopingWrapBox.UpdateDragPosition( current );
			}
		}

		/// <summary>
		/// DragOrigin Protected Dependency Property
		/// </summary>
		protected static readonly DependencyProperty DragOriginProperty
            = DependencyProperty.RegisterAttached( "DragOrigin", typeof( Point ), typeof( LoopingWrapBox ),
				new FrameworkPropertyMetadata( new Point()));

		/// <summary>
		/// Gets the DragOrigin property.  This dependency property 
		/// indicates the point where the drag operation began in the coordinate space of the element acting as the drag handle.
		/// </summary>
		protected static Point GetDragOrigin( DependencyObject d ) {
			return (Point)d.GetValue( DragOriginProperty );
		}

		/// <summary>
		/// Provides a secure method for setting the DragOrigin property.  
		/// This dependency property indicates the point where the drag operation began in the coordinate space of the element acting as the drag handle.
		/// </summary>
		protected static void SetDragOrigin( DependencyObject d, Point value ) {
			d.SetValue( DragOriginProperty, value );
		}

		/// <summary>
		/// IsDragging Protected Dependency Property
		/// </summary>
		protected static readonly DependencyProperty IsDraggingProperty
            = DependencyProperty.RegisterAttached( "IsDragging", typeof( bool ), typeof( LoopingWrapBox ),
				new FrameworkPropertyMetadata( false ));

		/// <summary>
		/// Gets the IsDragging property.  This dependency property 
		/// indicates whether the target drag handle is currently being dragged.
		/// </summary>
		protected static bool GetIsDragging( DependencyObject d ) {
			return (bool)d.GetValue( IsDraggingProperty );
		}

		/// <summary>
		/// Provides a secure method for setting the IsDragging property.  
		/// This dependency property indicates whether the target drag handle is currently being dragged.
		/// </summary>
		protected static void SetIsDragging( DependencyObject d, bool value ) {
			d.SetValue( IsDraggingProperty, value );
		}

		/// <summary>
		/// IsDragHandle Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty IsDragHandleProperty =
            DependencyProperty.RegisterAttached( "IsDragHandle", typeof( bool ), typeof( LoopingWrapBox ),
				new FrameworkPropertyMetadata( false, OnIsDragHandleChanged ));

		/// <summary>
		/// Gets the IsDragHandle property.  This dependency property 
		/// indicates that the element is a drag handle for an ancestor LoopingListBox.
		/// </summary>
		public static bool GetIsDragHandle( DependencyObject d ) {
			return (bool)d.GetValue( IsDragHandleProperty );
		}

		/// <summary>
		/// Sets the IsDragHandle property.  This dependency property 
		/// indicates that the element is a drag handle for an ancestor LoopingListBox.
		/// </summary>
		public static void SetIsDragHandle( DependencyObject d, bool value ) {
			d.SetValue( IsDragHandleProperty, value );
		}

		/// <summary>
		/// Handles changes to the IsDragHandle property.
		/// </summary>
		private static void OnIsDragHandleChanged( DependencyObject d, DependencyPropertyChangedEventArgs e ) {
			// attach/detach the drag behavior on the target element
			if( d is UIElement ) {
				var uie = d as UIElement;

				if((bool)e.NewValue ) {
					uie.MouseLeave += OnDragHandleMouseLeave;
					uie.AddHandler( MouseLeftButtonDownEvent, new MouseButtonEventHandler( OnDragHandleLeftButtonDown ), true );
					uie.AddHandler( MouseLeftButtonUpEvent, new MouseButtonEventHandler( OnDragHandleLeftButtonUp ), true );
					uie.MouseMove += OnDragHandleMouseMove;
				}
				else {
					uie.MouseLeave -= OnDragHandleMouseLeave;
					uie.RemoveHandler( MouseLeftButtonDownEvent, new MouseButtonEventHandler( OnDragHandleLeftButtonDown ) );
					uie.RemoveHandler( MouseLeftButtonUpEvent, new MouseButtonEventHandler( OnDragHandleLeftButtonUp ) );
					uie.MouseMove -= OnDragHandleMouseMove;
				}
			}
		}

		/// <summary>
		/// Offset Dependency Property
		/// </summary>
		public static readonly DependencyProperty OffsetProperty =
            LoopWrapPanel.OffsetProperty.AddOwner( typeof( LoopingWrapBox ),
													new FrameworkPropertyMetadata( 0.5d, FrameworkPropertyMetadataOptions.AffectsArrange | 
																						 FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ));

		/// <summary>
		/// Gets or sets the Offset property.  This dependency property 
		/// indicates the offset to be used by the items panel (the LoopWrapPanel).
		/// </summary>
		public double Offset {
			get { return (double)GetValue( OffsetProperty ); }
			set { SetValue( OffsetProperty, value ); }
		}

		/// <summary>
		/// Orientation Dependency Property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty =
            LoopWrapPanel.OrientationProperty.AddOwner( typeof( LoopingWrapBox ));

		/// <summary>
		/// Gets or sets the Orientation property.  This dependency property 
		/// indicates the orientation to be used by the items panel (the LoopWrapPanel).
		/// </summary>
		public Orientation Orientation {
			get { return (Orientation)GetValue( OrientationProperty ); }
			set { SetValue( OrientationProperty, value ); }
		}

		/// <summary>
		/// RelativeOffset Dependency Property
		/// </summary>
		public static readonly DependencyProperty RelativeOffsetProperty =
            LoopWrapPanel.RelativeOffsetProperty.AddOwner( typeof( LoopingWrapBox ) );

		/// <summary>
		/// Gets or sets the RelativeOffset property.  This dependency property 
		/// indicates the relative offset to be used by the items panel (the LoopWrapPanel).
		/// </summary>
		public double RelativeOffset {
			get { return (double)GetValue( RelativeOffsetProperty ); }
			set { SetValue( RelativeOffsetProperty, value ); }
		}

		public static readonly DependencyProperty LastItemMarginProperty =
			LoopWrapPanel.LastItemMarginProperty.AddOwner( typeof( LoopingWrapBox ));

		/// <summary>
		/// Gets or sets the LastItemMargin property. The dependency property adjusted the spacing
		/// between the last and first items in the looping list. The amount is measured in terms of
		/// child item extent i.e. 1.0 represents the height/width of one child item.
		/// </summary>
		public double LastItemMargin {
			get{ return((double)GetValue( LastItemMarginProperty )); }
			set{ SetValue( LastItemMarginProperty, value ); }
		}

		public override void OnApplyTemplate() {
			LoopPanel = GetDescendantByType( this, typeof( LoopWrapPanel )) as LoopWrapPanel;
			base.OnApplyTemplate();
		}

		public void Scroll( double viewportUnits ) {
			if( LoopPanel != null ) {
				LoopPanel.Scroll( viewportUnits );
			}
		}

		protected virtual bool BeginDragOperation( InputDevice deviceToCapture, Point initialDragPosition ) {
			// This is a helper method that provides for easy dragging of the 
			// LoopPanel's Offset using the Mouse. It can be overridden to provide 
			// support for other input devices like a Surface Contact.
			bool result = false;
			if( CapturedDevice == null ) {
				mLastDragPosition = initialDragPosition;
				if( deviceToCapture is MouseDevice ) {
					var md = deviceToCapture as MouseDevice;

					if( md.Captured != null ) md.Captured.ReleaseMouseCapture();

					// By default, we support dragging via the left mouse button.  
					// If another button is required for dragging, this method should be overridden.
					if( md.Captured == null && md.LeftButton == MouseButtonState.Pressed ) {
						CapturedDevice = md;
						md.Capture( this );
						result = true;
					}
				}
			}
			return result;
		}

		protected virtual void EndDragOperation() {
			if( CapturedDevice is MouseDevice ) {
				ReleaseMouseCapture();
			}
			CapturedDevice = null;
		}

		protected override void OnIsMouseCapturedChanged( DependencyPropertyChangedEventArgs e ) {
			// do not call the base method during a drag operation because
			// we do not want the autoscroll behavior of a normal listbox
			if( CapturedDevice == null ) base.OnIsMouseCapturedChanged( e );
		}

		protected override void OnLostMouseCapture( MouseEventArgs e ) {
			if( e.Device == CapturedDevice ) {
				EndDragOperation();
			}
		}

		protected override void OnMouseLeftButtonUp( MouseButtonEventArgs e ) {
			if( e.Device == CapturedDevice ) {
				EndDragOperation();
			}
		}

		protected override void OnMouseMove( MouseEventArgs e ) {
			if(( e.Device == CapturedDevice ) &&
			   ( CapturedDevice is MouseDevice ) &&
			   ( LoopPanel != null )) {
				UpdateDragPosition(( CapturedDevice as MouseDevice ).GetPosition( LoopPanel ) );
				e.Handled = true;
			}
			else {
				base.OnMouseMove( e );
			}
		}

		protected virtual void UpdateDragPosition( Point currentDragPosition ) {
			if( LoopPanel == null
				|| LoopPanel.Children.Count == 0 ) return;

			LoopPanel.Scroll( ( LoopPanel.Orientation == Orientation.Horizontal )
				? ( mLastDragPosition.X - currentDragPosition.X )
				: ( mLastDragPosition.Y - currentDragPosition.Y ) );
			mLastDragPosition = currentDragPosition;
		}

		/// <summary>
		/// BeginDrag Routed Event
		/// </summary>
		public static readonly RoutedEvent BeginDragEvent = EventManager.RegisterRoutedEvent( "BeginDrag",
			RoutingStrategy.Bubble, typeof( BeginDragEventHandler ), typeof( LoopingWrapBox ) );

		/// <summary>
		/// Occurs when an element marked with the IsDragHandle attached property begins a drag operation.
		/// </summary>
		public event BeginDragEventHandler BeginDrag {
			add { AddHandler( BeginDragEvent, value ); }
			remove { RemoveHandler( BeginDragEvent, value ); }
		}

		/// <summary>
		/// A helper method to raise the BeginDrag event.
		/// </summary>
		/// <param name="device">The input device that owns the drag operation.</param>
		/// <param name="dragOrigin">The origin of the drag operation relative to the input element raising the event.</param>
		/// <param name="currentPosition">The current position of the input device relative to the input element raising the event.</param>
		protected BeginDragEventArgs RaiseBeginDragEvent( InputDevice device, Point dragOrigin, Point currentPosition ) {
			return RaiseBeginDragEvent( this, device, dragOrigin, currentPosition );
		}

		/// <summary>
		/// A static helper method to raise the BeginDrag event on a target element.
		/// </summary>
		/// <param name="target">UIElement or ContentElement on which to raise the event</param>
		/// <param name="device">The input device that owns the drag operation.</param>
		/// <param name="dragOrigin">The origin of the drag operation relative to the input element raising the event.</param>
		/// <param name="currentPosition">The current position of the input device relative to the input element raising the event.</param>
		internal static BeginDragEventArgs RaiseBeginDragEvent( DependencyObject target, InputDevice device, Point dragOrigin, Point currentPosition ) {
			if( target == null ) {
				return null;
			}

			var args = new BeginDragEventArgs( device, dragOrigin, currentPosition ) { RoutedEvent = BeginDragEvent };

			RoutedEventHelper.RaiseEvent( target, args );
			return args;
		}
	}
}
