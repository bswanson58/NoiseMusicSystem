using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media.Animation;

// from: https://en.it1352.com/article/e52051208b7346019e8ce362e5637475.html
//
// usage: (do not use ListBox:ItemsSource)
//    <ListBox Behaviors:ItemsControlAnimation.ItemsSource="{Binding MyCollection}">
//      <Behaviors:ItemsControlAnimation.FadeInAnimation>
//          <Storyboard>
//              <DoubleAnimation Storyboard.TargetProperty="Opacity" From="0.0" To="1.0" Duration="0:0:3"/>
//          </Storyboard>
//      </Behaviors:ItemsControlAnimation.FadeInAnimation>
//      <Behaviors:ItemsControlAnimation.FadeOutAnimation>
//          <Storyboard>
//              <DoubleAnimation Storyboard.TargetProperty="Opacity" To="0.0" Duration="0:0:1"/>
//          </Storyboard>
//      </Behaviors:ItemsControlAnimation.FadeOutAnimation>
//    </ListBox>

namespace Noise.UI.Behaviours {
    public class ItemsControlAnimation {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.RegisterAttached( "ItemsSource", typeof( IList ), typeof( ItemsControlAnimation ), new UIPropertyMetadata( null, ItemsSourcePropertyChanged ));

        public static void SetItemsSource( DependencyObject element, IList value ) {
            element.SetValue( ItemsSourceProperty, value );
        }
        public static IList GetItemsSource( DependencyObject element ) {
            return (IList)element.GetValue( ItemsSourceProperty );
        }

        public static readonly DependencyProperty FadeInAnimationProperty =
            DependencyProperty.RegisterAttached( "FadeInAnimation", typeof( Storyboard ), typeof( ItemsControlAnimation ), new UIPropertyMetadata( null ));

        public static void SetFadeInAnimation( DependencyObject element, Storyboard value ) {
            element.SetValue( FadeInAnimationProperty, value );
        }
        public static Storyboard GetFadeInAnimation( DependencyObject element ) {
            return (Storyboard)element.GetValue( FadeInAnimationProperty );
        }

        public static readonly DependencyProperty FadeOutAnimationProperty =
            DependencyProperty.RegisterAttached( "FadeOutAnimation", typeof( Storyboard ), typeof( ItemsControlAnimation ), new UIPropertyMetadata( null ));

        public static void SetFadeOutAnimation( DependencyObject element, Storyboard value ) {
            element.SetValue( FadeOutAnimationProperty, value );
        }
        public static Storyboard GetFadeOutAnimation( DependencyObject element ) {
            return (Storyboard)element.GetValue( FadeOutAnimationProperty );
        }

        private static void ItemsSourcePropertyChanged( DependencyObject source, DependencyPropertyChangedEventArgs e ) {
            var itemsControl = source as ItemsControl;
            var itemsSource = e.NewValue as IList;

            if ( itemsControl == null ) {
                return;
            }
            if ( itemsSource == null ) {
                itemsControl.ItemsSource = null;
                return;
            }

            var itemsSourceType = itemsSource.GetType();
            var listType = typeof(ObservableCollection<>).MakeGenericType( itemsSourceType.GetGenericArguments()[0]);
            var mirrorItemsSource = Activator.CreateInstance( listType ) as IList;
            var deleteList = Activator.CreateInstance( listType ) as IList;

            if(( mirrorItemsSource != null ) &&
               ( deleteList != null )) {
                itemsControl.SetBinding( ItemsControl.ItemsSourceProperty, new Binding { Source = mirrorItemsSource } );

                AddItems( itemsControl, itemsSource, mirrorItemsSource );

                if( itemsSource is INotifyCollectionChanged notifySource ) {
                    notifySource.CollectionChanged += ( sender, collectionChangedArgs ) => {
                        switch( collectionChangedArgs.Action ) {
                            case NotifyCollectionChangedAction.Add:
                                AddItems( itemsControl, collectionChangedArgs.NewItems, mirrorItemsSource );
                                break;

                            case NotifyCollectionChangedAction.Remove:
                                RemoveItems( itemsControl, mirrorItemsSource, collectionChangedArgs.OldItems );
                                break;

                            case NotifyCollectionChangedAction.Reset:
                                deleteList.Clear();
                                foreach( var item in mirrorItemsSource ) {
                                    deleteList.Add( item );
                                }
                                RemoveItems( itemsControl, mirrorItemsSource, deleteList );
                                AddItems( itemsControl, itemsSource, mirrorItemsSource );
                                break;
                        }
                    };
                }
            }
        }

        private static void AddItems( ItemsControl itemsControl, IList items, IList mirrorItems ) {
            foreach( var newItem in items ) {
                mirrorItems.Add( newItem );
            }

            FadeInContainers( itemsControl, items );
        }

        private static void RemoveItems( ItemsControl itemsControl, IList mirrorItems, IList items ) {
            foreach( var oldItem in items ) {
                var container = itemsControl.ItemContainerGenerator.ContainerFromItem( oldItem ) as UIElement;
                var fadeOutAnimation = GetFadeOutAnimation(itemsControl);

                if(( container != null ) &&
                   ( fadeOutAnimation != null )) {
                    Storyboard.SetTarget( fadeOutAnimation, container );

                    void OnAnimationCompleted( object sender2, EventArgs e2 ) {
                        fadeOutAnimation.Completed -= OnAnimationCompleted;
                        mirrorItems.Remove( oldItem );
                    }

                    fadeOutAnimation.Completed += OnAnimationCompleted;
                    fadeOutAnimation.Begin();
                }
                else {
                    mirrorItems.Remove( oldItem );
                }
            }
        }

        private static void FadeInContainers( ItemsControl itemsControl, IList newItems ) {
            void StatusChanged( object sender, EventArgs e ) {
                if( itemsControl.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated ) {
                    itemsControl.ItemContainerGenerator.StatusChanged -= StatusChanged;
                    foreach( object newItem in newItems ) {
                        var container = itemsControl.ItemContainerGenerator.ContainerFromItem( newItem ) as UIElement;
                        var fadeInAnimation = GetFadeInAnimation( itemsControl );

                        if( container != null &&
                            fadeInAnimation != null ) {
                            Storyboard.SetTarget( fadeInAnimation, container );
                            fadeInAnimation.Begin();
                        }
                    }
                }
            }

            itemsControl.ItemContainerGenerator.StatusChanged += StatusChanged;
        }
    }
}
