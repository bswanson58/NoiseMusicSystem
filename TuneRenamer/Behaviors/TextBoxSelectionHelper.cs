using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace TuneRenamer.Behaviors {
    // usage:
    //
    //<TextBox ... >
    //<i:Interaction.Behaviors>
    //    <Behaviors:TextBoxSelectionHelper TextSelection={Binding VmProperty}" />
    //</i:Interaction.Behaviors>
    //</ItemsControl>

    public class TextBoxSelectionHelper : Behavior<TextBox> {
        public static readonly DependencyProperty TextSelectionProperty =  
            DependencyProperty.Register( "TextSelection",
                typeof( string ), typeof( TextBoxSelectionHelper ),  
                new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedTextChanged ));

        public string TextSelection {
            get => GetValue( TextSelectionProperty ) as string;
            set => SetValue( TextSelectionProperty, value );
        }

        private static void OnSelectedTextChanged( DependencyObject d, DependencyPropertyChangedEventArgs e ) {
            if( d is TextBoxSelectionHelper helper ) {
                if(( e.NewValue is string newValue ) && 
                   ( newValue != helper.TextSelection )) {
                    helper.TextSelection = newValue;
                }
            }
        }

        protected override void OnAttached() {
            base.OnAttached();

            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching() {
            base.OnDetaching();

            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged( object sender, RoutedEventArgs args ) {
            TextSelection = AssociatedObject.SelectedText;
        }
    }
}
