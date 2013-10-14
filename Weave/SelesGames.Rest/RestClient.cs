using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
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
#if DEBUG
            Debug.WriteLine("HTTP GET : {0}", url);
#endif
            return new HttpClient().GetAsync(url, cancellationToken);
        }

        public async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
        {
#if DEBUG
            Debug.WriteLine("HTTP GET : {0}", url);
#endif
            var response = await CreateClient().GetAsync(url, cancellationToken).ConfigureAwait(false);
            return await ReadObjectFromResponseMessage<T>(response);
        }

        public async Task<TResult> PostAsync<TPost, TResult>(string url, TPost obj, CancellationToken cancelToken)
        {
#if DEBUG
            Debug.WriteLine("HTTP POST : {0}", url);
#endif
            var client = CreateClient();

            HttpResponseMessage response;

            using (var ms = new MemoryStream())
            {
                WriteObject(ms, obj);
                ms.Position = 0;

                var content = new StreamContent(ms);
                content.Headers.TryAddWithoutValidation("Content-Type", Headers.ContentType);

                response = await client.PostAsync(url, content).ConfigureAwait(false);
            }

            return await ReadObjectFromResponseMessage<TResult>(response);
        }

        public async Task PostAsync<TPost>(string url, TPost obj, CancellationToken cancelToken)
        {
#if DEBUG
            Debug.WriteLine("HTTP POST : {0}", url);
#endif
            var client = CreateClient();

            using (var ms = new MemoryStream())
            {
                WriteObject(ms, obj);
                ms.Position = 0;

                var content = new StreamContent(ms);
                content.Headers.TryAddWithoutValidation("Content-Type", Headers.ContentType);

                await client.PostAsync(url, content).ConfigureAwait(false);
            }
        }




        #region helper methods

        HttpClient CreateClient()
        {
            var client = new HttpClient();

            if (!string.IsNullOrEmpty(Headers.Accept))
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", Headers.Accept);

            if (UseGzip)
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip");

            return client;
        }

        async Task<T> ReadObjectFromResponseMessage<T>(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            T result;

            using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                var contentEncoding = response.Content.Headers.ContentEncoding.FirstOrDefault();
                if ("gzip".Equals(contentEncoding, StringComparison.OrdinalIgnoreCase))
                {
                    using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
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