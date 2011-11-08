namespace DiscogsConnect
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class Image : ValueObject<Image>
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("uri150")]
        public string Uri150 { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ImageType Type { get; set; }
    }   
}
