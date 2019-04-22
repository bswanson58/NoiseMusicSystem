namespace Noise.Infrastructure.Interfaces {
    public interface IBlobStorageProvider {
        IBlobStorage         BlobStorage { get; }
    }
}
