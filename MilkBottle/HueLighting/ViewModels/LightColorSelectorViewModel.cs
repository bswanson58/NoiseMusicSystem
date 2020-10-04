using System;
using System.Windows.Media;
using HueLighting.Controls;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace HueLighting.ViewModels {
    public class LightColorSelectorViewModel : PropertyChangeBase, IDialogAware, IDisposable {
        public  const string                cSelectedColor = "color";

        private Color                       mLightColor;
        private IDisposable                 mColorSelectorSubscription;

        public  HslColorSelectorViewModel   ColorSelector { get; }

        public  DelegateCommand             Ok { get; }
        public  DelegateCommand             Cancel { get; }

        public  string                      Title { get; }
        public  event Action<IDialogResult> RequestClose;

        public LightColorSelectorViewModel( HslColorSelectorViewModel colorSelector ) {
            ColorSelector = colorSelector;
            mColorSelectorSubscription = ColorSelector.ColorChanged.Subscribe( OnColorChanged );

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );

            Title = "Hue Light Color Selection";
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            mLightColor = parameters.GetValue<Color>( cSelectedColor );
            ColorSelector.SelectedColor = mLightColor;
        }

        private void OnColorChanged( Color color ) {
            mLightColor = color;
        }

        private void OnOk() {
            var parameters = new DialogParameters{{ cSelectedColor, mLightColor }};

            RaiseRequestClose( new DialogResult( ButtonResult.OK, parameters ));
        }

        private void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }

        public void Dispose() {
            mColorSelectorSubscription?.Dispose();
            mColorSelectorSubscription = null;
        }
    }
}
