using System.Windows.Media;
using MilkBottle.Infrastructure.Dto;

namespace LightPipe.Dto {
    public class UiZoneDefinition {
        public  ZoneDefinition  ZoneDefinition { get; }
        public  Color           LegendColor { get; }

        public  float           Top => ZoneDefinition.ZoneArea.Top;
        public  float           Left => ZoneDefinition.ZoneArea.Left;
        public  float           Right => ZoneDefinition.ZoneArea.Right;
        public  float           Bottom => ZoneDefinition.ZoneArea.Bottom;

        public  float           Height => ZoneDefinition.ZoneArea.Height;
        public  float           Width => ZoneDefinition.ZoneArea.Width;

        public  string          Name => ZoneDefinition.ZoneName;
        public  string          Location => ZoneDefinition.LightLocation.ToString();
        public  string          AreaDescription => $"(Top: {ZoneDefinition.ZoneArea.Top:N0}, Left:{ZoneDefinition.ZoneArea.Left:N0}, Bottom:{ZoneDefinition.ZoneArea.Bottom:N0}, Right:{ZoneDefinition.ZoneArea.Right:N0})";
        public  string          Description => $"{Name} - Controls: {ZoneDefinition.LightLocation}";

        public UiZoneDefinition( ZoneDefinition zone, Color color ) {
            ZoneDefinition = zone;

            LegendColor = color;
        }
    }
}
