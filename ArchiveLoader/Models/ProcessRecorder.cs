using System.Collections.Generic;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;
using Caliburn.Micro;

namespace ArchiveLoader.Models {
    class ProcessRecorder : IProcessRecorder {
        private readonly ICatalogWriter     mCatalogWriter;
        private readonly IReportWriter      mReportWriter;

        private readonly IDictionary<string, BindableCollection<CompletedProcessItem>>    mCompletedItems;

        public BindableCollection<string>   AvailableVolumes { get; }

        public ProcessRecorder( ICatalogWriter catalogWriter, IReportWriter reportWriter ) {
            mCatalogWriter = catalogWriter;
            mReportWriter = reportWriter;

            mCompletedItems = new Dictionary<string, BindableCollection<CompletedProcessItem>>();
            AvailableVolumes = new BindableCollection<string>();
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
                AvailableVolumes.Add( targets.SourceVolumeName );
            }
        }

        public void ItemCompleted( ProcessItem item ) {
            if( mCompletedItems.ContainsKey( item.VolumeName )) {
                mCompletedItems[item.VolumeName].Add( new CompletedProcessItem( item ));
            }
        }

        public void JobCompleted( string volumeName ) {
            mCatalogWriter.CreateCatalog( volumeName, GetItemsForVolume( volumeName ));
            mReportWriter.CreateReport( volumeName, GetItemsForVolume( volumeName ));
        }
    }
}
