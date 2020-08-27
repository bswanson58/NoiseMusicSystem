using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using HueLighting.Dto;

namespace HueLighting.Interfaces {
    public interface IHubManager {
        Task<bool>                          InitializeConfiguredHub();

        Task<IEnumerable<HubInformation>>   LocateHubs();
        Task<String>                        RegisterApp( HubInformation hub, bool setAsConfiguredHub = false );
        void                                SetConfiguredHub( HubInformation hub );
        
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
