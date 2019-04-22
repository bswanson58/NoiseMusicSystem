using Noise.Infrastructure.Interfaces;

namespace Noise.BlobStorage.BlobStore {
    class InPlaceStorageManager : IInPlaceStorageManager {
        public void SetResolver( IBlobStorageResolver resolver ) {
            throw new System.NotImplementedException();
        }

        public bool Initialize( ILibraryConfiguration libraryConfiguration ) {
            throw new System.NotImplementedException();
        }

        public bool OpenStorage() {
            throw new System.NotImplementedException();
        }

        public bool CreateStorage() {
            throw new System.NotImplementedException();
        }

        public void CloseStorage() {
            throw new System.NotImplementedException();
        }

        public void DeleteStorage() {
            throw new System.NotImplementedException();
        }

        public bool IsOpen { get; }
        public IBlobStorage GetStorage() {
            throw new System.NotImplementedException();
        }
    }
}
