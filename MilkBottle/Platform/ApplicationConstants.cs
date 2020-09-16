using MilkBottle.Infrastructure.Interfaces;

namespace MilkBottle.Platform {
    class ApplicationConstants : IApplicationConstants {
        public  string  ApplicationName => "MilkBottle";
        public  string  CompanyName => "Secret Squirrel Software";
        public  string	EcosystemName => "Noise Music System";

        public  string  ConfigurationDirectory => "Configuration";

        public  string  DatabaseDirectory => "Database";
        public  string  DatabaseFileName => "MilkBottle.db";
        public  string  LogFileDirectory => "Logs";

        public  string  MilkLibraryFolder => "Milk Preset Library";
        public  string  MilkTextureFolder => "Milk Textures";

        public  string  PresetSidecarExtension => ".mbsc";
    }
}
