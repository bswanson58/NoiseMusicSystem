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
        DependencyProperty.RegisterAttached("ItemsSource",
                                            typeof(IList),
                                            typeof(ItemsControlAnimation),
                                            new UIPropertyMetadata(null, ItemsSourcePropertyChanged));
        public static void SetItemsSource( DependencyObject element, IList value ) {
            element.SetValue( ItemsSourceProperty, value );
        }
        public static IList GetItemsSource( DependencyObject element ) {
            return (IList)element.GetValue( ItemsSourceProperty );
        }

        private static void ItemsSourcePropertyChanged( DependencyObject source, DependencyPropertyChangedEventArgs e ) {
            ItemsControl itemsControl = source as ItemsControl;
            IList itemsSource = e.NewValue as IList;
            if ( itemsControl == null ) {
                return;
            }
            if ( itemsSource == null ) {
                itemsControl.ItemsSource = null;
                return;
            }

            Type itemsSourceType = itemsSource.GetType();
            Type listType = typeof(ObservableCollection<>).MakeGenericType(itemsSourceType.GetGenericArguments()[0]);
            IList mirrorItemsSource = (IList)Activator.CreateInstance(listType);
            itemsControl.SetBinding( ItemsControl.ItemsSourceProperty, new Binding { Source = mirrorItemsSource } );

            foreach ( object item in itemsSource ) {
                mirrorItemsSource.Add( item );
            }
            FadeInContainers( itemsControl, itemsSource );

            (itemsSource as INotifyCollectionChanged).CollectionChanged +=
                ( object sender, NotifyCollectionChangedEventArgs ne ) => {
                    if ( ne.Action == NotifyCollectionChangedAction.Add ) {
                        foreach ( object newItem in ne.NewItems ) {
                            mirrorItemsSource.Add( newItem );
                        }
                        FadeInContainers( itemsControl, ne.NewItems );
                    }
                    else if ( ne.Action == NotifyCollectionChangedAction.Remove ) {
                        foreach ( object oldItem in ne.OldItems ) {
                            UIElement container = itemsControl.ItemContainerGenerator.ContainerFromItem(oldItem) as UIElement;
                            Storyboard fadeOutAnimation = GetFadeOutAnimation(itemsControl);
                            if ( container != null && fadeOutAnimation != null ) {
                                Storyboard.SetTarget( fadeOutAnimation, container );

                                EventHandler onAnimationCompleted = null;
                                onAnimationCompleted = (( sender2, e2 ) => {
                                    fadeOutAnimation.Completed -= onAnimationCompleted;
                                    mirrorItemsSource.Remove( oldItem );
                                });

                                fadeOutAnimation.Completed += onAnimationCompleted;
                                fadeOutAnimation.Begin();
                            }
                            else {
                                mirrorItemsSource.Remove( oldItem );
                            }
                        }
                    }
                };
        }

        private static void FadeInContainers( ItemsControl itemsControl, IList newItems ) {
            EventHandler statusChanged = null;
            statusChanged = new EventHandler( delegate {
                if ( itemsControl.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated ) {
                    itemsControl.ItemContainerGenerator.StatusChanged -= statusChanged;
                    foreach ( object newItem in newItems ) {
                        UIElement container = itemsControl.ItemContainerGenerator.ContainerFromItem(newItem) as UIElement;
                        Storyboard fadeInAnimation = GetFadeInAnimation(itemsControl);
                        if ( container != null && fadeInAnimation != null ) {
                            Storyboard.SetTarget( fadeInAnimation, container );
                            fadeInAnimation.Begin();
                        }
                    }
                }
            } );
            itemsControl.ItemContainerGenerator.StatusChanged += statusChanged;
        }

        public static readonly DependencyProperty FadeInAnimationProperty =
        DependencyProperty.RegisterAttached("FadeInAnimation",
                                            typeof(Storyboard),
                                            typeof(ItemsControlAnimation),
                                            new UIPropertyMetadata(null));
        public static void SetFadeInAnimation( DependencyObject element, Storyboard value ) {
            element.SetValue( FadeInAnimationProperty, value );
        }
        public static Storyboard GetFadeInAnimation( DependencyObject element ) {
            return (Storyboard)element.GetValue( FadeInAnimationProperty );
        }

        public static readonly DependencyProperty FadeOutAnimationProperty =
        DependencyProperty.RegisterAttached("FadeOutAnimation",
                                            typeof(Storyboard),
                                            typeof(ItemsControlAnimation),
                                            new UIPropertyMetadata(null));
        public static void SetFadeOutAnimation( DependencyObject element, Storyboard value ) {
            element.SetValue( FadeOutAnimationProperty, value );
        }
        public static Storyboard GetFadeOutAnimation( DependencyObject element ) {
            return (Storyboard)element.GetValue( FadeOutAnimationProperty );
        }
    }
}
