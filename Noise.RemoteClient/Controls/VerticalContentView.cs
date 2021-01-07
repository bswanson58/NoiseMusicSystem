using Xamarin.Forms;

//from: https://stackoverflow.com/questions/42972823/vertical-slider-in-xamarin-forms

namespace Noise.RemoteClient.Controls {
    [ContentProperty( "VerticalContent" )]
    public class VerticalContentView : ContentView {
        public View VerticalContent {
            get => (View)GetValue( ContentProperty );
            set => SetValue( ContentProperty, Verticalize( value ));
        }

        public double ContentRotation { get; set; } = -90;

        private View Verticalize( View toBeRotated ) {
            if( toBeRotated == null )
                return null;

            toBeRotated.Rotation = ContentRotation;
            var result = new RelativeLayout();

            result.Children.Add( toBeRotated,
                xConstraint: Constraint.RelativeToParent( parent => parent.X - (( parent.Height - parent.Width ) / 2 )),
                yConstraint: Constraint.RelativeToParent( parent => ( parent.Height / 2 ) - ( parent.Width / 2 )),
                widthConstraint: Constraint.RelativeToParent( parent => parent.Height ),
                heightConstraint: Constraint.RelativeToParent( parent => parent.Width ));

            return result;
        }
    }
}
