
namespace Common.Microsoft.OneNote.Response
{
    /// <summary>
    /// Class representing a successful create call from the service
    /// </summary>
    public class CreateSuccessResponse : BaseResponse
    {
        /// <summary>
        /// URL to launch OneNote rich client
        /// </summary>
        public string OneNoteClientUrl { get; set; }

        /// <summary>
        /// URL to launch OneNote web experience
        /// </summary>
        public string OneNoteWebUrl { get; set; }
    }
}
