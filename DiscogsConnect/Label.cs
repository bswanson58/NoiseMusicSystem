namespace DiscogsConnect
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class LabelResponse : DiscogsResponse
    {
        [JsonProperty("label")]
        public Label Label { get; set; }
    }

    public class Label : ValueObject<Label>
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("parentLabel")]
        public string ParentLabel { get; set; }

        [JsonProperty("contactInfo")]
        public string ContactInfo { get; set; }

        [JsonProperty("images")]
        public List<Image> Images { get; set; }

        [JsonProperty("sublabels")]
        public List<string> Sublabels { get; set; }
    }
}
