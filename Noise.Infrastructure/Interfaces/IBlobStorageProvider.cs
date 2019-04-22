namespace Noise.Infrastructure.Interfaces {
    public interface IBlobStorageProvider {
        IBlobStorageManager  BlobStorageManager {  get; }
    }
}
