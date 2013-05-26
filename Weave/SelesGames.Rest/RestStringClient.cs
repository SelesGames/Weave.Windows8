using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SelesGames.Rest
{
    public class RestStringClient : BaseRestClient
    {
        public Task<string> GetAsync(string url, CancellationToken cancellationToken)
        {
            var client = new DelegateRestClient(stream =>
            {
                using (var reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    return result;
                }
            }) 
            { 
                UseGzip = UseGzip 
            };
            return client.GetAsync<string>(url, cancellationToken);
        }
    }
}
