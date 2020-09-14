using System.Collections.Generic;
using MilkBottle.Dto;

namespace MilkBottle.Interfaces {
    interface IPairingManager {
        IEnumerable<LightPipePairing>   GetPairings();

        void                            DeletePairing( LightPipePairing pairing );
        LightPipePairing                AddPairing( string pairName, string entertainmentGroupId, string zoneGroupId );

        void                            SetCurrentPairing( LightPipePairing pairing );
        LightPipePairing                GetCurrentPairing();
    }
}
