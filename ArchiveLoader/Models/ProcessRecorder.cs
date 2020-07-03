using System.Collections.Generic;
using System.Linq;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;
using Caliburn.Micro;

namespace ArchiveLoader.Models {
    class ProcessRecorder : IProcessRecorder {
        private readonly ICatalogWriter     mCatalogWriter;
        private readonly IReportWriter      mReportWriter;

        private readonly IDictionary<string, BindableCollection<CompletedProcessItem>>    mCompletedItems;

        public BindableCollection<DisplayedStatusItem>  AvailableVolumes { get; }

        public ProcessRecorder( ICatalogWriter catalogWriter, IReportWriter reportWriter ) {
            mCatalogWriter = catalogWriter;
            mReportWriter = reportWriter;

            mCompletedItems = new Dictionary<string, BindableCollection<CompletedProcessItem>>();
            AvailableVolumes = new BindableCollection<DisplayedStatusItem>();
        }

        public BindableCollection<CompletedProcessItem> GetItemsForVolume( string volume ) {
            var retValue = default( BindableCollection<CompletedProcessItem>);

            if( mCompletedItems.ContainsKey( volume )) {
                retValue = mCompletedItems[volume];
            }

            return retValue;
        }
        public void JobStarted( Events.JobTargets targets ) {
            if(!mCompletedItems.ContainsKey( targets.SourceVolumeName )) {
                mCompletedItems.Add( targets.SourceVolumeName, new BindableCollection<CompletedProcessItem>());
                AvailableVolumes.Add( new DisplayedStatusItem( targets.SourceVolumeName ));
            }
        }

        public void ItemCompleted( ProcessItem item ) {
            if( mCompletedItems.ContainsKey( item.VolumeName )) {
                mCompletedItems[item.VolumeName].Add( new CompletedProcessItem( item ));

                UpdateVolumeStatus( item.VolumeName, ProcessState.Running );
            }
        }

        public void JobCompleted( string volumeName ) {
            UpdateVolumeStatus( volumeName, ProcessState.Completed );

            mCatalogWriter.CreateCatalog( volumeName, GetItemsForVolume( volumeName ).OrderBy( i => i.FileName ));
            mReportWriter.CreateReport( volumeName, GetItemsForVolume( volumeName ).OrderBy( i => i.FileName ));
        }

        private void UpdateVolumeStatus( string volumeName, ProcessState defaultStatus ) {
            var volume = AvailableVolumes.FirstOrDefault( v => v.Name.Equals( volumeName ));

            if( volume != null ) {
                var statusList = mCompletedItems[volumeName].Select(i => i.FinalState).ToList();

                if( statusList.Any( s => s == ProcessState.Error )) {
                    volume.UpdateStatus( ProcessState.Error );
                }
                else if( statusList.Any( s => s == ProcessState.Aborted )) {
                    volume.UpdateStatus( ProcessState.Aborted );
                }
                else {
                    volume.UpdateStatus( defaultStatus );
                }
            }
        }
    }
}
