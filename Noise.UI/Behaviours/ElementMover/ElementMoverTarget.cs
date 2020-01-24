using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Noise.UI.Behaviours.ElementMover {
    public class ElementMoverTarget : Behavior<FrameworkElement> {
        public static DependencyProperty TargetLocationProperty = 
            DependencyProperty.RegisterAttached( "TargetLocation", typeof( Point ), typeof( ElementMoverTarget ));

        public static Point GetLocation( DependencyObject dependencyObject ) {
            return (Point)dependencyObject.GetValue( TargetLocationProperty );
        }
        public static void SetLocation( DependencyObject dependencyObject, Point value ) {
            dependencyObject.SetValue( TargetLocationProperty, value );
        }

        protected override void OnAttached() {
            base.OnAttached();

            AssociatedObject.Loaded += OnLoaded;
        }

        private void OnLoaded( object sender, RoutedEventArgs args ) {
//            UpdateTarget( CalculateTarget());

            AssociatedObject.Loaded -= OnLoaded;
        }

        protected virtual Point CalculateTarget() {
            return new Point( AssociatedObject.ActualWidth / 2.0, AssociatedObject.ActualHeight / 2.0 );
        }

        protected void UpdateTarget( Point target ) {
            SetLocation( AssociatedObject, target );
        }
    }

    public class ElementMoverListTarget : ElementMoverTarget {
        protected override void OnAttached() {
            base.OnAttached();

            if( AssociatedObject is ItemsControl itemsControl ) {
                itemsControl.ItemContainerGenerator.StatusChanged += OnCollectionChanged;
            }
        }

        protected override void OnDetaching() {
            if( AssociatedObject is ItemsControl itemsControl ) {
                itemsControl.ItemContainerGenerator.StatusChanged -= OnCollectionChanged;
            }

            base.OnDetaching();
        }

        private void OnCollectionChanged( Object sender, EventArgs e ) {
            if( AssociatedObject is ItemsControl itemsControl ) {
                if( itemsControl.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated ) {
                    UpdateTarget( CalculateTarget());
                }
            }
        }

        protected override Point CalculateTarget() {
            var retValue = base.CalculateTarget();

            if( AssociatedObject is ListBox listBox ) {
                if( listBox.Items.Count > 0 ) {
                    var listBoxItem = GetListBoxItem( listBox, listBox.Items.Count - 1 );

                    if( listBoxItem != null ) {
                        var relativePoint = listBoxItem.TransformToAncestor( AssociatedObject ).Transform( new Point( 0, 0 ));
                        var verticalPlacement = relativePoint.Y > listBox.ActualHeight ? listBox.ActualHeight : relativePoint.Y + listBoxItem.ActualHeight;

                        retValue = new Point( relativePoint.X + 10, verticalPlacement );
                    }
                }
            }

            return retValue;
        }

        static ListBoxItem GetListBoxItem( ListBox listView, int index ) {
            var retValue = default( ListBoxItem );

            if(( index >= 0 ) &&
               ( listView.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated )) {
                retValue = listView.ItemContainerGenerator.ContainerFromIndex( index ) as ListBoxItem;
            }

            return retValue;
        }
    }
}
