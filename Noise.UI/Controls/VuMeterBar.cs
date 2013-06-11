using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CuttingEdge.Conditions;
using Condition = CuttingEdge.Conditions.Condition;

namespace Noise.UI.Controls {
	public class VuMeterBar : RangeBase {
		static VuMeterBar() {
			MinHeightProperty.OverrideMetadata( typeof( VuMeterBar ), new FrameworkPropertyMetadata((double)1 ));
			MinWidthProperty.OverrideMetadata( typeof( VuMeterBar ), new FrameworkPropertyMetadata((double)10 ));
			ClipToBoundsProperty.OverrideMetadata( typeof( VuMeterBar ), new FrameworkPropertyMetadata( true ));
			ValueProperty.OverrideMetadata( typeof( VuMeterBar ), new FrameworkPropertyMetadata( 0.0, FrameworkPropertyMetadataOptions.AffectsRender, null ));
		}

		public static readonly DependencyProperty BlockMarginProperty =
			DependencyProperty.Register( "BlockMargin", typeof( double ), typeof( VuMeterBar ),
			new FrameworkPropertyMetadata( 1.0, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceBlockMargin ));

		public double BlockMargin {
			get { return (double)GetValue(BlockMarginProperty); }
			set { SetValue(BlockMarginProperty, value); }
		}

		public static readonly DependencyProperty BlockSizeProperty =
			DependencyProperty.Register( "BlockSize", typeof( double ), typeof( VuMeterBar ),
			new FrameworkPropertyMetadata( 3.0, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceBlockSize ));

		public double BlockSize {
			get { return (double)GetValue( BlockSizeProperty ); }
			set { SetValue( BlockSizeProperty, value ); }
		}

		public static readonly DependencyProperty PeakThresholdProperty =
			DependencyProperty.Register( "PeakThreshold", typeof( double ), typeof( VuMeterBar ),
			new FrameworkPropertyMetadata( 1.0, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceThreshold ));

		public double PeakThreshold {
			get { return (double)GetValue( PeakThresholdProperty ); }
			set { SetValue( PeakThresholdProperty, value ); }
		}

		public static readonly DependencyProperty PeakColorProperty =
			DependencyProperty.Register( "PeakColor", typeof( Brush ), typeof( VuMeterBar ),
			new FrameworkPropertyMetadata( Brushes.Red, FrameworkPropertyMetadataOptions.AffectsRender, null ));

		public Brush PeakColor {
			get { return (Brush)GetValue( PeakColorProperty ); }
			set { SetValue( PeakColorProperty, value ); }
		}

		private static int GetThreshold( double value, int blockCount ) {
			Condition.Requires( blockCount ).IsGreaterThan( 0 );
			Condition.Requires( value ).IsGreaterOrEqual( 0.0 );
			Condition.Requires( value ).IsLessOrEqual( 1.0 );

			int blockNumber = Math.Min((int)(value * (blockCount + 1)), blockCount);

			Condition.Ensures( blockNumber ).IsLessOrEqual( blockCount );
			Condition.Ensures( blockNumber ).IsGreaterOrEqual( 0 );

			return( blockNumber );
		}

		private static object CoerceBlockSize( DependencyObject element, object value ) {
			var input = (double)value;

			if( input < 1.0 ) {
				return 1.0;
			}

			return input;
		}

		private static object CoerceBlockMargin( DependencyObject element, object value ) {
			var input = (double)value;

			if(( input < 0 ) || 
			   ( double.IsNaN( input )) ||
			   ( double.IsInfinity( input ))) {
				return( 0.0 );
			}

			return( input );
		}

		private static object CoerceThreshold( DependencyObject elment, object value ) {
			var input = (double)value;

			if(( double.IsNaN( input )) ||
			   ( double.IsInfinity( input ))) {
				return( 0.0 );
			}

			return( input );
		}

		protected override void OnRender( DrawingContext drawingContext ) {
			var	blockCount = (int)( RenderSize.Width / ( BlockSize + BlockMargin ));

			for (int i = 0; i < blockCount; i++) {
				var	rect = GetRect( RenderSize, blockCount, BlockMargin, i );

				if (!rect.IsEmpty) {
					var	threshold = GetThreshold( Value, blockCount );
					var peak = GetThreshold( PeakThreshold, blockCount );
					var color = i < threshold ? ( i < peak ? Foreground : PeakColor ) : Background;

					drawingContext.DrawRectangle( color, null, rect );
				}
			}
		}

		private static Rect GetRect( Size targetSize, int blockCount, double blockMargin, int blockNumber ) {
			Condition.Requires( targetSize.Height ).IsGreaterThan( 0 );
			Condition.Requires( targetSize.Width ).IsGreaterThan( 0 );
			Condition.Requires( blockCount ).IsGreaterThan( 0 );
			Condition.Requires( blockCount ).IsGreaterThan( blockNumber );

			var retValue = Rect.Empty;

			double width = ( targetSize.Width - ( blockCount - 1 ) * blockMargin ) / blockCount;
			double left = ( width + blockMargin ) * blockNumber;
			double height = targetSize.Height;

			if(( width > 0 ) &&
				( height > 0 )) {
				retValue = new Rect( left, 0, width, height );
			}

			return( retValue );
		}
	}
}
