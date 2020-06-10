using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;
using ReusableBits.Mvvm.ViewModelSupport;
using ReusableBits.Ui.Utility;

namespace Noise.UI.Behaviours.ElementMover {
    public class MoveControlProperties : AutomaticPropertyBase {
        public string MoveText {
            get => Get( () => MoveText );
            set => Set( () => MoveText, value );
        }
    }

    public class ElementMoverBehavior : Behavior<FrameworkElement> {
        private readonly MoveControlProperties  mMoveControlProperties;

        public ElementMoverBehavior() {
            mMoveControlProperties = new MoveControlProperties();
        }

        public static readonly DependencyProperty StoryboardProperty = DependencyProperty.Register(
            "Storyboard", typeof( Storyboard ), typeof( ElementMoverBehavior ), new PropertyMetadata( null ));

        public Storyboard Storyboard {
            get => GetValue( StoryboardProperty ) as Storyboard;
            set => SetValue( StoryboardProperty, value );
        }

        public static readonly DependencyProperty LeftAnimationProperty = DependencyProperty.Register(
            "LeftAnimation", typeof( string ), typeof( ElementMoverBehavior ), new PropertyMetadata( "LeftAnimation" ));

        public string LeftAnimation {
            get => GetValue( LeftAnimationProperty ) as string;
            set => SetValue( LeftAnimationProperty, value );
        }

        public static readonly DependencyProperty TopAnimationProperty = DependencyProperty.Register(
            "TopAnimation", typeof( string ), typeof( ElementMoverBehavior ), new PropertyMetadata( "TopAnimation" ));

        public string TopAnimation {
            get => GetValue( TopAnimationProperty ) as string;
            set => SetValue( TopAnimationProperty, value );
        }

        public static readonly DependencyProperty MoveControlNameProperty = DependencyProperty.Register(
            "MoveControlName", typeof( string ), typeof( ElementMoverBehavior ), new PropertyMetadata( "MoveControl" ));

        public string MoveControlName {
            get => GetValue( MoveControlNameProperty ) as string;
            set => SetValue( MoveControlNameProperty, value );
        }

        public static readonly DependencyProperty MoveTargetTagProperty = DependencyProperty.Register(
            "MoveTargetTag", typeof( string ), typeof( ElementMoverBehavior ), new PropertyMetadata( "MoveTarget" ));

        public string MoveTargetTag {
            get => GetValue( MoveTargetTagProperty ) as string;
            set => SetValue( MoveTargetTagProperty, value );
        }

        protected override void OnAttached() {
            base.OnAttached();

            AssociatedObject.AddHandler( ElementMoverSource.MoveElementEvent, new RoutedEventHandler( OnMoveElement ));
        }

        protected override void OnDetaching() {
            AssociatedObject.RemoveHandler( ElementMoverSource.MoveElementEvent, new RoutedEventHandler( OnMoveElement ));

            base.OnDetaching();
        }

        private void OnMoveElement( object sender, RoutedEventArgs args ) {
            if( args is MoveElementEventArgs moveArgs ) {
                if(( SetStartPosition( moveArgs )) &&
                   ( SetTargetPosition()) &&
                   ( SetStyle( moveArgs ))) {
                    Storyboard.Begin();
                }
            }
        }

        private bool SetStyle( MoveElementEventArgs args ) {
            var retValue = false;
            var content = AssociatedObject.FindChildren<FrameworkElement>().FirstOrDefault( c => c.Name != null && c.Name.Equals( MoveControlName ));

            if( content is ContentControl control ) {
                mMoveControlProperties.MoveText = args.DisplayText;

                control.Content = mMoveControlProperties;
                retValue = true;
            }

            return retValue;
        }

        private bool SetStartPosition( MoveElementEventArgs  args ) {
            var retValue = false;

            if( args.SourceElement != null ) {
                var leftAnimation = Storyboard.Children.FirstOrDefault( c => c.Name.Equals( LeftAnimation ));
                var topAnimation = Storyboard.Children.FirstOrDefault( c => c.Name.Equals( TopAnimation ));
                var relativePoint = args.SourceElement.TransformToAncestor( AssociatedObject ).Transform( new Point( 0, 0 ));

                if( leftAnimation is DoubleAnimation left ) {
                    left.From = relativePoint.X;

                    retValue = true;
                }

                if( topAnimation is DoubleAnimation top ) {
                    top.From = relativePoint.Y;

                    retValue = true;
                }
            }

            return retValue;
        }

        private bool SetTargetPosition() {
            var retValue = false;
            var target = AssociatedObject.FindChildren<FrameworkElement>().FirstOrDefault( c => c.Tag != null && c.Tag.Equals( MoveTargetTag ));

            if( target != null ) {
                var leftAnimation = Storyboard.Children.FirstOrDefault( c => c.Name.Equals( LeftAnimation ));
                var topAnimation = Storyboard.Children.FirstOrDefault( c => c.Name.Equals( TopAnimation ));
                var relativePoint = target.TransformToAncestor( AssociatedObject ).Transform( new Point( 0, 0 ));
                var offset = target.GetValue( ElementMoverTarget.TargetLocationProperty );

                if( leftAnimation is DoubleAnimation left ) {
                    if( offset is Point point ) {
                        left.To = relativePoint.X + point.X;
                    }
                    else {
                        left.To = relativePoint.X;
                    }

                    retValue = true;
                }

                if( topAnimation is DoubleAnimation top ) {
                    if( offset is Point point ) {
                        top.To = relativePoint.Y + point.Y;
                    }
                    else {
                        top.To = relativePoint.Y;
                    }

                    retValue = true;
                }
            }

            return retValue;
        }
    }
}
