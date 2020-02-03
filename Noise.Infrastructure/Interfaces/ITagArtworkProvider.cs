namespace Noise.Infrastructure.Interfaces {
    public interface ITagArtworkProvider {
        byte[]      GetArtwork( long storageFileId, string name );
    }
}
