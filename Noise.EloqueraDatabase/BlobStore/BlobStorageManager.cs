using System.IO;
using Noise.Infrastructure.Interfaces;

namespace Noise.EloqueraDatabase.BlobStore {
	public class BlobStorageManager : IBlobStorageManager, IBlobStorage {
		private readonly IBlobStorageResolver	mBlobResolver;
		private readonly string					mRootStoragePath;
		private bool							mIsOpen;
		private string							mStoragePath;

		public BlobStorageManager( IBlobStorageResolver blobResolver, string rootStoragePath ) {
			mBlobResolver = blobResolver;
			mRootStoragePath = rootStoragePath;

			mIsOpen = false;
			mStoragePath = "";
		}

		public bool OpenStorage( string storageName ) {
			if(!string.IsNullOrWhiteSpace( mStoragePath )) {
				CloseStorage();
			}

			if( Directory.Exists( mRootStoragePath )) {
				string	storagePath = Path.Combine( mRootStoragePath, storageName );

				if( Directory.Exists( storagePath )) {
					mStoragePath = storagePath;

					mIsOpen = true;
				}
			}

			return( mIsOpen );
		}

		public bool CreateStorage( string storageName ) {
			string	storagePath = Path.Combine( mRootStoragePath, storageName );

			if(!Directory.Exists( storagePath )) {
				Directory.CreateDirectory( storagePath );
			}

			return( Directory.Exists( storagePath ));
		}

		public void CloseStorage() {
			if( mIsOpen ) {
				mIsOpen = false;
				mStoragePath = "";
			}
		}

		public IBlobStorage GetStorage() {
			return( this );
		}

		public void Insert( long blobId, string fromFile ) {
			Insert( blobId, new FileStream( fromFile, FileMode.Open, FileAccess.Read ));
		}

		public void Insert( long blobId, Stream blobData ) {
			string	blobPath = ResolveBlobId( blobId );

			if( File.Exists( blobPath )) {
				throw new BlobStorageException( blobId, blobPath, "Attempt to insert existing item." );
			}

			StoreBlob( blobData, blobPath );
		}

		public void Update( long blobId, string fromFile ) {
			Update( blobId, new FileStream( fromFile, FileMode.Open, FileAccess.Read ));
		}

		public void Update( long blobId, Stream blobData ) {
			string	blobPath = ResolveBlobId( blobId );

			if(!File.Exists( blobPath )) {
				throw new BlobStorageException( blobId, blobPath, "Attempt to update nonexistent item." );
			}

			StoreBlob( blobData, blobPath );
		}

		public void StoreText( long blobId, string text ) {
			var	blobPath = ResolveBlobId( blobId );
			var storagePath = Path.GetDirectoryName( blobPath );

			if(!string.IsNullOrEmpty( storagePath )) {
				if(!Directory.Exists( storagePath )) {
					Directory.CreateDirectory( storagePath );
				}

				var	blobStream = new FileStream( blobPath, FileMode.Create, FileAccess.Write );
				var writer = new StreamWriter( blobStream );

				writer.Write( text );
				blobStream.Close();
			}
		}

		public void Store( long blobId, string fromFile ) {
			StoreBlob( new FileStream( fromFile, FileMode.Open, FileAccess.Read ), ResolveBlobId( blobId ));
		}

		public void Store( long blobId, Stream blobData ) {
			StoreBlob( blobData, ResolveBlobId( blobId ));
		}

		public void Delete( long blobId ) {
			string	blobPath = ResolveBlobId( blobId );

			if( File.Exists( blobPath )) {
				File.Delete( blobPath );
			}
			else {
				throw new BlobStorageException( blobId, blobPath, "Attempt to delete nonexistent item." );
			}
		}

		public string RetrieveText( long blobId ) {
			var		retValue = "";
			var		stream = Retrieve( blobId );

			if( stream != null ) {
				var reader = new StreamReader( stream );

				retValue = reader.ReadToEnd();
			}

			return( retValue );
		}

		public byte[] RetrieveBytes( long blobId ) {
			byte[]	retValue = null;
			var		stream = Retrieve( blobId );

			if( stream != null ) {
				retValue = new byte[stream.Length];

				stream.Read( retValue, 0, retValue.Length );
			}

			return( retValue );
		}

		public Stream Retrieve( long blobId ) {
			Stream	retValue;
			string	blobPath = ResolveBlobId( blobId );

			if( File.Exists( blobPath )) {
				retValue = new FileStream( blobPath, FileMode.Open, FileAccess.Read );
			}
			else {
				throw new BlobStorageException( blobId, blobPath, "Attempt to retrieve nonexistent item." );
			}

			return( retValue );
		}

		private static void StoreBlob( Stream blobData, string blobPath ) {
			var storagePath = Path.GetDirectoryName( blobPath );

			if(!string.IsNullOrEmpty( storagePath )) {
				if(!Directory.Exists( storagePath )) {
					Directory.CreateDirectory( storagePath );
				}

				Stream	blobStream = new FileStream( blobPath, FileMode.Create, FileAccess.Write );

				blobData.Position = 0;
				blobData.CopyTo( blobStream );
				blobStream.Close();
			}
		}

		private string ResolveBlobId( long blobId ) {
			var retValue = mStoragePath;

			for( uint level = 0; level < mBlobResolver.StorageLevels; level++ ) {
				retValue = Path.Combine( retValue, mBlobResolver.KeyForStorageLevel( blobId, level ));
			}

			retValue = Path.Combine( retValue, blobId.ToString());

			return( retValue );
		}
	}
}
