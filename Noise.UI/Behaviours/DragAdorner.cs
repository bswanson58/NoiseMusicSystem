// Copyright (C) Josh Smith - January 2007
using System.Windows.Documents;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Noise.UI.Behaviours {
	/// <summary>
	/// Renders a visual which can follow the mouse cursor, 
	/// such as during a drag-and-drop operation.
	/// </summary>
	public class DragAdorner : Adorner {
		private readonly Rectangle	mChild;
		private double				mOffsetLeft;
		private double				mOffsetTop;

		/// <summary>
		/// Initializes a new instance of DragVisualAdorner.
		/// </summary>
		/// <param name="adornedElement">The element being adorned.</param>
		/// <param name="size">The size of the adorner.</param>
		/// <param name="brush">A brush to with which to paint the adorner.</param>
		public DragAdorner( UIElement adornedElement, Size size, Brush brush )
			: base( adornedElement ) {
			var rect = new Rectangle();

			rect.Fill = brush;
			rect.Width = size.Width;
			rect.Height = size.Height;
			rect.IsHitTestVisible = false;
			mChild = rect;
		}

		/// <summary>
		/// Override.
		/// </summary>
		/// <param name="transform"></param>
		/// <returns></returns>
		public override GeneralTransform GetDesiredTransform( GeneralTransform transform ) {
			var result = new GeneralTransformGroup();

			result.Children.Add( base.GetDesiredTransform( transform ) );
			result.Children.Add( new TranslateTransform( mOffsetLeft, mOffsetTop ) );
			return result;
		}

		/// <summary>
		/// Gets/sets the horizontal offset of the adorner.
		/// </summary>
		public double OffsetLeft {
			get { return mOffsetLeft; }
			set {
				mOffsetLeft = value;
				UpdateLocation();
			}
		}

		/// <summary>
		/// Updates the location of the adorner in one atomic operation.
		/// </summary>
		/// <param name="left"></param>
		/// <param name="top"></param>
		public void SetOffsets( double left, double top ) {
			mOffsetLeft = left;
			mOffsetTop = top;
			UpdateLocation();
		}

		/// <summary>
		/// Gets/sets the vertical offset of the adorner.
		/// </summary>
		public double OffsetTop {
			get { return mOffsetTop; }
			set {
				mOffsetTop = value;
				UpdateLocation();
			}
		}

		/// <summary>
		/// Override.
		/// </summary>
		/// <param name="constraint"></param>
		/// <returns></returns>
		protected override Size MeasureOverride( Size constraint ) {
			mChild.Measure( constraint );
			return mChild.DesiredSize;
		}

		/// <summary>
		/// Override.
		/// </summary>
		/// <param name="finalSize"></param>
		/// <returns></returns>
		protected override Size ArrangeOverride( Size finalSize ) {
			mChild.Arrange( new Rect( finalSize ) );
			return finalSize;
		}

		/// <summary>
		/// Override.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		protected override Visual GetVisualChild( int index ) {
			return mChild;
		}

		/// <summary>
		/// Override.  Always returns 1.
		/// </summary>
		protected override int VisualChildrenCount {
			get { return 1; }
		}

		private void UpdateLocation() {
			var adornerLayer = Parent as AdornerLayer;

			if( adornerLayer != null ) {
				adornerLayer.Update( AdornedElement );
			}
		}
	}
}