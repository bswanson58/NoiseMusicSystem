using System;
using System.Threading.Tasks;
using System.Windows.Media;
using HueLighting.Dto;
using Q42.HueApi.Models.Groups;

namespace HueLighting.Interfaces {
    public interface IEntertainmentGroupManager : IDisposable {
        Group                       EntertainmentGroup { get; }

        Task<EntertainmentGroup>    GetGroupLayout();

        double                      OverallBrightness { get; set; }

        void                        SetLightColor( string lightId, Color toColor );
        void                        UpdateLights();
    }
}
