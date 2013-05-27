using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SelesGames.Rest
{
    public abstract class RestClient : BaseRestClient
    {
        public RestClient()
        {
            Headers = new Headers();
        }

        public Task GetAsync(string url, CancellationToken cancellationToken)
        {
            var request = HttpWebRequest.CreateHttp(url);
            return request.GetResponseAsync();
        }

        public Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
        {
            var request = HttpWebRequest.CreateHttp(url);

            if (!string.IsNullOrEmpty(Headers.Accept))
                request.Accept = Headers.Accept;

            if (UseGzip)
                request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";

            return request
                .GetResponseAsync()
                .ContinueWith(
                    task =>
                    {
                        var response = (HttpWebResponse)task.Result;
                        if (response.StatusCode == HttpStatusCode.OK)
                            return ReadObjectFromWebResponse<T>(response);
                        else
                            return default(T);
                        
                        throw new WebException(string.Format("Status code: {0}", response.StatusCode), null, WebExceptionStatus.UnknownError, response);
                    },
                    cancellationToken,
                    TaskContinuationOptions.OnlyOnRanToCompletion, 
                    TaskScheduler.Default
                );
        }

        public async Task<TResult> PostAsync<TPost, TResult>(string url, TPost obj, CancellationToken cancelToken)
        {
            var request = HttpWebRequest.CreateHttp(url);
            request.Method = "POST";

            if (!string.IsNullOrEmpty(Headers.ContentType))
                request.ContentType = Headers.ContentType;

            if (!string.IsNullOrEmpty(Headers.Accept))
                request.Accept = Headers.Accept;

            if (UseGzip)
                request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";

            using (var requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false))
            {
                cancelToken.ThrowIfCancellationRequested();
                WriteObject(requestStream, obj);
            }

            var response = await request.GetResponseAsync().ConfigureAwait(false);

            cancelToken.ThrowIfCancellationRequested();

            return ReadObjectFromWebResponse<TResult>((HttpWebResponse)response);
        }

        public async Task<bool> PostAsync<TPost>(string url, TPost obj, CancellationToken cancelToken)
        {
            var request = HttpWebRequest.CreateHttp(url);
            request.Method = "POST";

            if (!string.IsNullOrEmpty(Headers.ContentType))
                request.ContentType = Headers.ContentType;

            if (!string.IsNullOrEmpty(Headers.Accept))
                request.Accept = Headers.Accept;

            if (UseGzip)
                request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip";

            using (var requestStream = await request.GetRequestStreamAsync().ConfigureAwait(false))
            {
                cancelToken.ThrowIfCancellationRequested();
                WriteObject(requestStream, obj);
            }

            var response = await request.GetResponseAsync().ConfigureAwait(false);
            var httpResponse = (HttpWebResponse)response;
            return httpResponse.StatusCode == HttpStatusCode.Created;        
        }




        #region helper methods

        T ReadObjectFromWebResponse<T>(HttpWebResponse response)
        {
            T result;

            using (var stream = response.GetResponseStream())
            {
                var contentEncoding = response.Headers["Content-Encoding"];
                if (UseGzip || "gzip".Equals(contentEncoding, StringComparison.OrdinalIgnoreCase))
                {
                    using (var gzip = new GZipStream(stream, CompressionMode.Decompress, false))
                    {
                        result = ReadObject<T>(gzip);
                    }
                }
                else
                {
                    result = ReadObject<T>(stream);
                }
            }
            return result;
        }

        #endregion




        protected abstract T ReadObject<T>(Stream readStream);
        protected abstract void WriteObject<T>(Stream writeStream, T obj);
    }
}
