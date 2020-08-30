using System.Collections.Generic;
using System.Collections.ObjectModel;
using LightPipe.Dto;

namespace LightPipe.ViewModels {
    public class ZoneDisplayViewModel {
        public ObservableCollection<UiZoneDefinition>   Zones { get; }

        public ZoneDisplayViewModel() {
            Zones = new ObservableCollection<UiZoneDefinition>();
        }

        public void SetZones( IEnumerable<UiZoneDefinition> zones ) {
            Zones.Clear();
            Zones.AddRange( zones );
        }
    }
}
