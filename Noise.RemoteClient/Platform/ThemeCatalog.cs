using System.Collections.Generic;
using System.Diagnostics;

namespace Noise.RemoteClient.Platform {
    [DebuggerDisplay("Theme = {" + nameof(Name) + "}")]
    class ThemeResource {
        public  int     ResourceId { get; }
        public  string  Name { get; }
        public  string  Source { get; }

        public ThemeResource( int resourceId, string name, string source ) {
            ResourceId = resourceId;
            Name = name;
            Source = source;
        }
    }

    static class ThemeCatalog {
        public static int DefaultFont => 2;

        public static IEnumerable<ThemeResource> FontThemes => new List<ThemeResource> {
            new ThemeResource( 1, "Small", "Resources/TextSizes/SmallText.xaml" ),
            new ThemeResource( 2, "Medium", "Resources/TextSizes/MediumText.xaml" ),
            new ThemeResource( 3, "Large", "Resources/TextSizes/LargeText.xaml" )
        };

        public static int DefaultTheme => 1;

        public static IEnumerable<ThemeResource> ThemeResources => new List<ThemeResource> {
            new ThemeResource( 1, "Dark Blue", "Resources/Themes/DarkBlue.xaml" ),
            new ThemeResource( 2, "Dark Indigo", "Resources/Themes/DarkIndigo.xaml" ),
        };
    }
}
