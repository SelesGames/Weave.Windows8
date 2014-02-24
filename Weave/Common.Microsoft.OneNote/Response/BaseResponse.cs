using System.Net;

namespace Common.Microsoft.OneNote.Response
{
    /// <summary>
    /// Base class representing a simplified response from a service call 
    /// </summary>
    public abstract class BaseResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// Per call identifier that can be logged to diagnose issues with Microsoft support
        /// </summary>
        public string CorrelationId { get; set; }
    }
}
