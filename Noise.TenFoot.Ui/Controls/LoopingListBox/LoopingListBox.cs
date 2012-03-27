/* Copyright (c) 2009, Dr. WPF
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 *   * Redistributions of source code must retain the above copyright
 *     notice, this list of conditions and the following disclaimer.
 * 
 *   * Redistributions in binary form must reproduce the above copyright
 *     notice, this list of conditions and the following disclaimer in the
 *     documentation and/or other materials provided with the distribution.
 * 
 *   * The name Dr. WPF may not be used to endorse or promote products
 *     derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY Dr. WPF ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL Dr. WPF BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Noise.TenFoot.Ui.Controls.LoopingListBox {
#if SURFACE
    public class LoopingListBox : Microsoft.Surface.Presentation.Controls.SurfaceListBox
#else
	public class LoopingListBox : ListBox
#endif
 {
		#region constructors

		#region static

		static LoopingListBox() {
			DefaultStyleKeyProperty.OverrideMetadata( typeof( LoopingListBox ), new FrameworkPropertyMetadata( typeof( LoopingListBox ) ) );
			EventManager.RegisterClassHandler( typeof( LoopingListBox ), LoopingListBox.BeginDragEvent, new BeginDragEventHandler( OnBeginDrag ) );
		}

		#endregion

		#endregion

		#region static members

		#region private methods

		private static Visual GetDescendantByType( Visual element, Type type ) {
			if( element == null ) return null;

			if( element.GetType() == type || element.GetType().IsSubclassOf( type ) ) return element;

			Visual foundElement = null;
			if( element is FrameworkElement )
				( element as FrameworkElement ).ApplyTemplate();

			for( int i = 0;
				i < VisualTreeHelper.GetChildrenCount( element ); i++ ) {
				Visual visual = VisualTreeHelper.GetChild( element, i ) as Visual;
				foundElement = GetDescendantByType( visual, type );
				if( foundElement != null )
					break;
			}

			return foundElement;
		}

		private static void OnDragHandleLeftButtonDown( object sender, MouseButtonEventArgs e ) {
			UIElement uie = sender as UIElement;
			SetIsDragging( uie, true );
			SetDragOrigin( uie, e.GetPosition( uie ) );
			uie.CaptureMouse();
		}

		private static void OnDragHandleLeftButtonUp( object sender, MouseButtonEventArgs e ) {
			UIElement uie = sender as UIElement;
			uie.ClearValue( DragOriginProperty );
			uie.ClearValue( IsDraggingProperty );
			if( Mouse.Captured == uie ) {
				uie.ReleaseMouseCapture();
			}
		}

		private static void OnDragHandleMouseLeave( object sender, MouseEventArgs e ) {
			UIElement uie = sender as UIElement;
			uie.ClearValue( DragOriginProperty );
			uie.ClearValue( IsDraggingProperty );
			if( Mouse.Captured == uie ) {
				uie.ReleaseMouseCapture();
			}
		}

		private static void OnDragHandleMouseMove( object sender, MouseEventArgs e ) {
			UIElement uie = sender as UIElement;
			if( GetIsDragging( uie ) ) {
				SetIsDragging( uie, false );
				Point dragOrigin = GetDragOrigin( uie );
				e.Handled = true;
				if( Mouse.Captured == uie ) {
					uie.ReleaseMouseCapture();
				}
				RaiseBeginDragEvent( uie, e.Device, dragOrigin, e.GetPosition( uie ) );
			}
		}

		private static void OnBeginDrag( object sender, BeginDragEventArgs e ) {
			UIElement element = e.OriginalSource as UIElement;
			LoopingListBox loopingListBox = sender as LoopingListBox;

			if( loopingListBox != null
				&& loopingListBox.LoopPanel != null
				&& element != null ) {
				Point origin = element.TranslatePoint( e.DragOrigin, loopingListBox.LoopPanel );
				Point current = element.TranslatePoint( e.CurrentPosition, loopingListBox.LoopPanel );
				loopingListBox.BeginDragOperation( e.Device, origin );
				loopingListBox.UpdateDragPosition( current );
			}
		}

		#endregion

		#endregion

		#region dependency properties

		#region DragOrigin

		/// <summary>
		/// DragOrigin Protected Dependency Property
		/// </summary>
		protected static readonly DependencyProperty DragOriginProperty
            = DependencyProperty.RegisterAttached( "DragOrigin", typeof( Point ), typeof( LoopingListBox ),
				new FrameworkPropertyMetadata( new Point() ) );

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

		#endregion

		#region IsDragging

		/// <summary>
		/// IsDragging Protected Dependency Property
		/// </summary>
		protected static readonly DependencyProperty IsDraggingProperty
            = DependencyProperty.RegisterAttached( "IsDragging", typeof( bool ), typeof( LoopingListBox ),
				new FrameworkPropertyMetadata( (bool)false ) );

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

		#endregion

		#region IsDragHandle

		/// <summary>
		/// IsDragHandle Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty IsDragHandleProperty =
            DependencyProperty.RegisterAttached( "IsDragHandle", typeof( bool ), typeof( LoopingListBox ),
				new FrameworkPropertyMetadata( (bool)false,
					new PropertyChangedCallback( OnIsDragHandleChanged ) ) );

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
				UIElement uie = d as UIElement;
				if( (bool)e.NewValue ) {
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

		#endregion

		#region Offset

		/// <summary>
		/// Offset Dependency Property
		/// </summary>
		public static readonly DependencyProperty OffsetProperty =
            LoopPanel.OffsetProperty.AddOwner( typeof( LoopingListBox ) );

		/// <summary>
		/// Gets or sets the Offset property.  This dependency property 
		/// indicates the offset to be used by the items panel (the LoopPanel).
		/// </summary>
		public double Offset {
			get { return (double)GetValue( OffsetProperty ); }
			set { SetValue( OffsetProperty, value ); }
		}

		#endregion

		#region Orientation

		/// <summary>
		/// Orientation Dependency Property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty =
            LoopPanel.OrientationProperty.AddOwner( typeof( LoopingListBox ) );

		/// <summary>
		/// Gets or sets the Orientation property.  This dependency property 
		/// indicates the orientation to be used by the items panel (the LoopPanel).
		/// </summary>
		public Orientation Orientation {
			get { return (Orientation)GetValue( OrientationProperty ); }
			set { SetValue( OrientationProperty, value ); }
		}

		#endregion

		#region RelativeOffset

		/// <summary>
		/// RelativeOffset Dependency Property
		/// </summary>
		public static readonly DependencyProperty RelativeOffsetProperty =
            LoopPanel.RelativeOffsetProperty.AddOwner( typeof( LoopingListBox ) );

		/// <summary>
		/// Gets or sets the RelativeOffset property.  This dependency property 
		/// indicates the relative offset to be used by the items panel (the LoopPanel).
		/// </summary>
		public double RelativeOffset {
			get { return (double)GetValue( RelativeOffsetProperty ); }
			set { SetValue( RelativeOffsetProperty, value ); }
		}

		#endregion

		#endregion

		#region properties

		#region protected

		protected InputDevice CapturedDevice { get; set; }

		protected LoopPanel LoopPanel { get; private set; }

		#endregion

		#endregion

		#region methods

		#region public

		public override void OnApplyTemplate() {
			LoopPanel = GetDescendantByType( this, typeof( LoopPanel ) ) as LoopPanel;
			base.OnApplyTemplate();
		}

		public void Scroll( double viewportUnits ) {
			if( LoopPanel != null ) {
				LoopPanel.Scroll( viewportUnits );
			}
		}

		#endregion

		#region protected

		protected virtual bool BeginDragOperation( InputDevice deviceToCapture, Point initialDragPosition ) {
			// This is a helper method that provides for easy dragging of the 
			// LoopPanel's Offset using the Mouse. It can be overridden to provide 
			// support for other input devices like a Surface Contact.
			bool result = false;
			if( CapturedDevice == null ) {
				_lastDragPosition = initialDragPosition;
				if( deviceToCapture is MouseDevice ) {
					MouseDevice md = deviceToCapture as MouseDevice;

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
			if( e.Device == CapturedDevice && LoopPanel != null ) {
				UpdateDragPosition( ( CapturedDevice as MouseDevice ).GetPosition( LoopPanel ) );
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
				? ( _lastDragPosition.X - currentDragPosition.X )
				: ( _lastDragPosition.Y - currentDragPosition.Y ) );
			_lastDragPosition = currentDragPosition;
		}

		#endregion

		#endregion

		#region events

		#region public

		#region BeginDrag

		/// <summary>
		/// BeginDrag Routed Event
		/// </summary>
		public static readonly RoutedEvent BeginDragEvent = EventManager.RegisterRoutedEvent( "BeginDrag",
			RoutingStrategy.Bubble, typeof( BeginDragEventHandler ), typeof( LoopingListBox ) );

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
			if( target == null ) return null;

			BeginDragEventArgs args = new BeginDragEventArgs( device, dragOrigin, currentPosition );
			args.RoutedEvent = BeginDragEvent;
			RoutedEventHelper.RaiseEvent( target, args );
			return args;
		}

		#endregion

		#endregion

		#endregion

		#region fields

		private Point _lastDragPosition;

		#endregion
	}
}
