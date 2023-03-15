using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using HueLighting.Dto;
using Q42.HueApi.Models.Groups;

namespace HueLighting.Interfaces {
    public interface IHubManager {
        Task<bool>                          InitializeConfiguredHub();
        bool                                IsInitialized { get; }

        Task<IEnumerable<HubInformation>>   LocateHubs();
        Task<IList<HubInformation>>         GetRegisteredHubs();
        Task<HubInformation>                RegisterApp( HubInformation hub, bool setAsConfiguredHub = false );
        void                                SetConfiguredHub( HubInformation hub );

        Task<IEnumerable<Bulb>>             GetBulbs();
        Task<IEnumerable<BulbGroup>>        GetBulbGroups();
        
        Task<IEnumerable<Group>>            GetEntertainmentGroups();
        Task<EntertainmentGroup>            GetEntertainmentGroupLayout( Group forGroup );
        Task<IEntertainmentGroupManager>    StartEntertainmentGroup();
        Task<IEntertainmentGroupManager>    StartEntertainmentGroup( Group forGroup );
        
        void                                EmulateHub();

        Task<bool>                          SetBulbState( Bulb bulb, bool state );
        Task<bool>                          SetBulbState( Bulb bulb, int brightness );
        Task<bool>                          SetBulbState( Bulb bulb, double brightness );
        Task<bool>                          SetBulbState( Bulb bulb, Color color, TimeSpan? transitionTime = null );
        Task<bool>                          SetBulbState( Bulb bulb, Color color, double brightness, TimeSpan transitionTime );

        Task<bool>                          SetBulbState( IEnumerable<Bulb> bulbList, bool state );
        Task<bool>                          SetBulbState( IEnumerable<Bulb> bulbList, int brightness );
        Task<bool>                          SetBulbState( IEnumerable<Bulb> bulbList, double brightness );
        Task<bool>                          SetBulbState( IEnumerable<Bulb> bulbList, Color color, TimeSpan? transitionTime = null );
        Task<bool>                          SetBulbState( IEnumerable<Bulb> bulbList, Color color, double brightness, TimeSpan transitionTime );
    }
}
