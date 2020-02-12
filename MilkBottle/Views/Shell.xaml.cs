using System;
using System.ComponentModel;
using System.Windows;
using MilkBottle.Properties;
using MilkBottle.Support;

namespace MilkBottle.Views {
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell {
        public Shell() {
            InitializeComponent();

            Closing += OnWindowClosing;
//            StateChanged += OnStateChanged;
        }

        private void OnStateChanged( object sender, EventArgs args ) {
            UseNoneWindowStyle = WindowState == WindowState.Maximized;
        }

        private void OnWindowClosing( object sender, CancelEventArgs e ) {
            Settings.Default.ShellWindowPlacement = this.GetPlacement();
            Settings.Default.Save();
        }

        protected override void OnSourceInitialized( EventArgs e ) {
            base.OnSourceInitialized( e );

            this.SetPlacement( Settings.Default.ShellWindowPlacement );
        }
    }
}
