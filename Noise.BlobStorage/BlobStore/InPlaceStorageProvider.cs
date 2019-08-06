using System;
using System.IO;
using Noise.Infrastructure.Interfaces;

namespace Noise.BlobStorage.BlobStore {
    class InPlaceStorageProvider : IInPlaceStorage {
        private readonly INoiseLog		        mLog;
        private readonly IStorageFileProvider	mStorageFileProvider;
        private readonly IStorageFolderSupport	mStorageFolderSupport;

        public InPlaceStorageProvider( IStorageFileProvider storageFileProvider, IStorageFolderSupport storageFolderSupport, INoiseLog log ) {
            mLog = log;
            mStorageFileProvider = storageFileProvider;
            mStorageFolderSupport = storageFolderSupport;
        }

        private string ResolvePath( long blobId ) {
            var retValue = string.Empty;
            var file = mStorageFileProvider.GetFileForMetadata( blobId );

            if( file != null ) {
                retValue = mStorageFolderSupport.GetPath( file );
            }

            return retValue;
        }

        public bool IsInPlace => true;

        public bool BlobExists( string blobId ) {
            return true;
        }

        public void Insert( long blobId, string fromFile ) { }
        public void Insert( long blobId, Stream blobData ) { }
        public void Insert( string blobId, string data ) {
            throw new NotImplementedException();
        }
        public void Insert<T>( string blobId, T data ) {
            throw new NotImplementedException();
        }

        public void Store( long blobId, string fromFile ) { }
        public void Store( long blobId, Stream blobData ) { }
        public void Store<T>( string blobId, T data ) {
            throw new NotImplementedException();
        }
        public void StoreText( long blobId, string text ) {
        }
        public void StoreText( string blobId, string text ) {
            throw new NotImplementedException();
        }

        public void Delete( long blobId ) { }
        public void Delete( string blobId ) {
            throw new NotImplementedException();
        }

        public Stream Retrieve( long blobId ) {
            var blobPath = ResolvePath( blobId );
            var retValue = default( Stream );

            if(!string.IsNullOrWhiteSpace( blobPath )) {
                if( File.Exists( blobPath )) {
                    retValue = new FileStream( blobPath, FileMode.Open, FileAccess.Read );
                }
                else {
                    mLog.LogMessage( $"Attempt to retrieve non-existent blob item: { blobId }" );

                    retValue = new MemoryStream();
//				throw new BlobStorageException( blobId, blobPath, "Attempt to retrieve nonexistent item." );
                }
            }

            return( retValue );
        }

        public Stream Retrieve( string blobId ) {
            throw new NotImplementedException();
        }

        public byte[] RetrieveBytes( long blobId ) {
            byte[]	retValue = null;

            using( var stream = Retrieve( blobId )) {
                if( stream != null ) {
                    retValue = new byte[stream.Length];

                    stream.Read( retValue, 0, retValue.Length );
                    stream.Close();
                }
            }
			

            return( retValue );
        }

        public string RetrieveText( long blobId ) {
            var		retValue = string.Empty;

            using( var stream = Retrieve( blobId )) {
                if( stream != null ) {
                    using( var reader = new StreamReader( stream )) {
                        retValue = reader.ReadToEnd();

                        reader.Close();
                    }

                    stream.Close();
                }
            }

            return( retValue );
        }

        public string RetrieveText( string blobId ) {
            throw new NotImplementedException();
        }

        public T RetrieveObject<T>( string blobId ) {
            throw new NotImplementedException();
        }

        public void DeleteStorage() { }
    }
}
