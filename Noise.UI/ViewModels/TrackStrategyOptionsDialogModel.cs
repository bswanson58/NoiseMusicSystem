using System;
using Noise.Infrastructure.Dto;
using Prism.Commands;
using Prism.Services.Dialogs;
using ReusableBits.Mvvm.ViewModelSupport;

namespace Noise.UI.ViewModels {
    public class TrackStrategyOptionsDialogModel : PropertyChangeBase, IDialogAware {
        public  const string                        cTrackParameter = "track";

        private bool                                mDoNotPlay;

        public  DbTrack                             Track { get; private set; }
        
        public  bool                                PlayNext { get; set; }
        public  bool                                PlayPrevious { get; set; }
        public  bool                                CanPlayAdjacent { get; set; }

        public  string                              Title { get; }
        public  DelegateCommand                     Ok { get; }
        public  DelegateCommand                     Cancel { get; }
        public  event Action<IDialogResult>         RequestClose;

        public TrackStrategyOptionsDialogModel() {
            Title = "Strategy Play Options";

            Ok = new DelegateCommand( OnOk );
            Cancel = new DelegateCommand( OnCancel );
        }

        public void OnDialogOpened( IDialogParameters parameters ) {
            Track = parameters.GetValue<DbTrack>( cTrackParameter );

            if( Track != null ) {
                DoNotPlay = Track.DoNotStrategyPlay;

                switch( Track.PlayAdjacentStrategy ) {
                    case ePlayAdjacentStrategy.PlayNext:
                        PlayNext = true;
                        break;

                    case ePlayAdjacentStrategy.PlayPrevious:
                        PlayPrevious = true;
                        break;

                    case ePlayAdjacentStrategy.PlayNextPrevious:
                        PlayNext = true;
                        PlayPrevious = true;
                        break;
                }
            }

            RaisePropertyChanged( () => Track );
            RaisePropertyChanged( () => PlayNext );
            RaisePropertyChanged( () => PlayPrevious );
            RaisePropertyChanged( () => CanPlayAdjacent );
            RaisePropertyChanged( () => DoNotPlay );
        }

        public bool DoNotPlay {
            get => mDoNotPlay;
            set {
                mDoNotPlay = value;

                CanPlayAdjacent = !mDoNotPlay;
                if(!CanPlayAdjacent ) {
                    PlayNext = false;
                    PlayPrevious = false;
                }

                RaisePropertyChanged( () => CanPlayAdjacent );
                RaisePropertyChanged( () => PlayNext );
                RaisePropertyChanged( () => PlayPrevious );
            }
        }

        public bool CanCloseDialog() {
            return true;
        }

        public void OnDialogClosed() { }

        public void OnOk() {
            if( PlayNext  && !PlayPrevious ) {
                Track.PlayAdjacentStrategy = ePlayAdjacentStrategy.PlayNext;
            }
            else if( PlayPrevious && !PlayNext ) {
                Track.PlayAdjacentStrategy = ePlayAdjacentStrategy.PlayPrevious;
            }
            else if( PlayPrevious && PlayNext ) {
                Track.PlayAdjacentStrategy = ePlayAdjacentStrategy.PlayNextPrevious;
            }
            else if(!PlayPrevious && !PlayNext ) {
                Track.PlayAdjacentStrategy = ePlayAdjacentStrategy.None;
            }

            Track.DoNotStrategyPlay = DoNotPlay;

            RaiseRequestClose( new DialogResult( ButtonResult.OK, new DialogParameters{{ cTrackParameter, Track }}));
        }

        public void OnCancel() {
            RaiseRequestClose( new DialogResult( ButtonResult.Cancel ));
        }

        private void RaiseRequestClose( IDialogResult dialogResult ) {
            RequestClose?.Invoke( dialogResult );
        }
    }
}
