using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

// from: https://stackoverflow.com/questions/2088909/inline-editing-textblock-in-a-listbox-with-data-template-wpf

namespace Noise.UI.Controls {
    /// <summary>
    /// Interaction logic for EditableTextBlock.xaml
    /// usage: <Controls:EditableTextBlock Text="{Binding Path=Name, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
    /// </summary>
    public partial class EditableTextBlock {
        public EditableTextBlock() {
            InitializeComponent();
        }

        public string Text {
            get => (string)GetValue( TextProperty );
            set => SetValue( TextProperty, value );
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register( "Text", typeof( string ), typeof( EditableTextBlock ), new UIPropertyMetadata());

        private void TextBox_LostFocus( object sender, RoutedEventArgs e ) {
            if( sender is TextBox textBox ) {
                if( textBox.Parent is Grid grid ) {
                    foreach( var c in grid.Children ) {
                        if( c is TextBlock textBlock ) {
                            textBlock.Visibility = Visibility.Visible;
                            textBox.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void TextBlock_MouseDown( object sender, MouseButtonEventArgs e ) {
            if( sender is TextBlock textBlock ) {
                if( textBlock.Parent is Grid grid ) {
                    foreach( var c in grid.Children ) {
                        if( c is TextBox textBox ) {
                            textBox.Visibility = Visibility.Visible;
                            textBlock.Visibility = Visibility.Collapsed;

                            Dispatcher?.BeginInvoke( (Action)(() => Keyboard.Focus( textBox )), DispatcherPriority.Render );
                        }
                    }
                }
            }
        }

        private void TextBox_KeyDown( object sender, KeyEventArgs e ) {
            if(( e != null ) &&
               ( e.Key == Key.Return )) {
                TextBox_LostFocus( sender, null );
            }
        }
    }
}
