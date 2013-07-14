using System;
using System.Net;

namespace SelesGames.Rest
{
    public static class UriBuilderExtensions
    {
        public static UriBuilder AddParameter(this UriBuilder uri, string parameterName, object parameterValue)
        {
            var encodedQuery = string.Format("{0}={1}", parameterName, WebUtility.UrlEncode(parameterValue.ToString()));

            if (uri.Query != null && uri.Query.Length > 1)
                uri.Query = uri.Query.Substring(1) + "&" + encodedQuery;
            else
                uri.Query = encodedQuery;

            return uri;
        }

        public static UriBuilder AddCacheBuster(this UriBuilder uri, string cacheBusterParameter = "xzpmqr")
        {
            return uri.AddParameter(cacheBusterParameter, Guid.NewGuid());
        }
    }
}
