using Common.Compression;
using System.IO;
using System.IO.Compression;

namespace Common.Windows.Compression
{
    public class DeflateHandler : CompressionHandler
    {
        public DeflateHandler()
        {
            SupportedEncodings.Add("deflate");
        }

        public override Stream Compress(Stream inputStream)
        {
            return new DeflateStream(inputStream, CompressionMode.Compress, leaveOpen: true);
        }

        public override Stream Decompress(Stream inputStream)
        {
            return new DeflateStream(inputStream, CompressionMode.Decompress, leaveOpen: false);
        }
    }
}
