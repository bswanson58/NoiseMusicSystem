using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ArchiveLoader.Dto;
using ArchiveLoader.Interfaces;

namespace ArchiveLoader.Models {
    class CatalogWriter : ICatalogWriter {
        private readonly IPreferences   mPreferences;

        public CatalogWriter( IPreferences preferences ) {
            mPreferences = preferences;
        }

        public void CreateCatalog( string volumeName, IEnumerable<CompletedProcessItem> items ) {
            var preferences = mPreferences.Load<ArchiveLoaderPreferences>();
            var reportFile = Path.ChangeExtension(Path.Combine(preferences.ReportDirectory, volumeName), ".xml" );

            if (volumeName.Contains(Path.VolumeSeparatorChar.ToString())) {
                reportFile = Path.ChangeExtension(Path.Combine(preferences.ReportDirectory, Path.GetFileNameWithoutExtension(volumeName)), ".xml" );
            }

            Task.Run(() => { WriteCatalog( reportFile, items ); });
        }

        private void WriteCatalog( string toFile, IEnumerable<CompletedProcessItem> items ) {}
    }
}
