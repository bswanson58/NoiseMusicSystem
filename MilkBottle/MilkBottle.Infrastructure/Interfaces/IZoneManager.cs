using System.Collections.Generic;
using MilkBottle.Infrastructure.Dto;

namespace MilkBottle.Infrastructure.Interfaces {
    public interface IZoneManager {
        IEnumerable<ZoneGroup>  GetZones();
        ZoneGroup               GetZone( string groupId );

        ZoneGroup               CreateZone( string zoneName );
        void                    UpdateZone( ZoneGroup zone );
        void                    DeleteZone( string groupId );

        void                    SetCurrentGroup( string groupId );
        ZoneGroup               GetCurrentGroup();
    }
}
