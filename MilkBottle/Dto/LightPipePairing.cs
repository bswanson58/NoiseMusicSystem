using System;
using System.Diagnostics;

namespace MilkBottle.Dto {
    [DebuggerDisplay("Pairing: {" + nameof( PairingName ) + "}")]
    public class LightPipePairing {
        public  string  PairingId { get; set; }
        public  string  PairingName { get; set; }
        public  string  EntertainmentGroupId { get; set; }
        public  string  ZoneGroupId {  get; set; }

        public LightPipePairing() {
            PairingId = Guid.NewGuid().ToString();
        }

        public LightPipePairing( string pairingName, string entertainmentGroupId, string zoneGroupId ) :
            this() {
            PairingName = pairingName;
            EntertainmentGroupId = entertainmentGroupId;
            ZoneGroupId = zoneGroupId;
        }
    }
}
