using ArchiveLoader.Dto;
using Caliburn.Micro;

namespace ArchiveLoader.Interfaces {
    public interface IProcessRecorder {
        void    JobStarted( Events.JobTargets targets );
        void    ItemCompleted( ProcessItem item );
        void    JobCompleted( string volumeName );

        BindableCollection<DisplayedStatusItem>     AvailableVolumes { get; }
        BindableCollection<CompletedProcessItem>    GetItemsForVolume( string volume );
    }
}
