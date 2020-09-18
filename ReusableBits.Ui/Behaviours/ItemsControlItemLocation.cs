using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using ReusableBits.Ui.Utility;

namespace ReusableBits.Ui.Behaviours {
    // Use this behavior to bind the location of an ItemsControlItem container with an ItemsControl relative to some parent element.
    // usage:
    //
    //<ItemsControl ... >
    //<i:Interaction.Behaviors>
    //    <Behaviours:ItemsControlItemLocation
    //				ItemLocation="{Binding Point}"
    //				RootElementTag="_tagName"
    //				ItemsControl="{Binding RelativeSource = {RelativeSource FindAncestor, AncestorType={x:Type ItemsControl}}}" />
    //</i:Interaction.Behaviors>
    //</ItemsControl>
    public class ItemsControlItemLocation : Behavior<FrameworkElement> {
        private FrameworkElement    mRootElement;

        public static readonly DependencyProperty ItemLocationProperty = DependencyProperty.Register(
            "ItemLocation",
            typeof( Point ),
            typeof( ItemsControlItemLocation ),
            new PropertyMetadata( null ) );

        public Point ItemLocation {
            get => (Point)GetValue( ItemLocationProperty );
            set => SetValue( ItemLocationProperty, value );
        }

        public static readonly DependencyProperty ItemsControlProperty = DependencyProperty.Register(
            "ItemsControl",
            typeof( ItemsControl ),
            typeof( ItemsControlItemLocation ),
            new PropertyMetadata( null ) );

        public ItemsControl ItemsControl {
            get => GetValue( ItemsControlProperty ) as ItemsControl;
            set => SetValue( ItemsControlProperty, value );
        }

        public static readonly DependencyProperty RootElementTagProperty = DependencyProperty.Register(
            "RootElementTag",
            typeof( String ),
            typeof( ItemsControlItemLocation ),
            new PropertyMetadata( null ) );

        public string RootElementTag {
            get => (String)GetValue( RootElementTagProperty );
            set => SetValue( RootElementTagProperty, value );
        }

        protected override void OnAttached() {
            base.OnAttached();

            if( AssociatedObject != null ) {
                AssociatedObject.LayoutUpdated += OnLayoutChanged;

                mRootElement = Application.Current.MainWindow.FindChildren<FrameworkElement>().FirstOrDefault( e => e.Tag?.Equals( RootElementTag ) == true );
            }
        }

        protected override void OnDetaching() {
            if( AssociatedObject != null ) {
                AssociatedObject.LayoutUpdated -= OnLayoutChanged;
            }

            base.OnDetaching();
        }

        private void OnLayoutChanged( object sender, EventArgs args ) {
            if(( ItemsControl != null ) &&
               ( mRootElement != null )) {
                if( ItemsControl.ItemContainerGenerator.ContainerFromItem( AssociatedObject.DataContext ) is FrameworkElement container ) {
                    ItemLocation = container.TransformToAncestor( mRootElement ).Transform(new Point( 0, 0 ));
                }
            }
        }
    }
}
