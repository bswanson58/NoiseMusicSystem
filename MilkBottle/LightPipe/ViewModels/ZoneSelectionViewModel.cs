using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using LightPipe.Dto;
using MilkBottle.Infrastructure.Dto;
using MilkBottle.Infrastructure.Interfaces;
using ReusableBits.Mvvm.ViewModelSupport;

namespace LightPipe.ViewModels {
    class ZoneSelectionViewModel : PropertyChangeBase {
        private readonly IZoneManager                   mZoneManager;
        private readonly List<Color>                    mLegendColors;
        private ZoneGroup                               mCurrentZone;

        public  ObservableCollection<ZoneGroup>         Zones { get; }
        public  ObservableCollection<UiZoneDefinition>  ZoneList {get; }
        public  ZoneDisplayViewModel                    ZoneDisplay { get; }

        public ZoneSelectionViewModel( IZoneManager zoneManager, ZoneDisplayViewModel zoneDisplayViewModel ) {
            mZoneManager = zoneManager;
            ZoneDisplay = zoneDisplayViewModel;

            Zones = new ObservableCollection<ZoneGroup>();
            ZoneList = new ObservableCollection<UiZoneDefinition>();

            mLegendColors = new List<Color> { Colors.OrangeRed, Colors.LimeGreen, Colors.CornflowerBlue, Colors.Goldenrod,   Colors.BlueViolet };
            LoadZones();
        }

        public ZoneGroup CurrentZone {
            get => mCurrentZone;
            set {
                mCurrentZone = value;
                mZoneManager.SetCurrentGroup( CurrentZone.GroupId );

                ZoneList.Clear();
                mCurrentZone.Zones.ForEach( z => {
                    ZoneList.Add( new UiZoneDefinition( z, mLegendColors[ZoneList.Count % mLegendColors.Count]));
                });
                ZoneDisplay.SetZones( ZoneList );

                RaisePropertyChanged( () => CurrentZone );
                RaisePropertyChanged( () => ZoneList );
            }
        }

        private void LoadZones() {
            Zones.Clear();
            Zones.AddRange( mZoneManager.GetZones());

            var currentGroup = mZoneManager.GetCurrentGroup();

            CurrentZone = Zones.FirstOrDefault( z => z.GroupId.Equals( currentGroup?.GroupId )) ??
                          Zones.FirstOrDefault();
        }
    }
}
