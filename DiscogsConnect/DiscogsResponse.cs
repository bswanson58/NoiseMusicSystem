namespace DiscogsConnect
{
    using Newtonsoft.Json;

    public class DiscogsResponse
    {
        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        public DiscogsResponse EnsureStatusAndVersion()
        {
            if (!Status)
                throw new DiscogsException("");

            if (Version != "2.0")
                throw new DiscogsException("");

            return this;
        }
    }
}
