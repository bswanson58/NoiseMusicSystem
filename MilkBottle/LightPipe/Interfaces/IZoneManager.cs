using System.Collections.Generic;
using LightPipe.Dto;

namespace LightPipe.Interfaces {
    public interface IZoneManager {
        IEnumerable<ZoneGroup>  GetZones();
        ZoneGroup               GetZone( string groupId );

        ZoneGroup               CreateZone( string zoneName );
        void                    UpdateZone( ZoneGroup zone );
        void                    DeleteZone( string groupId );
    }
}
