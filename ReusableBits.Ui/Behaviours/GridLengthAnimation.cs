using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace ReusableBits.Ui.Behaviours {
	// from: http://stackoverflow.com/questions/197855/in-wpf-has-anybody-animated-a-grid

	public class GridLengthAnimation : AnimationTimeline {
		public static readonly DependencyProperty FromProperty = DependencyProperty.Register( "From", typeof( GridLength ), typeof( GridLengthAnimation ) );
		public GridLength From {
			get { return (GridLength)GetValue( FromProperty ); }
			set { SetValue( FromProperty, value ); }
		}

		public static readonly DependencyProperty ToProperty = DependencyProperty.Register( "To", typeof( GridLength ), typeof( GridLengthAnimation ) );
		public GridLength To {
			get { return (GridLength)GetValue( ToProperty ); }
			set { SetValue( ToProperty, value ); }
		}

		public override Type TargetPropertyType {
			get { return typeof( GridLength ); }
		}

		protected override Freezable CreateInstanceCore() {
			return new GridLengthAnimation();
		}

		public override object GetCurrentValue( object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock ) {
			double fromValue = From.Value;
			double toValue = To.Value;

			if( defaultOriginValue is GridLength ) {
				fromValue = ((GridLength)defaultOriginValue ).Value;
			}

			var retValue = fromValue > toValue ?
				new GridLength(( 1 - animationClock.CurrentProgress.Value ) * ( fromValue - toValue ) + toValue, To.IsStar ? GridUnitType.Star : GridUnitType.Pixel ) :
				new GridLength(( animationClock.CurrentProgress.Value ) * ( toValue - fromValue ) + fromValue, To.IsStar ? GridUnitType.Star : GridUnitType.Pixel );

			return( retValue );
		}
	}
}
