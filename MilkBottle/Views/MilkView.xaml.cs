﻿using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using MilkBottle.ViewModels;
using OpenTK;
using OpenTK.Graphics;

namespace MilkBottle.Views {
    /// <summary>
    /// Interaction logic for MilkView.xaml
    /// </summary>
    public partial class MilkView {
        private GLControl   mGlControl;

        public MilkView() {
            InitializeComponent();
        
            if(( DataContext is MilkViewModel vm ) &&
               ( mGlControl != null )) {
                vm.Initialize( mGlControl );
                vm.StartVisualization();
            }
        }

        private void WindowsFormsHost_Initialized(object sender, EventArgs e) {
            mGlControl = new GLControl( new GraphicsMode( 32, 24 ), 2, 0, GraphicsContextFlags.Default ) { Dock = DockStyle.Fill };

            if( sender is WindowsFormsHost host ) {
                host.Child = mGlControl;
            }
        }

        private void OnSizeChanged( object sender, SizeChangedEventArgs e ) {
            if( DataContext is MilkViewModel vm ) {
                vm.OnSizeChanged( (int)e.NewSize.Width, (int)e.NewSize.Height );
            }
        }
    }
}