using LiteDB;

namespace Noise.Metadata.Interfaces {
    public interface IDatabaseProvider {
        LiteDatabase    GetDatabase();
        void            Shutdown();
    }
}
