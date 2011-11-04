namespace DiscogsConnect
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class DiscogsClient
    {
        HttpClient client;

        public DiscogsClient() : this("http://api.discogs.com")
        {
        }

        public DiscogsClient(string baseAddress)
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(baseAddress);
            client.MaxResponseContentBufferSize = int.MaxValue; 
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        }

        public System.Drawing.Bitmap GetImage(string imageUri)
        {
            var responseMessage = client.Get(imageUri);
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            responseMessage.EnsureSuccessStatusCode();

            var bitmap = new Bitmap(responseMessage.Content.ContentReadStream);
            return bitmap;
        }

        public Release SearchRelease(int id)
        {
            var urlBuilder = new UrlBuilder(client.BaseAddress);
            urlBuilder.Path = string.Format("release/{0}", id);
            
            var responseMessage = client.Get(urlBuilder.ToString());
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            responseMessage.EnsureSuccessStatusCode();

            var response = Deserialize<ReleaseResponse>(responseMessage);
            response.EnsureStatusAndVersion();

            return response.Release;
        }

        public MasterRelease SearchMasterRelease(int id)
        {
            var urlBuilder = new UrlBuilder(client.BaseAddress);
            urlBuilder.Path = string.Format("master/{0}", id);

            var responseMessage = client.Get(urlBuilder.ToString());
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            responseMessage.EnsureSuccessStatusCode();
            
            var response = Deserialize<MasterReleaseResponse>(responseMessage);
            response.EnsureStatusAndVersion();

            return response.Master;
        }

        public Artist SearchArtist(string artist, bool includeReleases = false)
        {            
            var urlBuilder = new UrlBuilder(client.BaseAddress);            
            urlBuilder.Path = string.Format("artist/{0}", artist);
            
            if (includeReleases)
                urlBuilder.QueryString["releases"] = "1";

            var responseMessage = client.Get(urlBuilder.ToString());
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            responseMessage.EnsureSuccessStatusCode();
            
            var response = Deserialize<ArtistResponse>(responseMessage);
            response.EnsureStatusAndVersion();

            return response.Artist;
        }

        public Label SearchLabel(string label)
        {
            var urlBuilder = new UrlBuilder(client.BaseAddress);
            urlBuilder.Path = string.Format("label/{0}", label);
            
            var responseMessage = client.Get(urlBuilder.ToString());
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            responseMessage.EnsureSuccessStatusCode();

            var response = Deserialize<LabelResponse>(responseMessage);
            response.EnsureStatusAndVersion();

            return response.Label;
        }

        public Search Search(string searchString, SearchType searchType = SearchType.All, uint pageNumber = 1)
        {
            var urlBuilder = new UrlBuilder(client.BaseAddress);
            urlBuilder.Path = "search";
            urlBuilder.QueryString["q"] = searchString;

            if (searchType != SearchType.All)
                urlBuilder.QueryString["type"] = searchType.ToString().ToLower();

            if (pageNumber > 1)
                urlBuilder.QueryString["page"] = pageNumber.ToString();

            var responseMessage = client.Get(urlBuilder.ToString());
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            responseMessage.EnsureSuccessStatusCode();

            var response = Deserialize<SearchResponse>(responseMessage);
            response.EnsureStatusAndVersion();

            return response.Search;
        }

        static T Deserialize<T>(HttpResponseMessage responseMessage)
        {            
            var responseString = responseMessage.Content.ReadAsString();
            //var formatted = Format(responseString).ToString();
            return JsonSerializer.Current.DeserializeObject<T>(responseString);
        }

        static string Format(string json)
        {                        
            var jsonObject = JObject.Parse(json);
            using (StringWriter sw = new StringWriter(CultureInfo.InvariantCulture))
            {
                var textWriter = new JsonTextWriter(sw)
                {
                    Formatting = Formatting.Indented
                };

                jsonObject.WriteTo(textWriter);
                return sw.ToString();
            }                                                
        }
    }    

    public enum SearchType
    {
        All,
        Releases,
        Artists,
        Labels
    }
}
