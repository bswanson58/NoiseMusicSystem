using System;

namespace MilkBottle.Infrastructure.Interfaces {
    public interface IApplicationConstants {
        String  ApplicationName { get; }
        String  CompanyName { get; }
        String  EcosystemName { get; }

        String  ConfigurationDirectory { get; }
        String  DatabaseDirectory { get; }
        String  DatabaseFileName { get; }
        String  LogFileDirectory { get; }

        String  MilkLibraryFolder { get; }
        String  MilkTextureFolder { get; }
    }
}
