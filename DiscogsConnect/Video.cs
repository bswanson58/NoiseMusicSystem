namespace DiscogsConnect
{
    using Newtonsoft.Json;

    public class Video : ValueObject<Video>
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("embed")]
        public bool Embed { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
