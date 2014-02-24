using Common.Microsoft;
using Common.Microsoft.OneNote;
using Common.Microsoft.OneNote.Response;
using System.Threading.Tasks;

namespace Weave.Microsoft.OneNote
{
    public static class OneNoteItemExtensions
    {
        public static async Task<BaseResponse> SendToOneNote(this HtmlLinkOneNoteItem oneNoteItem, LiveAccessToken token)
        {
            var html = await new Formatter().CreateHtml(oneNoteItem).ConfigureAwait(false);
            return await CreateClient(token).CreateSimple(html);
        }

        public static async Task<BaseResponse> SendToOneNote(this HtmlLinkOneNoteItem oneNoteItem, string token)
        {
            var html = await new Formatter().CreateHtml(oneNoteItem).ConfigureAwait(false);
            return await CreateClient(token).CreateSimple(html);
        }
        
        public static async Task<BaseResponse> SendToOneNote(this MobilizedOneNoteItem oneNoteItem, LiveAccessToken token)
        {
            var html = await new Formatter().CreateHtml(oneNoteItem).ConfigureAwait(false);
            return await CreateClient(token).CreateSimple(html);
        }

        public static async Task<BaseResponse> SendToOneNote(this MobilizedOneNoteItem oneNoteItem, string token)
        {
            var html = await new Formatter().CreateHtml(oneNoteItem).ConfigureAwait(false);
            return await CreateClient(token).CreateSimple(html);
        }




        #region private helper methods

        static OneNoteServiceClientBase CreateClient(LiveAccessToken token)
        {
            return new TokenBasedOneNoteServiceClient(token);
        }

        static OneNoteServiceClientBase CreateClient(string token)
        {
            return new OneNoteServiceClient(token);
        }

        #endregion
    }
}
