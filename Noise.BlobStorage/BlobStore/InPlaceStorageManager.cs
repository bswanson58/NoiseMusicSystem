using Noise.Infrastructure.Interfaces;

namespace Noise.BlobStorage.BlobStore {
    class InPlaceStorageManager : IInPlaceStorageManager {
        private readonly IBlobStorage   mStorage;

        public InPlaceStorageManager( IInPlaceStorage storage ) {
            mStorage = storage;
        }

        public bool Initialize( ILibraryConfiguration libraryConfiguration ) {
            return true;
        }

        public bool OpenStorage() {
            return true;
        }

        public bool CreateStorage() {
            return true;
        }

        public void SetResolver( IBlobStorageResolver resolver ) { }
        public void CloseStorage() { }
        public void DeleteStorage() { }

        public bool IsOpen => true;

        public IBlobStorage GetStorage() {
            return mStorage;
        }
    }
}
