using System;
using Newtonsoft.Json;

// ReSharper disable once IdentifierTypo

namespace Noise.Hass.Dto {
    public class CommandDto {
        [JsonProperty( PropertyName = "command")]
        public  string      Command { get; set; }

        [JsonProperty( PropertyName = "parameter")]
        public  string      Parameter { get; set; }

        public CommandDto() {
            Command = String.Empty;
            Parameter = String.Empty;
        }
    }
}
