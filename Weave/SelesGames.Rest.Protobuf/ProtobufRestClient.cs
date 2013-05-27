
namespace SelesGames.Rest.Protobuf
{
    public class ProtobufRestClient : RestClient
    {
        public ProtobufRestClient()
        {
            Headers.Accept = "application/protobuf";
            Headers.ContentType = "application/protobuf";
        }

        protected override T ReadObject<T>(System.IO.Stream readStream)
        {
            return ProtoBuf.Serializer.Deserialize<T>(readStream);
        }

        protected override void WriteObject<T>(System.IO.Stream writeStream, T obj)
        {
            ProtoBuf.Serializer.Serialize(writeStream, obj);
        }
    }
}
