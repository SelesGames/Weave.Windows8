
namespace Common.Windows.Compression
{
    public class CompressionHandlerCollection : Common.Compression.CompressionHandlerCollection
    {
        public CompressionHandlerCollection()
        {
            this.Add(new GZipHandler());
            this.Add(new DeflateHandler());
        }
    }
}
