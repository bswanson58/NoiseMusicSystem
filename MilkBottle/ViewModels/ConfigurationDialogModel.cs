using System;
using System.Windows;
using MilkBottle.Dto;
using MilkBottle.Interfaces;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace MilkBottle.ViewModels {
    class ConfigurationDialogModel : PropertyChangeBase, IDialogAware {
        private readonly IPreferences       mPreferences;
        private readonly double             mAspectRatio;
        private int                         mMeshWidth;
        private int                         mMeshHeight;
        private int                         mSmoothPresetDuration;
        private float                       mBeatSensitivity;

        public  string                      Title { get; }
        public  DelegateCommand             Ok { get; }
        public  DelegateCommand             Cancel { get; }

        public  string                      MeshDescription => $"Width: {mMeshWidth} by Height:{mMeshHeight}";

        public event Action<IDialogResult> RequestClose;

        public ConfigurationDialogModel( IPreferences preferences ) {
            mPreferences = preferences;

            Title = "Configuration";
            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );

//            mAspectRatio = SystemParameters.FullPrimaryScreenHeight / SystemParameters.FullPrimaryScreenWidth;
            mAspectRatio = 9.0 / 16.0; // assume a common aspect ratio for now.

            var configuration = mPreferences.Load<MilkConfiguration>();

            SmoothPresetDuration = configuration.SmoothPresetDuration;
            BeatSensitivity = configuration.BeatSensitivity;
            mMeshWidth = configuration.MeshWidth;
            mMeshHeight = configuration.MeshHeight;
        }

        public int MeshWidth {
            get => mMeshWidth;
            set {
                mMeshWidth = value;

                MeshHeight = (int)( mMeshWidth * mAspectRatio );

                RaisePropertyChanged( () => MeshWidth );
                RaisePropertyChanged( () => MeshDescription );
            }
        }

        public int MeshHeight {
            get => mMeshHeight;
            set {
                mMeshHeight = value;

                RaisePropertyChanged( () => MeshHeight );
                RaisePropertyChanged( () => MeshDescription );
            }
        }

        public int SmoothPresetDuration {
            get => mSmoothPresetDuration;
            set {
                mSmoothPresetDuration = value;

                RaisePropertyChanged( () => SmoothPresetDuration );
            }
        }

        public float BeatSensitivity {
            get => mBeatSensitivity;
            set {
                mBeatSensitivity = value;

                RaisePropertyChanged( () => BeatSensitivity );
            }
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogOpened( IDialogParameters parameters ) { }
        public void OnDialogClosed() { }

        public void OnOk() {
            var configuration = mPreferences.Load<MilkConfiguration>();

            configuration.MeshWidth = MeshWidth;
            configuration.MeshHeight = MeshHeight;
            configuration.BeatSensitivity = BeatSensitivity;
            configuration.SmoothPresetDuration = SmoothPresetDuration;

            mPreferences.Save( configuration );

            RaiseRequestClose( new DialogResult( ButtonResult.OK ));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
