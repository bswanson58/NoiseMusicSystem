using System.IO;

namespace Noise.Infrastructure.Interfaces {
    public interface IInPlaceStorage : IBlobStorage { }

	public interface IBlobStorage {
		bool	BlobExists( string blobId );

		void	Insert( long blobId, string fromFile );
		void	Insert( long blobId, Stream blobData );
		void	Insert( string blobId, string data );
		void	Insert<T>( string blobId, T data );

		void	Store( long blobId, string fromFile );
		void	Store( long blobId, Stream blobData );
		void	Store<T>( string blobId, T data );
		void	StoreText( long blobId, string text );
		void	StoreText( string blobId, string text );

		void	Delete( long blobId );
		void	Delete( string blobId );

		Stream	Retrieve( long blobId );
		Stream	Retrieve( string blobId );
		byte[]	RetrieveBytes( long blobId );
		string	RetrieveText( long blobId );
		string	RetrieveText( string blobId );
		T		RetrieveObject<T>( string blobId );

		void	DeleteStorage();
	}

}
