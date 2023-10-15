using System;
using Newtonsoft.Json;
using Noise.Hass.Mqtt;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Noise.Hass.Dto {
    public class StatusDto : IJsonPayload {
        [JsonProperty( PropertyName = "artist")]
        public  string  Artist { get; set; }

        [JsonProperty( PropertyName = "album")]
        public  string  Album { get; set; }

        [JsonProperty( PropertyName = "trackname")]
        public  string  TrackName { get; set; }

        [JsonProperty( PropertyName = "tracknumber")]
        public  int     TrackNumber { get; set; }

        [JsonProperty( PropertyName = "duration")]
        public  int     Duration { get; set; }

        [JsonProperty( PropertyName = "position")]
        public  int     Position { get; set; }

        [JsonProperty( PropertyName = "positionat")]
        public  string  PositionUpdatedAt { get; set; }

        [JsonProperty( PropertyName = "playstate")]
        public  string  PlayState { get; set; }

        [JsonProperty( PropertyName = "volume")]
        public  int     Volume { get; set; }

        public StatusDto() {
            Artist = String.Empty;
            Album = String.Empty;
            TrackName = String.Empty;
            PlayState = String.Empty;
            PositionUpdatedAt = String.Empty;
            TrackNumber = 0;
            Duration = 0;
            Position = 0;
            Volume = 0;
        }
    }
}
