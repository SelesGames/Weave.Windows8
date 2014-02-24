using System.Threading.Tasks;

namespace Common.Microsoft.OneNote
{
    public class OneNoteServiceClient : OneNoteServiceClientBase
    {
        string accessToken;

        public OneNoteServiceClient(string accessToken)
        {
            this.accessToken = accessToken;
        }

        protected override Task<string> GetAccessToken()
        {
            return Task.FromResult(accessToken);
        }
    }
}
