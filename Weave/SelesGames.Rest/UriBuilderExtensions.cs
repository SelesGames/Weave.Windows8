using System;
using System.Net;

namespace SelesGames.Rest
{
    public static class UriBuilderExtensions
    {
        //public static void AppendQuery(this UriBuilder uri, bool useEncoding, string queryFormat, params object[] args)
        //{
        //    var encodedQuery = useEncoding ?
        //        HttpUtility.UrlEncode(string.Format(queryFormat, args))
        //        :
        //        string.Format(queryFormat, args);

        //    if (uri.Query != null && uri.Query.Length > 1)
        //        uri.Query = uri.Query.Substring(1) + "&" + encodedQuery;
        //    else
        //        uri.Query = encodedQuery;
        //}

        //public static void AppendQuery(this UriBuilder uri, string queryFormat, params object[] args)
        //{
        //    uri.AppendQuery(true, queryFormat, args);
        //}

        public static UriBuilder AddParameter(this UriBuilder uri, string parameterName, object parameterValue)
        {
            var encodedQuery = string.Format("{0}={1}", parameterName, WebUtility.UrlEncode(parameterValue.ToString()));

            if (uri.Query != null && uri.Query.Length > 1)
                uri.Query = uri.Query.Substring(1) + "&" + encodedQuery;
            else
                uri.Query = encodedQuery;

            return uri;
        }
    }
}
