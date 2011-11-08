namespace DiscogsConnect
{
    using Newtonsoft.Json;

    public class Track : ValueObject<Track>
    {
        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("position")]
        public string Position { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
