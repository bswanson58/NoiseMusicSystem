using System;

namespace ReusableBits.Platform {
    public static class NoiseIpcSubject {
        public const string cCompanionApplication = "companionApplication";
        public const string cActivateApplication = "activateApplication";
    }

    public class CompanionApplication {
        public  string  Name { get; set; }
        public  string  Icon { get; set; }

        public CompanionApplication() {
            Name = String.Empty;
            Icon = String.Empty;
        }

        public CompanionApplication( string name, string icon ) {
            Name = name;
            Icon = icon;
        }
    }

    public class ActivateApplication { }
}
