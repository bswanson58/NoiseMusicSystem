using System.Collections.ObjectModel;
using System.Linq;
using MilkBottle.Infrastructure.Dto;

namespace LightPipe.ViewModels {
    public class UiZoneDefinition {
        private readonly ZoneDefinition mZoneDefinition;

        public  float   Top => mZoneDefinition.ZoneArea.Top;
        public  float   Left => mZoneDefinition.ZoneArea.Left;
        public  float   Right => mZoneDefinition.ZoneArea.Right;
        public  float   Bottom => mZoneDefinition.ZoneArea.Bottom;

        public  float   Height => mZoneDefinition.ZoneArea.Height;
        public  float   Width => mZoneDefinition.ZoneArea.Width;

        public  string  Name => mZoneDefinition.ZoneName;

        public UiZoneDefinition( ZoneDefinition zone ) {
            mZoneDefinition = zone;
        }
    }
    
    public class ZoneDisplayViewModel {
        public ObservableCollection<UiZoneDefinition>   Zones { get; }

        public ZoneDisplayViewModel() {
            Zones = new ObservableCollection<UiZoneDefinition>();
        }

        public void SetZones( ZoneGroup zoneGroup ) {
            Zones.Clear();
            Zones.AddRange( from z in zoneGroup.Zones select new UiZoneDefinition( z ));
        }
    }
}
