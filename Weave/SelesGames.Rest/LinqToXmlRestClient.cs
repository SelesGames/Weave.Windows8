using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SelesGames.Rest
{
    public class LinqToXmlRestClient<T> : BaseRestClient
    {
        public Task<T> GetAndParseAsync(string url, Func<XElement, T> parser, CancellationToken cancellationToken)
        {
            var client = new DelegateRestClient(stream =>
            {
                var xml = XElement.Load(stream);
                return parser(xml);
            }) 
            { 
                UseGzip = UseGzip 
            };
            return client.GetAsync<T>(url, cancellationToken);
        }
    }
}
