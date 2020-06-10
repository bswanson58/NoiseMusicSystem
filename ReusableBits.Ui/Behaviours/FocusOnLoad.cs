using System.Linq;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using ReusableBits.Ui.Utility;

namespace ReusableBits.Ui.Behaviours {
    //<i:Interaction.Behaviors>
    //    <behaviors:FocusOnLoad FocusElementTag="SomeTagName"/>
    //</i:Interaction.Behaviors>
    public class FocusOnLoad : Behavior<FrameworkElement> {
        public static readonly DependencyProperty FocusElementProperty =
            DependencyProperty.RegisterAttached( "FocusElementTag", typeof( string ), typeof( FocusOnLoad ), new PropertyMetadata());

        public string FocusElementTag {
            get => (string)GetValue( FocusElementProperty );
            set => SetValue( FocusElementProperty, value );
        }

        protected override void OnAttached() {
            base.OnAttached();

            AssociatedObject.Loaded += OnLoaded;
        }

        private void OnLoaded( object sender, RoutedEventArgs e ) {
            if( sender is FrameworkElement element ) {
                var elementName = FocusElementTag;
                var focusedElement = element.FindChildren<FrameworkElement>( el => el.Tag?.Equals( elementName ) == true ).FirstOrDefault();

                focusedElement?.Focus();
            }
        }

        protected override void OnDetaching() {
            base.OnDetaching();

            AssociatedObject.Loaded -= OnLoaded;
        }
    }
}
