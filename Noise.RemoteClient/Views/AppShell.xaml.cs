using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Noise.RemoteClient.Views {
    [XamlCompilation( XamlCompilationOptions.Compile )]
    public partial class AppShell : Shell {
        public AppShell() {
            InitializeComponent();
        }
    }
}