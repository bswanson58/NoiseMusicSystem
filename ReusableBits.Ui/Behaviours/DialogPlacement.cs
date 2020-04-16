using System;
using System.Linq;
using System.Windows;
using System.Windows.Interactivity;
using ReusableBits.Ui.Utility;

namespace ReusableBits.Ui.Behaviours {
    // Usage:
    //    <i:Interaction.Behaviors>
    //      <behaviors:DialogPlacement AssociatedControlTag="control_relative_to" HorizontalPlacement="AlignLeft" VerticalPlacement="Above" VerticalOffset="-25"/>
    //    </i:Interaction.Behaviors>
    //

    public enum DialogPlacementVertical {
        Above,
        AlignTop,
        Center,
        AlignBottom,
        Below
    }

    public enum DialogPlacementHorizontal {
        ToLeft,
        AlignLeft,
        Center,
        AlignRight,
        ToRight, 
    }

    public class DialogPlacement : Behavior<FrameworkElement> {
        public static readonly DependencyProperty AssociatedControlProperty = DependencyProperty.Register(
            "AssociatedControlTag", typeof( string ), typeof( DialogPlacement ), new PropertyMetadata( String.Empty ));

        public string AssociatedControlTag {
            get => (string)GetValue( AssociatedControlProperty );
            set => SetValue( AssociatedControlProperty, value );
        }

        public static readonly DependencyProperty HorizontalPlacementProperty = DependencyProperty.Register(
            "HorizontalPlacement", typeof( DialogPlacementHorizontal ), typeof( DialogPlacement ), new PropertyMetadata( DialogPlacementHorizontal.Center ));

        public DialogPlacementHorizontal HorizontalPlacement {
            get => (DialogPlacementHorizontal)GetValue( HorizontalPlacementProperty );
            set => SetValue( HorizontalPlacementProperty, value );
        }

        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register(
            "HorizontalOffset", typeof( double ), typeof( DialogPlacement ), new PropertyMetadata( 0.0 ));

        public double HorizontalOffset {
            get => (double)GetValue( HorizontalOffsetProperty );
            set => SetValue( HorizontalOffsetProperty, value );
        }

        public static readonly DependencyProperty VerticalPlacementProperty = DependencyProperty.Register(
            "VerticalPlacement", typeof( DialogPlacementVertical ), typeof( DialogPlacement ), new PropertyMetadata( DialogPlacementVertical.Center ));

        public DialogPlacementVertical VerticalPlacement {
            get => (DialogPlacementVertical)GetValue( VerticalPlacementProperty );
            set => SetValue( VerticalPlacementProperty, value );
        }

        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register(
            "VerticalOffset", typeof( double ), typeof( DialogPlacement ), new PropertyMetadata( 0.0 ));

        public double VerticalOffset {
            get => (double)GetValue( VerticalOffsetProperty );
            set => SetValue( VerticalOffsetProperty, value );
        }

        protected override void OnAttached() {
            base.OnAttached();

            AssociatedObject.Loaded += OnLoaded;
        }

        private Window GetParentWindow() {
            return Window.GetWindow( AssociatedObject );
        }

        private void OnLoaded( object sender, RoutedEventArgs args ) {
            if( Application.Current.MainWindow != null ) {
                var dialog = GetParentWindow();
                var associatedControl = Application.Current.MainWindow.FindChildren<FrameworkElement>( c => AssociatedControlTag.Equals( c?.Tag )).FirstOrDefault();
                
                if(( dialog != null ) &&
                   ( associatedControl != null )) {
                    var mainWindowPosition = Application.Current.MainWindow.WindowState == WindowState.Maximized ? 
                                                    Application.Current.MainWindow.PointToScreen( new Point( 0, 0 )) :
                                                    new Point( Application.Current.MainWindow.Left, Application.Current.MainWindow.Top );
                    var associatedLocation = associatedControl.TransformToVisual( Application.Current.MainWindow )
                                                    .TransformBounds( new Rect( mainWindowPosition,
                                                                      new Size( associatedControl.ActualWidth, associatedControl.ActualHeight )));

                    switch( VerticalPlacement ) {
                        case DialogPlacementVertical.Above:
                            dialog.Top = associatedLocation.Top - dialog.ActualHeight;
                            break;

                        case DialogPlacementVertical.AlignBottom:
                            dialog.Top = associatedLocation.Bottom - dialog.ActualHeight;
                            break;

                        case DialogPlacementVertical.AlignTop:
                            dialog.Top = associatedLocation.Top;
                            break;

                        case DialogPlacementVertical.Below:
                            dialog.Top = associatedLocation.Bottom;
                            break;

                        case DialogPlacementVertical.Center:
                            dialog.Top = associatedLocation.Top + ( associatedControl.ActualHeight / 2.0 ) - ( dialog.ActualHeight / 2.0 );
                            break;
                    }

                    dialog.Top += VerticalOffset;

                    switch( HorizontalPlacement ) {
                        case DialogPlacementHorizontal.AlignLeft:
                            dialog.Left = associatedLocation.Left;
                            break;

                        case DialogPlacementHorizontal.AlignRight:
                            dialog.Left = associatedLocation.Right - dialog.ActualWidth;
                            break;

                        case DialogPlacementHorizontal.Center:
                            dialog.Left = associatedLocation.Left + ( associatedControl.ActualWidth / 2.0 ) - ( dialog.ActualWidth / 2.0 );
                            break;

                        case DialogPlacementHorizontal.ToLeft:
                            dialog.Left = associatedLocation.Left - dialog.ActualWidth;
                            break;

                        case DialogPlacementHorizontal.ToRight:
                            dialog.Left = associatedLocation.Right;
                            break;
                    }

                    dialog.Left += HorizontalOffset;
                }
            }

            AssociatedObject.Loaded -= OnLoaded;
        }
    }
}
