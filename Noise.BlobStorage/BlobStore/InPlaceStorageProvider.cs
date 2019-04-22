using System.IO;
using Noise.Infrastructure.Interfaces;

namespace Noise.BlobStorage.BlobStore {
    class InPlaceStorageProvider : IBlobStorage {
        public bool BlobExists( string blobId ) {
            throw new System.NotImplementedException();
        }

        public void Insert( long blobId, string fromFile ) {
            throw new System.NotImplementedException();
        }

        public void Insert( long blobId, Stream blobData ) {
            throw new System.NotImplementedException();
        }

        public void Insert( string blobId, string data ) {
            throw new System.NotImplementedException();
        }

        public void Insert<T>( string blobId, T data ) {
            throw new System.NotImplementedException();
        }

        public void Store( long blobId, string fromFile ) {
            throw new System.NotImplementedException();
        }

        public void Store( long blobId, Stream blobData ) {
            throw new System.NotImplementedException();
        }

        public void Store<T>( string blobId, T data ) {
            throw new System.NotImplementedException();
        }

        public void StoreText( long blobId, string text ) {
            throw new System.NotImplementedException();
        }

        public void StoreText( string blobId, string text ) {
            throw new System.NotImplementedException();
        }

        public void Delete( long blobId ) {
            throw new System.NotImplementedException();
        }

        public void Delete( string blobId ) {
            throw new System.NotImplementedException();
        }

        public Stream Retrieve( long blobId ) {
            throw new System.NotImplementedException();
        }

        public Stream Retrieve( string blobId ) {
            throw new System.NotImplementedException();
        }

        public byte[] RetrieveBytes( long blobId ) {
            throw new System.NotImplementedException();
        }

        public string RetrieveText( long blobId ) {
            throw new System.NotImplementedException();
        }

        public string RetrieveText( string blobId ) {
            throw new System.NotImplementedException();
        }

        public T RetrieveObject<T>( string blobId ) {
            throw new System.NotImplementedException();
        }

        public void DeleteStorage() {
            throw new System.NotImplementedException();
        }
    }
}
