using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using MilkBottle.ViewModels;
using OpenTK;
using OpenTK.Graphics;

namespace MilkBottle.Views {
    /// <summary>
    /// Interaction logic for GlSurface.xaml
    /// </summary>
    public partial class GlSurface {
        GLControl mGlControl;
        
        public GlSurface() {
            InitializeComponent();

            if(( DataContext is GlSurfaceViewModel vm ) &&
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
            if( DataContext is GlSurfaceViewModel vm ) {
                vm.OnSizeChanged( (int)e.NewSize.Width, (int)e.NewSize.Height );
            }
        }
    }
}
