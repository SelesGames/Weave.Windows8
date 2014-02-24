using System.Threading.Tasks;

namespace Common.Microsoft.OneNote
{
    public class TokenBasedOneNoteServiceClient : OneNoteServiceClientBase
    {
        LiveAccessToken access;

        public TokenBasedOneNoteServiceClient(LiveAccessToken access)
        {
            this.access = access;
        }

        protected override Task<string> GetAccessToken()
        {
            return access.GetAccessToken();
        }
    }
}
