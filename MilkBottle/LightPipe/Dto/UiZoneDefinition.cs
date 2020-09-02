using System.Windows.Media;
using MilkBottle.Infrastructure.Dto;

namespace LightPipe.Dto {
    public class UiZoneDefinition {
        private readonly ZoneDefinition mZoneDefinition;

        public  float   Top => mZoneDefinition.ZoneArea.Top;
        public  float   Left => mZoneDefinition.ZoneArea.Left;
        public  float   Right => mZoneDefinition.ZoneArea.Right;
        public  float   Bottom => mZoneDefinition.ZoneArea.Bottom;

        public  float   Height => mZoneDefinition.ZoneArea.Height;
        public  float   Width => mZoneDefinition.ZoneArea.Width;

        public  string  Name => mZoneDefinition.ZoneName;
        public  string  Location => mZoneDefinition.LightLocation.ToString();
        public  string  AreaDescription => $"(Top: {mZoneDefinition.ZoneArea.Top:N0}, Left:{mZoneDefinition.ZoneArea.Left:N0}, Bottom:{mZoneDefinition.ZoneArea.Bottom:N0}, Right:{mZoneDefinition.ZoneArea.Right:N0})";
        public  string  Description => $"{Name} - Controls: {mZoneDefinition.LightLocation}";
        public  Color   LegendColor { get; }

        public UiZoneDefinition( ZoneDefinition zone, Color color ) {
            mZoneDefinition = zone;

            LegendColor = color;
        }
    }
    
}
