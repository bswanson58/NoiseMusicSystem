using System.Threading.Tasks;
using System.Windows.Media;
using HueLighting.Dto;
using Q42.HueApi.Models.Groups;

namespace HueLighting.Interfaces {
    public interface IEntertainmentGroupManager {
        Group                       EntertainmentGroup { get; }

        Task<EntertainmentGroup>    GetGroupLayout();

        void                        SetOverallBrightness( double brightness );

        void                        SetLightColor( string lightId, Color toColor );
        void                        UpdateLights();
    }
}
