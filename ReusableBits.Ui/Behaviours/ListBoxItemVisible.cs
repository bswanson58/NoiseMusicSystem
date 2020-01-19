using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ReusableBits.Ui.Behaviours {
	public class ListBoxItemVisible : Behavior<ListBox> {
		//<ListBox ... >
		//<i:Interaction.Behaviors>
		//    <Behaviours:ListBoxItemVisible Item="{Binding propertyName} />
		//</i:Interaction.Behaviors>
		//</ListBox>
		private ListBox	mItemsControl;

		protected override void OnAttached() {
			base.OnAttached();

			mItemsControl = AssociatedObject;
		}

		public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(
			"Item",
			typeof( object ),
			typeof( ListBoxItemVisible ),
			new PropertyMetadata( false, OnItemChanged ));

		public object Item {
			get => GetValue( ItemProperty );
            set => SetValue( ItemProperty, value );
        }

		private static void OnItemChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e ) {
			if(( e.NewValue != null ) &&
			   ( sender is ListBoxItemVisible behavior )) {
                var listBox = behavior.mItemsControl;

                listBox.Dispatcher?.BeginInvoke((Action)( () => {
                        listBox.UpdateLayout();

                        listBox.ScrollIntoView( e.NewValue );
                    }));
            }
		}
	}
}
