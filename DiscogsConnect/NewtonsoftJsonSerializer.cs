namespace DiscogsConnect
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal sealed class NewtonsoftJsonSerializer : IJsonSerializer
    {
        public T DeserializeObject<T>(string json)
        {
            var responseJObject = JObject.Parse(json);
            var responseJToken = responseJObject["resp"];

            T result = JsonConvert.DeserializeObject<T>(responseJToken.ToString());
            return result;
        }
    }
}
