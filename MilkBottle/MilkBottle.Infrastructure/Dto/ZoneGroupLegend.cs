using System.Collections.Generic;
using System.Windows.Media;

namespace MilkBottle.Infrastructure.Dto {
    public class ZoneLegend {
        public  GroupLightLocation  Location { get; }
        public  Color               ZoneColor { get; }

        public ZoneLegend( GroupLightLocation location, Color color ) {
            Location = location;
            ZoneColor = color;
        }
    }

    public class ZoneGroupLegend {
        public  string              GroupId { get; }
        public  List<ZoneLegend>    Zones { get; }

        public ZoneGroupLegend( string groupId, IEnumerable<ZoneLegend> zoneLegends ) {
            GroupId = groupId;
            Zones = new List<ZoneLegend>( zoneLegends );
        }
    }
}
