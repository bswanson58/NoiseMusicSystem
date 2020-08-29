using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MilkBottle.Infrastructure.Dto;
using MilkBottle.Infrastructure.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace LightPipe.ViewModels {
    class ZoneSelectionViewModel : PropertyChangeBase {
        private readonly IZoneManager   mZoneManager;
        private ZoneGroup               mCurrentZone;

        public  ObservableCollection<ZoneGroup> Zones { get; }
        public  List<ZoneDefinition>            ZoneList => CurrentZone?.Zones;

        public ZoneSelectionViewModel( IZoneManager zoneManager ) {
            mZoneManager = zoneManager;

            Zones = new ObservableCollection<ZoneGroup>();

            LoadZones();
        }

        public ZoneGroup CurrentZone {
            get => mCurrentZone;
            set {
                mCurrentZone = value;
                mZoneManager.SetCurrentGroup( CurrentZone.GroupId );

                RaisePropertyChanged( () => CurrentZone );
                RaisePropertyChanged( () => ZoneList );
            }
        }

        private void LoadZones() {
            Zones.Clear();
            Zones.AddRange( mZoneManager.GetZones());

            var currentGroup = mZoneManager.GetCurrentGroup();

            CurrentZone = Zones.FirstOrDefault( z => z.GroupId.Equals( currentGroup.GroupId )) ??
                          Zones.FirstOrDefault();
        }
    }
}
