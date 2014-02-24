using System.Threading.Tasks;

namespace Common.Microsoft
{
    public abstract class LiveAccessToken
    {
        public abstract Task<string> GetAccessToken();
    }
}