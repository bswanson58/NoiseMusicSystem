using System;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;
using MahApps.Metro.Controls;

namespace MilkBottle.Behaviors {
    //<i:Interaction.Behaviors>
    //    <behaviors:MetroWindowTitleColor NormalBrush="{DynamicResource AccentColorBrush}" MaximizedBrush="{DynamicResource WindowBackgroundBrush}"/>
    //</i:Interaction.Behaviors>

    class MetroWindowTitleColor : Behavior<MetroWindow> {
        public static readonly DependencyProperty MaximizedBrushProperty = DependencyProperty.Register(
            "MaximizedBrush",
            typeof( Brush ),
            typeof( MetroWindowTitleColor ),
            new PropertyMetadata( null ));

        public Brush MaximizedBrush {
            get => GetValue( MaximizedBrushProperty ) as Brush;
            set => SetValue( MaximizedBrushProperty, value );
        }

        public static readonly DependencyProperty NormalBrushProperty = DependencyProperty.Register(
            "NormalBrush",
            typeof( Brush ),
            typeof( MetroWindowTitleColor ),
            new PropertyMetadata( null ));

        public Brush NormalBrush {
            get => GetValue( NormalBrushProperty ) as Brush;
            set => SetValue( NormalBrushProperty, value );
        }

        protected override void OnAttached() {
            base.OnAttached();

            AssociatedObject.StateChanged += OnStateChanged;
        }

        private void OnStateChanged( object sender, EventArgs args ) {
            AssociatedObject.WindowTitleBrush = AssociatedObject.WindowState == WindowState.Maximized ? MaximizedBrush : NormalBrush;
        }

        protected override void OnDetaching() {
            base.OnDetaching();

            AssociatedObject.StateChanged -= OnStateChanged;
        }
    }
}
