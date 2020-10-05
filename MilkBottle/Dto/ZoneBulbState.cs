using System.Collections.Generic;
using System.Windows.Media;
using HueLighting.Dto;

namespace MilkBottle.Dto {
    public class BulbState {
        public  Bulb        Bulb { get; }
        public  Color       Color { get; }

        public BulbState( Bulb bulb, Color color ) {
            Bulb = bulb;
            Color = color;
        }
    }

    public class ZoneBulbState {
        public  string          ZoneName { get; }
        public  List<BulbState> BulbStates { get; }
        public  long            ProcessingTime { get; private set; }

        public ZoneBulbState( string zoneName ) {
            ZoneName = zoneName;

            BulbStates = new List<BulbState>();
        }

        public void SetProcessingTime( long milliseconds ) {
            ProcessingTime = milliseconds;
        }
    }
}
