using System.Collections.Generic;
using System.Windows.Media;

namespace MilkBottle.Dto {
    public class BulbState {
        public  string      BulbName { get; }
        public  Color       Color { get; }

        public BulbState( string bulbName, Color color ) {
            BulbName = bulbName;
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
