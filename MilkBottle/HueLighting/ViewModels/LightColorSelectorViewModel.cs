using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using HueLighting.Controls;
using HueLighting.Dto;
using HueLighting.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace HueLighting.ViewModels {
    public class LightColorSelectorViewModel : PropertyChangeBase, IDialogAware, IDisposable {
        public  const string                cSelectedColor = "color";

        private readonly IHubManager        mHubManager;
        private readonly List<Bulb>         mTargetBulbs;
        private CancellationTokenSource     mTokenSource;

        private Color                       mLightColor;
        private IDisposable                 mColorSelectorSubscription;
        private IDisposable                 mBulbSelectorSubscription;

        public  HsbColorSelectorViewModel   ColorSelector { get; }
        public  BulbSelectorViewModel       BulbSelector { get; }

        public  DelegateCommand             Ok { get; }
        public  DelegateCommand             Cancel { get; }

        public  string                      Title { get; }
        public  event Action<IDialogResult> RequestClose;

        public LightColorSelectorViewModel( IHubManager hubManager, HsbColorSelectorViewModel colorSelector, BulbSelectorViewModel bulbSelector ) {
            mHubManager = hubManager;
            ColorSelector = colorSelector;
            BulbSelector = bulbSelector;

            mTargetBulbs = new List<Bulb>();
            mColorSelectorSubscription = ColorSelector.ColorChanged.Subscribe( OnColorChanged );
            mBulbSelectorSubscription = BulbSelector.TargetLightsChanged.Subscribe( OnBulbTargetChanged );

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

            UpdateTargetBulbs();
        }

        private async void OnBulbTargetChanged( IEnumerable<Bulb> bulbList ) {
            await mHubManager.SetBulbState( mTargetBulbs, false );
            mTargetBulbs.Clear();

            if( bulbList != null ) {
                mTargetBulbs.AddRange( bulbList );

                await mHubManager.SetBulbState( mTargetBulbs, true );
                UpdateTargetBulbs();
            }
        }

        private void UpdateTargetBulbs() {
            mTokenSource?.Cancel();
            mTokenSource = new CancellationTokenSource();

            Task.Run( () => UpdateTargetBulbsTask( mTargetBulbs.ToArray(), mLightColor, mTokenSource.Token ), mTokenSource.Token );
        }

        private async void UpdateTargetBulbsTask( IEnumerable<Bulb> bulbList, Color toColor, CancellationToken cancellationToken ) {
            foreach( var bulb in bulbList ) {
                if(!cancellationToken.IsCancellationRequested ) {
                    await mHubManager.SetBulbState( bulb, toColor );
                }
            }
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

        public void OnDialogClosed() {
            mHubManager.SetBulbState( mTargetBulbs, false );
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }

        public void Dispose() {
            mColorSelectorSubscription?.Dispose();
            mColorSelectorSubscription = null;

            mBulbSelectorSubscription?.Dispose();
            mBulbSelectorSubscription = null;

            mTokenSource?.Cancel();
            mTokenSource = null;
        }
    }
}
