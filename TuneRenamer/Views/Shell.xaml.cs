﻿using System;
using System.ComponentModel;
using TuneRenamer.Properties;
using TuneRenamer.Support;

namespace TuneRenamer.Views {
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell {
        public Shell() {
            InitializeComponent();

            Closing += OnWindowClosing;
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
