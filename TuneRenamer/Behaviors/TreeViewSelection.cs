﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace TuneRenamer.Behaviors {
    //
    // usage:
    // <e:Interaction.Behaviors>
    //      <behaviors:TreeViewSelection SelectedItem="{Binding SelectedItem, Mode=TwoWay}" />
    // </e:Interaction.Behaviors>    
    //
    public class TreeViewSelection : Behavior<TreeView> {
        public object SelectedItem {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register( "SelectedItem", typeof( object ), typeof( TreeViewSelection ), new UIPropertyMetadata( null, OnSelectedItemChanged ));

        private static void OnSelectedItemChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e ) {
            if( e.NewValue is TreeViewItem item ) {
                item.SetValue( TreeViewItem.IsSelectedProperty, true );
            }
        }

        protected override void OnAttached() {
            base.OnAttached();

            AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching() {
            base.OnDetaching();

            if( AssociatedObject != null ) {
                AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            }
        }

        private void OnTreeViewSelectedItemChanged( object sender, RoutedPropertyChangedEventArgs<object> e ) {
            SelectedItem = e.NewValue;
        }
    }
}
