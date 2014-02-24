using System.Net;

namespace Common.Microsoft.OneNote.Response
{
    /// <summary>
    /// Class representing standard error from the service
    /// </summary>
    public class StandardErrorResponse : BaseResponse
    {
        /// <summary>
        /// Error message - intended for developer, not end user
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public StandardErrorResponse()
        {
            this.StatusCode = HttpStatusCode.InternalServerError;
        }
    }
}
