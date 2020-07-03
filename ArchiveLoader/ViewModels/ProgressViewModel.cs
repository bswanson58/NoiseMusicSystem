using System;
using ArchiveLoader.Dto;
using Caliburn.Micro;
using ReusableBits.Mvvm.ViewModelSupport;

namespace ArchiveLoader.ViewModels {
    class ProgressViewModel : PropertyChangeBase, IDisposable,
                              IHandle<Events.VolumeDetected>, IHandle<Events.VolumeStarted>, IHandle<Events.FileCopied>, IHandle<Events.VolumeCompleted> {
        private readonly IEventAggregator   mEventAggregator;

        public  string                      CurrentVolume { get; private set; }
        public  string                      CurrentFile {  get; private set; }
        public  long                        TotalFileSize { get; private set; }
        public  long                        CurrentFileSize { get; set; }
        public  int                         CurrentPercent { get; private set; }
        public  bool                        IsActive { get; private set; }

        public ProgressViewModel( IEventAggregator eventAggregator ) {
            mEventAggregator = eventAggregator;

            TotalFileSize = 1;
            CurrentFileSize = 0;

            mEventAggregator.Subscribe( this );
        }

        public void Handle( Events.VolumeDetected message ) {
            CurrentVolume = message.VolumeName;

            TotalFileSize = 1;
            CurrentFileSize = 0;
            CurrentPercent = 0;
            CurrentFile = String.Empty;
            IsActive = true;

            RaisePropertyChanged( () => CurrentVolume );
            RaisePropertyChanged( () => CurrentFile );
            RaisePropertyChanged( () => CurrentPercent );
            RaisePropertyChanged( () => TotalFileSize );
            RaisePropertyChanged( () => CurrentFileSize );
            RaisePropertyChanged( () => IsActive );
        }

        public void Handle( Events.VolumeStarted message ) {
            CurrentVolume = message.VolumeName;
            TotalFileSize = message.VolumeSize;

            CurrentFile = String.Empty;
            CurrentFileSize = 0L;
            CurrentPercent = 0;
            IsActive = true;

            RaisePropertyChanged( () => CurrentVolume );
            RaisePropertyChanged( () => CurrentFile );
            RaisePropertyChanged( () => CurrentPercent );
            RaisePropertyChanged( () => TotalFileSize );
            RaisePropertyChanged( () => CurrentFileSize );
            RaisePropertyChanged( () => IsActive );
        }

        public void Handle( Events.FileCopied message ) {
            CurrentFile = message.FileName;
            CurrentFileSize += message.FileSize;

            if( TotalFileSize > 1 ) {
                if( CurrentFileSize > TotalFileSize ) {
                    CurrentFileSize = TotalFileSize;
                }

                CurrentPercent = (int)(((float)CurrentFileSize / TotalFileSize ) * 100 );

                RaisePropertyChanged( () => CurrentFileSize );
                RaisePropertyChanged( () => CurrentPercent );
            }

            RaisePropertyChanged( () => CurrentFile );
        }

        public void Handle( Events.VolumeCompleted message ) {
            IsActive = false;

            RaisePropertyChanged( () => IsActive );
        }

        public void Dispose() {
            mEventAggregator.Unsubscribe( this );
        }
    }
}
