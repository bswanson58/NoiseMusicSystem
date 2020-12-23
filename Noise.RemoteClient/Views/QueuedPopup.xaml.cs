using Noise.RemoteClient.Dto;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms.Xaml;

namespace Noise.RemoteClient.Views {
    [XamlCompilation( XamlCompilationOptions.Compile )]
    partial class QueuedPopup : PopupPage {
        public QueuedPopup( QueuedItem item ) {
            InitializeComponent();

            _itemName.Text = item.QueuedItemName;
        }
    }
}