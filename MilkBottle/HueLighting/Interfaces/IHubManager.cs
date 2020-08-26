using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using HueLighting.Dto;
using Q42.HueApi.Models.Bridge;

namespace HueLighting.Interfaces {
    public interface IHubManager {
        Task<bool>                          InitializeHub();
        Task<IEnumerable<LocatedBridge>>    LocateHubs();
        void                                EmulateHub();

        Task<IEnumerable<Bulb>>             BulbList();

        Task<bool>                          SetBulbState( String bulbId, bool state );
        Task<bool>                          SetBulbState( IEnumerable<string> bulbList, bool state );
        Task<bool>                          SetBulbState( String bulbId, Color color );
        Task<bool>                          SetBulbState( IEnumerable<string> bulbList, Color color );
        Task<bool>                          SetBulbState( String bulbId, int brightness );
        Task<bool>                          SetBulbState( IEnumerable<string> bulbList, int brightness );
    }
}
