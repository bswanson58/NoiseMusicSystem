﻿﻿using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noise.Metadata.MetadataProviders.Discogs.Rto;
using Refit;

/* ******** Hey You! *********
 *
 * This is a generated file, and gets rewritten every time you build the
 * project. If you want to edit it, you need to edit the mustache template
 * in the Refit package */

namespace RefitInternalGenerated
{
    [AttributeUsage (AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate)]
    sealed class PreserveAttribute : Attribute
    {
#pragma warning disable 0649
        //
        // Fields
        //
        public bool AllMembers;

        public bool Conditional;
#pragma warning restore 0649
    }
}

namespace Noise.Metadata.MetadataProviders.Discogs
{
    using RefitInternalGenerated;

    [Preserve]
    public partial class AutoGeneratedIDiscogsApi : IDiscogsApi
    {
        public HttpClient Client { get; protected set; }
        readonly Dictionary<string, Func<HttpClient, object[], object>> methodImpls;

        public AutoGeneratedIDiscogsApi(HttpClient client, IRequestBuilder requestBuilder)
        {
            methodImpls = requestBuilder.InterfaceHttpMethods.ToDictionary(k => k, v => requestBuilder.BuildRestResultFuncForMethod(v));
            Client = client;
        }

        public virtual Task<DiscogsSearchResult> Search(string query,string searchType,string token)
        {
            var arguments = new object[] { query,searchType,token };
            return (Task<DiscogsSearchResult>) methodImpls["Search"](Client, arguments);
        }

        public virtual Task<DiscogsArtist> GetArtist(string artistId,string token)
        {
            var arguments = new object[] { artistId,token };
            return (Task<DiscogsArtist>) methodImpls["GetArtist"](Client, arguments);
        }

        public virtual Task<DiscogsArtistReleases> GetArtistReleases(string artistId,string token)
        {
            var arguments = new object[] { artistId,token };
            return (Task<DiscogsArtistReleases>) methodImpls["GetArtistReleases"](Client, arguments);
        }

    }
}


