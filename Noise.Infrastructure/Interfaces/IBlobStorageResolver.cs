namespace Noise.Infrastructure.Interfaces {
	public interface IBlobStorageResolver {
		uint		StorageLevels { get; }
		string		KeyForStorageLevel( long blobId, uint level );
	}
}
