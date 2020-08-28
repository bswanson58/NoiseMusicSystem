using System;
using System.Collections.Generic;
using System.Drawing;
using MilkBottle.Infrastructure.Dto;

namespace LightPipe.Dto {
    public class ZoneDefinition {
        public  string              ZoneName { get; set; }
        public  RectangleF          ZoneArea { get; set; }
        public  GroupLightLocation  LightLocation { get; set; }

        public ZoneDefinition( string name, RectangleF area, GroupLightLocation lightLocation ) {
            ZoneName = name;
            ZoneArea = area;
            LightLocation = lightLocation;
        }

        public bool ContainsPoint( PointF point ) {
            return ZoneArea.Contains( point );
        }
    }

    public class ZoneGroup {
        public  string                  GroupId { get; set; }
        public  string                  GroupName { get; set; }
        public  List<ZoneDefinition>    Zones { get; set; }

        public ZoneGroup() {
            GroupId = Guid.NewGuid().ToString();
            GroupName = String.Empty;
            Zones = new List<ZoneDefinition>();
        }

        public ZoneGroup( string groupName ) :
            this() {
            GroupName = groupName;
        }
    }
}
