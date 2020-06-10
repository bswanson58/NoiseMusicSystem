using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Noise.UI.Behaviours {
    public class DataGridItemVisible : Behavior<DataGrid> {
        //<ListBox ... >
        //<i:Interaction.Behaviors>
        //    <Behaviours:DataGridItemVisible Item="{Binding propertyName} />
        //</i:Interaction.Behaviors>
        //</ListBox>
        private DataGrid	mItemsControl;

        protected override void OnAttached() {
            base.OnAttached();

            mItemsControl = AssociatedObject;
        }

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(
            "Item",
            typeof( object ),
            typeof( DataGridItemVisible ),
            new PropertyMetadata( false, OnItemChanged ));

        public object Item {
            get => GetValue( ItemProperty );
            set => SetValue( ItemProperty, value );
        }

        private static void OnItemChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e ) {
            if(( e.NewValue != null ) &&
               ( sender is DataGridItemVisible behavior )) {
                var dataGrid = behavior.mItemsControl;

                dataGrid.Dispatcher?.BeginInvoke((Action)( () => {
                    dataGrid.UpdateLayout();

                    dataGrid.ScrollIntoView( e.NewValue );
                }));
            }
        }
    }
}
