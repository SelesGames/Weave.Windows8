using System;
using System.IO;

namespace SelesGames.Rest
{
    public class DelegateRestClient : RestClient
    {
        Func<Stream, object> map;

        public DelegateRestClient(Func<Stream, object> map)
        {
            this.map = map;
        }

        protected override T ReadObject<T>(Stream stream)
        {
            return (T)map(stream);
        }

        protected override void WriteObject<T>(Stream writeStream, T obj)
        {
            throw new NotImplementedException();
        }
    }

    //public static class RestClient
    //{
    //    public static DelegateRestClient<T> Create<T>(Func<Stream, T> map, bool useGzip = false)
    //    {
    //        return new DelegateRestClient<T>(map) { UseGzip = useGzip };
    //    }
    //}
}
