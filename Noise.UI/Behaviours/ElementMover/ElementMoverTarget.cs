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
        public static readonly DependencyProperty NextInsertIndexProperty = DependencyProperty.Register(
            "NextInsertIndex", typeof( int ), typeof( ElementMoverListTarget ), new PropertyMetadata( -1 ));

        public int NextInsertIndex {
            get => (int)GetValue( NextInsertIndexProperty );
            set => SetValue( NextInsertIndexProperty, value );
        }

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
                    // account for the track we're queueing has already been added to the collection.
                    var insertIndex = Math.Min( NextInsertIndex > 0 ? NextInsertIndex - 1 : listBox.Items.Count - 1, listBox.Items.Count - 1 );
                    var listBoxItem = GetListBoxItem( listBox, insertIndex );

                    if( listBoxItem != null ) {
                        var relativePoint = listBoxItem.TransformToAncestor( AssociatedObject ).Transform( new Point( 0, 0 ));
                        var verticalPlacement = relativePoint.Y > listBox.ActualHeight ? listBox.ActualHeight : relativePoint.Y + listBoxItem.ActualHeight;

                        retValue = new Point( relativePoint.X + 10, verticalPlacement );
                    }
                }
                else {
                    retValue = new Point( 10, 10 );
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
