using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Noise.TenFoot.Ui.Controls.LoopingListBox {
	public class LoopWrapPanel : Panel {
		private bool	mIsMeasured;
		private bool	mInMeasure;
		private int		mPivotalChildIndex;

		static LoopWrapPanel() {
			EventManager.RegisterClassHandler( typeof( LoopWrapPanel ), RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler( OnRequestBringIntoView ));
		}

		public LoopWrapPanel() {
			mPivotalChildIndex = -1;
		}

		/// <summary>
		/// BringChildrenIntoView Dependency Property
		/// </summary>
		public static readonly DependencyProperty BringChildrenIntoViewProperty =
            DependencyProperty.Register( "BringChildrenIntoView", typeof( bool ), typeof( LoopWrapPanel ), new FrameworkPropertyMetadata( false ));

		/// <summary>
		/// Gets or sets the BringChildrenIntoView property.  This dependency property 
		/// indicates whether the panel should automatically adjust its Offset property to bring 
		/// its direct children into view when they raise the RequestBringIntoView event.
		/// </summary>
		public bool BringChildrenIntoView {
			get { return (bool)GetValue( BringChildrenIntoViewProperty ); }
			set { SetValue( BringChildrenIntoViewProperty, value ); }
		}

		/// <summary>
		/// Offset Dependency Property
		/// </summary>
		public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register( "Offset", typeof( double ), typeof( LoopWrapPanel ),
				new FrameworkPropertyMetadata( 0.5d, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault ) );

		/// <summary>
		/// Gets or sets the Offset property.  This dependency property indicates the logical 
		/// offset of children (in the orientation direction) within the LoopPanel. 
		/// 
		/// The Offset property provides for logical units in the direction of the
		/// panel's orientation.  This allows the panel to support per-item scrolling.
		/// Each logical unit represents a single child.  Since children can have 
		/// varying sizes, this means a fraction of a unit (such as 0.1) will represent
		/// different lengths for different children.  All placement of children occurs
		/// around the current pivotal child.  The pivotal child's index is determined 
		/// by truncating the Offset value and applying a modulo using the child count.
		/// 
		/// Another way to think about this property is that its non-fractional portion
		/// represents the index of the pivotal child (the child around which other 
		/// children are arranged). If the value is between 0 (inclusive) 
		/// and 1 (exclusive), the first child is the pivotal child.  The fractional
		/// portion represents an offset for the pivotal child. 
		/// 
		/// The default value of 0.5 means that the first child is the pivotal child and 
		/// it is centered along its extent (the child's width for a horizontal 
		/// orientation or height for a vertical orientation).
		/// </summary>
		public double Offset {
			get { return (double)GetValue( OffsetProperty ); }
			set { SetValue( OffsetProperty, value ); }
		}

		/// <summary>
		/// Orientation Dependency Property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty =
            StackPanel.OrientationProperty.AddOwner( typeof( LoopWrapPanel ),
				new FrameworkPropertyMetadata( Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsArrange ));

		/// <summary>
		/// Gets or sets the Orientation property.  This dependency property 
		/// indicates the layout orientation of the looping children.
		/// </summary>
		public Orientation Orientation {
			get { return (Orientation)GetValue( OrientationProperty ); }
			set { SetValue( OrientationProperty, value ); }
		}

		/// <summary>
		/// RelativeOffset Dependency Property
		/// </summary>
		public static readonly DependencyProperty RelativeOffsetProperty =
            DependencyProperty.Register( "RelativeOffset", typeof( double ), typeof( LoopWrapPanel ),
				new FrameworkPropertyMetadata( 0.5d, FrameworkPropertyMetadataOptions.AffectsArrange ), IsRelativeOffsetValid );

		/// <summary>
		/// Gets or sets the RelativeOffset property.  This dependency property 
		/// indicates where the pivotal child is positioned within the panel. 
		/// The default value of 0.5 means that the pivotal child is positioned
		/// with its edge at the center of the panel.
		/// </summary>
		public double RelativeOffset {
			get { return (double)GetValue( RelativeOffsetProperty ); }
			set { SetValue( RelativeOffsetProperty, value ); }
		}

		private static bool IsRelativeOffsetValid( object value ) {
			var	v = (double)value;

			return ( !double.IsNaN( v )
				&& !double.IsPositiveInfinity( v )
				&& !double.IsNegativeInfinity( v )
				&& v >= 0.0d && v <= 1.0d );
		}

		public static readonly DependencyProperty LastItemMarginProperty =
			DependencyProperty.Register( "LastItemMargin", typeof( double ), typeof( LoopWrapPanel ),
				new FrameworkPropertyMetadata( 0.5d, FrameworkPropertyMetadataOptions.AffectsArrange ), IsLastItemMarginValid );

		private static bool IsLastItemMarginValid( object value ) {
			var	v = (double)value;

			return ( !double.IsNaN( v )
				&& !double.IsPositiveInfinity( v )
				&& !double.IsNegativeInfinity( v )
				&& v >= 0.0d && v <= 10.0d );
		}

		/// <summary>
		/// Gets or sets the LastItemMargin property. The dependency property adjusted the spacing
		/// between the last and first items in the looping list. The amount is measured in terms of
		/// child item extent i.e. 1.0 represents the height/width of one child item.
		/// </summary>
		public double LastItemMargin {
			get{ return((double)GetValue( LastItemMarginProperty )); }
			set{ SetValue( LastItemMarginProperty, value ); }
		}

		/// <summary>
		/// Provides a helper method for scrolling the panel by viewport units rather than
		/// adjusting the Offset property (which uses logical units)
		/// </summary>
		/// <param name="viewportUnits">The number of device independent pixels to scroll.  A positive value 
		/// scrolls the Offset forward and a negative value scrolls it backward</param>
		public void Scroll( double viewportUnits ) {
			int childCount = InternalChildren.Count;
			if( childCount == 0 ) return;

			// determine the new Offset value required to move the specified viewport units
			double	newOffset = Offset;
			int		childIndex = mPivotalChildIndex;
			bool	isHorizontal = ( Orientation == Orientation.Horizontal );
			bool	isForwardMovement = ( viewportUnits > 0 );
			int		directionalFactor = isForwardMovement ? 1 : -1;
			double	remainingExtent = Math.Abs( viewportUnits );
			UIElement child = InternalChildren[childIndex];
			double	childExtent = isHorizontal ? child.DesiredSize.Width : child.DesiredSize.Height;
			double	fractionalOffset = ( Offset > 0 ) ? Offset - Math.Truncate( Offset ) : 1.0d - Math.Truncate( Offset ) + Offset;
			double	childRemainingExtent = isForwardMovement ? childExtent - fractionalOffset * childExtent : fractionalOffset * childExtent;

			if( DoubleHelper.LessThanOrVirtuallyEqual( childRemainingExtent, remainingExtent )) {
				newOffset = Math.Round( isForwardMovement ? newOffset + 1 - fractionalOffset : newOffset - fractionalOffset );
				remainingExtent -= childRemainingExtent;
				while( DoubleHelper.StrictlyGreaterThan( remainingExtent, 0.0d ) ) {
					childIndex = isForwardMovement ? ( childIndex + 1 ) % childCount : ( childIndex == 0 ? childCount - 1 : childIndex - 1 );
					child = InternalChildren[childIndex];
					childExtent = isHorizontal ? child.DesiredSize.Width : child.DesiredSize.Height;
					if( DoubleHelper.LessThanOrVirtuallyEqual( childExtent, remainingExtent )) {
						newOffset += 1.0d * directionalFactor;
						remainingExtent -= childExtent;
					}
					else {
						newOffset += remainingExtent * directionalFactor / childExtent;
						remainingExtent = 0.0d;
					}
				}
			}
			else {
				newOffset += remainingExtent * directionalFactor / childExtent;
			}

			Offset = newOffset;
		}

		private void ArrangeWrap( int firstChild, int itemCount, Point childOffset, Point wrapOrigin, Size childSize ) {
			UIElementCollection children = InternalChildren;

			for( var indexOffset = 0; indexOffset < itemCount; indexOffset++ ) {
				var childIndex = firstChild + indexOffset;
				if( childIndex < children.Count ) {
					var child = children[firstChild + indexOffset];
					var childRect = new Rect( wrapOrigin, childSize );

					child.Arrange( childRect );

					wrapOrigin.Offset( childOffset.X, childOffset.Y );
				}
			}
		}

		private double CalculateWrapTopLeft( double controlExtent, double childExtent, int childCount ) {
			// Currently calculated as being centered.
			var spacing = controlExtent - ( childExtent * childCount );

			return( spacing / 2.0 );
		}

		protected override Size ArrangeOverride( Size finalSize ) {
			var		controlBounds = new Rect( finalSize );
			bool	isHorizontal = ( Orientation == Orientation.Horizontal );
			var		children = InternalChildren;
			int		childCount = children.Count;
			var		childRect = new Rect();
			var		childSize = new Size();
			var		childOffset = new Point();
			double	childExtent = 0.0;
			double	wrapTopLeft = 0.0;
			int		wrapCount = 0;
			int		wrapItemCount = 0;
			double	nextEdge = 0, priorEdge = 0;
			int		nextWrapIndex = 0, priorWrapIndex = 0;

			mPivotalChildIndex = -1;

			// arrange pivotal child
			if( childCount > 0 ) {
				var		wrapOrigin = new Point();

				// determine pivotal child index
				double adjustedOffset = Offset % childCount;
				if( adjustedOffset < 0 ) {
					adjustedOffset = ( adjustedOffset + childCount ) % childCount;
				}
				mPivotalChildIndex = (int)adjustedOffset;

				var child = children[mPivotalChildIndex];
				if( isHorizontal ) {
					childExtent = child.DesiredSize.Width;
					wrapOrigin.X = finalSize.Width * RelativeOffset - childExtent * ( adjustedOffset - Math.Truncate( adjustedOffset ));
					childSize.Width = childExtent;
					childSize.Height = Math.Min( finalSize.Height, child.DesiredSize.Height );

					wrapItemCount = CalculateWrapCount( finalSize.Height, child.DesiredSize.Height );
					wrapTopLeft = CalculateWrapTopLeft( finalSize.Height, child.DesiredSize.Height, wrapItemCount );
					wrapOrigin.Y = wrapTopLeft;
					childOffset.Y = childSize.Height;

					nextEdge = wrapOrigin.X + childExtent;
					priorEdge = wrapOrigin.X - childExtent;
				}
				else {
					childExtent = child.DesiredSize.Height;
					wrapOrigin.Y = finalSize.Height * RelativeOffset - childExtent * ( adjustedOffset - Math.Truncate( adjustedOffset ));
					childSize.Height = childExtent;
					childSize.Width = Math.Min( finalSize.Width, child.DesiredSize.Width );

					wrapItemCount = CalculateWrapCount( finalSize.Width, child.DesiredSize.Width );
					wrapTopLeft = CalculateWrapTopLeft( finalSize.Width, child.DesiredSize.Width, wrapItemCount );
					wrapOrigin.X = wrapTopLeft;
					childOffset.X = childSize.Width;

					nextEdge = wrapOrigin.Y + childExtent;
					priorEdge = wrapOrigin.Y - childExtent;
				}

				var pivotalWrapIndex = mPivotalChildIndex / wrapItemCount;

				ArrangeWrap( pivotalWrapIndex * wrapItemCount, wrapItemCount, childOffset, wrapOrigin, childSize  );

				wrapCount = CalculateWraps( childCount, wrapItemCount );
				nextWrapIndex = ( pivotalWrapIndex + 1 ) % wrapCount;
				priorWrapIndex = ( pivotalWrapIndex == 0 ) ? wrapCount - 1 : pivotalWrapIndex - 1;

				if( pivotalWrapIndex == 0 ) {
					priorEdge -= LastItemMargin * childExtent;
				}

				if( pivotalWrapIndex == ( wrapCount - 1 )) {
					nextEdge += LastItemMargin * childExtent;
				}
			}

			// arrange subsequent and prior children until we run out of room
			bool isNextFull = false, isPriorFull = false;

			for( int i = 1; i < wrapCount; i++ ) {
				// for odd iterations, arrange the next wrap row
				// for even iterations, arrange the prior wrap row
				bool isArrangingNext = ( i % 2 == 1 );

				if( isArrangingNext && isNextFull && !isPriorFull ) {
					isArrangingNext = false;
				}
				else if( !isArrangingNext && isPriorFull && !isNextFull ) {
					isArrangingNext = true;
				}

				// get the wrap index and adjust the appropriate counter
				int wrapIndex = nextWrapIndex;

				if( isArrangingNext ) {
					nextWrapIndex = ( nextWrapIndex + 1 ) % wrapCount;
				}
				else {
					wrapIndex = priorWrapIndex;
					priorWrapIndex = ( priorWrapIndex > 0 ) ? priorWrapIndex - 1 : wrapCount - 1;
				}

				var		wrapRect = new Rect();

				// determine the child's arrange rect
				if( isHorizontal ) {
					wrapRect.X = isArrangingNext ? nextEdge : priorEdge;
					wrapRect.Y = wrapTopLeft;

					wrapRect.Width = childExtent;
					wrapRect.Height = Math.Max( finalSize.Height, childSize.Height * wrapItemCount );
				}
				else {
					wrapRect.Y = isArrangingNext ? nextEdge : priorEdge;
					wrapRect.Y = wrapTopLeft;

					wrapRect.Height = childExtent;
					wrapRect.Width = Math.Max( finalSize.Width, childSize.Width * wrapItemCount );
				}

				if( isHorizontal ) {
					if( isArrangingNext ) {
						nextEdge = wrapRect.X + childExtent;

						if( wrapIndex == ( wrapCount - 1 )) {
							nextEdge += LastItemMargin * childExtent;
						}
					}
					else {
						priorEdge = wrapRect.X - childExtent;

						if( wrapIndex == 0 ) {
							priorEdge -= LastItemMargin * childExtent;
						}
					}
				}
				else {
					if( isArrangingNext ) {
						nextEdge = wrapRect.Y + childExtent;

						if( wrapIndex == ( wrapCount - 1 )) {
							nextEdge += LastItemMargin * childExtent;
						}
					}
					else {
						priorEdge = childRect.Y - childExtent;

						if( wrapIndex == 0 ) {
							priorEdge -= LastItemMargin * childExtent;
						}
					}
				}

				var intersection = Rect.Intersect( wrapRect, controlBounds );

				if((!intersection.IsEmpty ) &&
				   ( intersection.Width * intersection.Height > 1.0e-10 )) {
					ArrangeWrap( wrapIndex * wrapItemCount, wrapItemCount, childOffset, wrapRect.Location, childSize );
				}
				else {
					ArrangeWrap( wrapIndex * wrapItemCount, wrapItemCount, childOffset, wrapRect.Location, new Size());

					// if there's no room for the child, set the appropriate full flag
					if( isArrangingNext ) {
						isNextFull = true;
					}
					else {
						isPriorFull = true;
					}
				}
			}

			return( finalSize );
		}

		private int CalculateWraps( int childCount, int wrapCount ) {
			var retValue = childCount / wrapCount;

			if(( childCount % wrapCount ) > 0 ) {
				retValue++;
			}

			return( retValue );
		}

		private int CalculateWrapCount( double extent, double itemExtent ) {
			return( (int)( extent / itemExtent ));
		}

		protected override Size MeasureOverride( Size availableSize ) {
			var		desiredSize = new Size();

			mInMeasure = true;
			try {
				UIElementCollection children = InternalChildren;
				bool isHorizontal = ( Orientation == Orientation.Horizontal );
				Size childSize = availableSize;

				// children will be measured to their desired size in the logical direction
				if( isHorizontal ) {
					childSize.Width = Double.PositiveInfinity;
				}
				else {
					childSize.Height = Double.PositiveInfinity;
				}

				// this calculation assumes that all children will measure the same size and uses
				// the measurement of the first child.
				if( children.Count > 0 ) {
					UIElement	firstChild = children[0];

					firstChild.Measure( childSize );
					var commonChildSize = firstChild.DesiredSize;
					int	wrapCount;

					if( isHorizontal ) {
						wrapCount = CalculateWrapCount( availableSize.Height, commonChildSize.Height );

						desiredSize.Height = commonChildSize.Height * wrapCount;
						desiredSize.Width = commonChildSize.Width * CalculateWraps( children.Count, wrapCount );

						desiredSize.Width += commonChildSize.Width * LastItemMargin;
					}
					else {
						wrapCount = CalculateWrapCount( availableSize.Width, commonChildSize.Width );

						desiredSize.Width = commonChildSize.Width * wrapCount;
						desiredSize.Height = commonChildSize.Height * CalculateWraps( children.Count, wrapCount );

						desiredSize.Height += commonChildSize.Height * LastItemMargin;
					}

					// Run through all of the children to have their desired size calculated.
					foreach( UIElement child in children ) {
						child.Measure( childSize );
					}
				}

				// The available size represents a maximum constraint in the logical direction.  We
				// should never return a size larger than the available size.
				if( isHorizontal ) {
					desiredSize.Width = Math.Min( availableSize.Width, desiredSize.Width );
				}
				else {
					desiredSize.Height = Math.Min( availableSize.Height, desiredSize.Height );
				}

				mIsMeasured = true;
			}
			finally {
				mInMeasure = false;
			}

			return desiredSize;
		}

		protected override void OnVisualChildrenChanged( DependencyObject visualAdded, DependencyObject visualRemoved ) {
			// changes to the children collection require an update to the Offset 
			// so that the children don't shift when the collection changes

			// we do not want to process changes that occur as part of container generation
			if( mInMeasure || !mIsMeasured ) return;

			var childCount = InternalChildren.Count;

			if( visualAdded != null ) {
				double adjustedOffset = Offset % childCount;

				if( adjustedOffset < 0 ) {
					adjustedOffset += childCount;
				}
				
				var		newPivotalChildIndex = (int)adjustedOffset;

				if( newPivotalChildIndex != mPivotalChildIndex ) {
					// add necessary correction to keep Offset the same
					if( childCount > 1 ) {
						Offset += ((int)Offset ) / ( childCount - 1 ) + ( Offset < 0 ? -1 : 0 );
					}
				}
			}

			if( visualRemoved != null ) {
				// add necessary correction to keep Offset the same
				Offset -= ( (int)Offset ) / childCount + ( Offset < 0 ? -1 : 0 );
			}

			base.OnVisualChildrenChanged( visualAdded, visualRemoved );
		}

		private static void OnRequestBringIntoView( object sender, RequestBringIntoViewEventArgs e ) {
			var		lp = sender as LoopWrapPanel;
			var		target = e.TargetObject;

			if(( lp != null ) &&
			   ( lp.BringChildrenIntoView ) &&
			   ( target != lp )) {
				UIElement child = null;

				while( target != null ) {
					if( ( target is UIElement )
						&& lp.InternalChildren.Contains( target as UIElement )) {
						child = target as UIElement;
						break;
					}
					target = VisualTreeHelper.GetParent( target );
					if( target == lp ) break;
				}

				if(( child != null ) &&
				   ( lp.InternalChildren.Contains( child ))) {
					e.Handled = true;

					// determine if the child needs to be brought into view
					GeneralTransform childTransform = child.TransformToAncestor( lp );
					Rect childRect = childTransform.TransformBounds( new Rect( new Point( 0, 0 ), child.RenderSize ) );
					Rect intersection = Rect.Intersect( new Rect( new Point( 0, 0 ), lp.RenderSize ), childRect );

					// if the intersection is different than the child rect, it is either not visible 
					// or only partially visible, so adjust the Offset to bring it into view
					if( !DoubleHelper.AreVirtuallyEqual( childRect, intersection ) ) {
						if( !intersection.IsEmpty ) {
							// the child is already partially visible, so just scroll it into view
							lp.Scroll( ( lp.Orientation == Orientation.Horizontal )
								? ( DoubleHelper.AreVirtuallyEqual( childRect.X, intersection.X ) ? childRect.Width - intersection.Width + Math.Min( 0, lp.RenderSize.Width - childRect.Width ) : childRect.X - intersection.X )
								: ( DoubleHelper.AreVirtuallyEqual( childRect.Y, intersection.Y ) ? childRect.Height - intersection.Height + Math.Min( 0, lp.RenderSize.Height - childRect.Height ) : childRect.Y - intersection.Y ) );
						}
						else {
							// the child is not visible at all
							lp.Scroll( ( lp.Orientation == Orientation.Horizontal )
								? ( DoubleHelper.StrictlyLessThan( childRect.Right, 0.0d ) ? childRect.X : childRect.Right - lp.RenderSize.Width + Math.Min( 0, lp.RenderSize.Width - childRect.Width ) )
								: ( DoubleHelper.StrictlyLessThan( childRect.Bottom, 0.0d ) ? childRect.Y : childRect.Bottom - lp.RenderSize.Height + Math.Min( 0, lp.RenderSize.Height - childRect.Height ) ) );
						}
					}
				}
			}
		}
	}
}
