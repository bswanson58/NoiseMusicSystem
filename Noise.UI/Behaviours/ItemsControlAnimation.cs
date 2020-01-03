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
    internal class ItemsControlState {
        private readonly object     mLock;
        private readonly Type       mListType;
        private readonly IList      mMirrorList;
        private readonly IList      mDeleteList;
        private readonly IList      mSourceList;

        public  ItemsControl        Control { get; }

        public ItemsControlState( ItemsControl itemsControl, IList sourceList ) {
            var itemsSourceType = sourceList.GetType();

            mSourceList = sourceList;
            mListType = typeof(ObservableCollection<>).MakeGenericType( itemsSourceType.GetGenericArguments()[0]);

            Control = itemsControl;

            mMirrorList = CreateList( mListType );
            mDeleteList = CreateList( mListType );

            mLock = new object();

            Control.SetBinding( ItemsControl.ItemsSourceProperty, new Binding { Source = mMirrorList } );
        }

        public IList CreateDeletionList() {
            var retValue = CreateList( mListType );

            lock( mLock ) {
                foreach( var item in mMirrorList ) {
                    if((!mSourceList.Contains( item )) &&
                       (!mDeleteList.Contains( item ))) {
                        retValue.Add( item );
                        mDeleteList.Add( item );
                    }
                }
            }

            return retValue;
        }

        public IList CreateAdditionList() {
            var retValue = CreateList( mListType );

            lock( mLock ) {
                foreach( var item in mSourceList ) {
                    if(!mMirrorList.Contains( item )) {
                        retValue.Add( item );
                        InsertItemInMirror( item );
                    }
                }
            }

            return retValue;
        }

        public void DeleteItem( object item ) {
            lock( mLock ) {
                if( mMirrorList.Contains( item )) {
                    mMirrorList.Remove( item );
                }
                if( mDeleteList.Contains( item )) {
                    mDeleteList.Remove( item );
                }
            }
        }

        private void InsertItemInMirror( object item ) {
            var sourceIndex = mSourceList.IndexOf( item );
            var addIndex = sourceIndex;
            var maxIndex = Math.Min( mMirrorList.Count - 1, sourceIndex );

            for( var i = 0; i <= maxIndex; i++ ) {
                if( mDeleteList.Contains( mMirrorList[i])) {
                    addIndex++;
                }
            }

            mMirrorList.Insert( addIndex, item );
        }

        private static IList CreateList( Type listType ) {
            return Activator.CreateInstance( listType ) as IList;
        }
    }

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

            if( itemsSource is INotifyCollectionChanged notifySource ) {
                var state = new ItemsControlState( itemsControl, itemsSource );

                notifySource.CollectionChanged += ( sender, collectionChangedArgs ) => {
                    RemoveItems( state, state.CreateDeletionList());

                    FadeInContainers( state.Control, state.CreateAdditionList());
                };
            }
        }

        private static void RemoveItems( ItemsControlState state, IList list ) {
            foreach( var oldItem in list ) {
                var container = state.Control.ItemContainerGenerator.ContainerFromItem( oldItem ) as UIElement;
                var fadeOutAnimation = GetFadeOutAnimation( state.Control );

                if(( container != null ) &&
                   ( fadeOutAnimation != null )) {
                    Storyboard.SetTarget( fadeOutAnimation, container );

                    void OnAnimationCompleted( object sender2, EventArgs e2 ) {
                        fadeOutAnimation.Completed -= OnAnimationCompleted;
                        state.DeleteItem( oldItem );
                    }

                    fadeOutAnimation.Completed += OnAnimationCompleted;
                    fadeOutAnimation.Begin();
                }
                else {
                    state.DeleteItem( oldItem );
                }
            }
        }

        private static void FadeInContainers( ItemsControl itemsControl, IList newItems ) {
            void StatusChanged( object sender, EventArgs e ) {
                if( itemsControl.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated ) {
                    itemsControl.ItemContainerGenerator.StatusChanged -= StatusChanged;

                    foreach( var newItem in newItems ) {
                        var container = itemsControl.ItemContainerGenerator.ContainerFromItem( newItem ) as UIElement;
                        var fadeInAnimation = GetFadeInAnimation( itemsControl );

                        if(( container != null ) &&
                           ( fadeInAnimation != null )) {
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
