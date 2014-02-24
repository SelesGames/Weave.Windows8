using System.Threading.Tasks;

namespace Common.Microsoft
{
    public class LiveTransientAccessToken : LiveAccessToken
    {
        public string AccessToken { get; private set; }

        public LiveTransientAccessToken(string accessToken)
        {
            this.AccessToken = accessToken;
        }

        public override Task<string> GetAccessToken()
        {
            return Task.FromResult(AccessToken);
        }
    }
}
