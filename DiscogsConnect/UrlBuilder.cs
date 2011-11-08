namespace DiscogsConnect
{
    using System;
    using System.Collections.Specialized;
    using System.Text;
    using System.Web;

    internal class UrlBuilder : UriBuilder
    { 
        NameValueCollection _queryParams;
        
        public NameValueCollection QueryString
        {
            get
            {
                if (_queryParams == null)                
                    _queryParams = new NameValueCollection();
                
                return _queryParams;
            }
        }
        
        public string PageName
        {
            get
            {
                string path = base.Path;
                return path.Substring(path.LastIndexOf("/") + 1);
            }
            set
            {
                string path = base.Path;
                path = path.Substring(0, path.LastIndexOf("/"));
                base.Path = string.Concat(path, "/", value);
            }
        }
        
        public UrlBuilder() : base()
        {
        }
        
        public UrlBuilder(System.Web.HttpRequest request) : base(request.Url.AbsoluteUri)
        {
            UpdateQueryParams();
        }
        
        public UrlBuilder(string uri) : base(uri)
        {
            UpdateQueryParams();
        }

        public UrlBuilder(Uri uri) : base(uri)
        {
            UpdateQueryParams();
        }
        
        public UrlBuilder(string schemeName, string hostName) : base(schemeName, hostName)
        {
        }
        
        public UrlBuilder(string scheme, string host, int portNumber) : base(scheme, host, portNumber)
        {
        }

        public UrlBuilder(string scheme, string host, int port, string pathValue) : base(scheme, host, port, pathValue)
        {
        }

        public UrlBuilder(string scheme, string host, int port, string path, string extraValue) : base(scheme, host, port, path, extraValue)
        {
        }
        
        public new string ToString()
        {
            UpdateQueryString();
            return base.Uri.AbsoluteUri;
        }
                       
        private void UpdateQueryParams()
        {
            string query = base.Query;

            if (_queryParams == null)
                _queryParams = new NameValueCollection();
            else
                _queryParams.Clear();

            if (string.IsNullOrEmpty(query))
                return;

            // Start after the '?'.
            query = query.Substring(1);

            string[] pairs = query.Split(new char[] { '&' });
            foreach (string s in pairs)
            {
                string[] pair = s.Split(new char[] { '=' });
                string key = HttpUtility.UrlDecode(pair[0]);
                string value = HttpUtility.UrlDecode((pair.Length > 1) ? pair[1] : string.Empty);
                _queryParams.Add(key, value);
            }
        }
        
        private void UpdateQueryString()
        {
            int count = _queryParams.Count;

            if (count == 0)
            {
                base.Query = string.Empty;
            }
            else
            {
                StringBuilder query = new StringBuilder();
                for (int keyIndex = 0; keyIndex < count; keyIndex++)
                {
                    if (keyIndex > 0)
                        query.Append('&');
                    string key = HttpUtility.UrlEncode(_queryParams.GetKey(keyIndex));
                    query.Append(key);

                    // There are 0..n values per key possible.
                    string[] values = _queryParams.GetValues(keyIndex);
                    int currValueIndex = 0;
                    foreach (string value in values)
                    {
                        query.Append('=');
                        query.Append(HttpUtility.UrlEncode(value));
                        if (++currValueIndex < values.Length)
                        {
                            query.Append('&');
                            query.Append(key);
                        }
                    }
                }
                base.Query = query.ToString();
            }
        }        
    }
}
