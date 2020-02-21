namespace ReusableBits.Platform {
    public static class NoiseIpcSubject {
        public const string cCompanionApplication = "companionApplication";
        public const string cActivateApplication = "activateApplication";
    }

    public class CompanionApplication {
        public  string  Name { get; }
        public  string  Icon { get; }

        public CompanionApplication( string name, string icon ) {
            Name = name;
            Icon = icon;
        }
    }

    public class ActivateApplication { }
}
