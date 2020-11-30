using LiteDB;

namespace Noise.Metadata.Interfaces {
    public interface IDatabaseProvider {
        LiteDatabase    GetDatabase();
        void            Shutdown();
        
        void            ExportMetadata( string exportPath );
        void            ImportMetadata( string importPath );
    }
}
