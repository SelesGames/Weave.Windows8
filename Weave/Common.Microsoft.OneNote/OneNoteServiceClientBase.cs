using Common.Microsoft.OneNote.Response;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Common.Microsoft.OneNote
{
    public abstract class OneNoteServiceClientBase
    {
        #region Private member variables

        // v1.0 Endpoints        
        const string PAGESENDPOINT = "https://www.onenote.com/api/v1.0/pages";

        const string PRESENTATION = "Presentation";

        #endregion




        public async Task<BaseResponse> CreateSimple(string html)
        {
            var accessToken = await GetAccessToken();

            // Create the request message, which is a multipart/form-data request
            var createMessage = new HttpRequestMessage(HttpMethod.Post, PAGESENDPOINT)
            {
                Content = CreateHtmlContent(html)
            };

            var response = await CreateClient(accessToken).SendAsync(createMessage);
            return await TranslateResponse(response);
        }

        public async Task<BaseResponse> CreateWithImage(string html, string imageName, Stream imageStream)
        {
            var accessToken = await GetAccessToken();

            using (var imageContent = new StreamContent(imageStream))
            {
                imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                
                var createMessage = new HttpRequestMessage(HttpMethod.Post, PAGESENDPOINT)
                {
                    Content = 
                        new MultipartFormDataContent
                        {
                            { CreateHtmlContent(html), PRESENTATION },
                            { imageContent, imageName }
                        }
                };

                var response = await CreateClient(accessToken).SendAsync(createMessage);
                return await TranslateResponse(response);
            }
        }

        public async Task<BaseResponse> CreateWithHtml(string html, string embeddedPartName, string embeddedHtml)
        {
            var accessToken = await GetAccessToken();

            var createMessage = new HttpRequestMessage(HttpMethod.Post, PAGESENDPOINT)
            {
                Content = 
                    new MultipartFormDataContent
                    {
                        { CreateHtmlContent(html), PRESENTATION },
                        { CreateHtmlContent(embeddedHtml), embeddedPartName },
                    }
            };

            var response = await CreateClient(accessToken).SendAsync(createMessage);
            return await TranslateResponse(response);
        }

        protected abstract Task<string> GetAccessToken();




        #region Private helper functions

        HttpClient CreateClient(string accessToken)
        {
            var client = new HttpClient();
            var headers = client.DefaultRequestHeaders;

            // Note: API only supports JSON return type.
            headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // This allows you to see what happens when an unauthenticated call is made.
            if (accessToken != null)
            {
                headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return client;
        }

        StringContent CreateHtmlContent(string html)
        {
            return new StringContent(html, Encoding.UTF8, "text/html");
        }

        async static Task<BaseResponse> TranslateResponse(HttpResponseMessage response)
        {
            BaseResponse standardResponse;
            if (response.StatusCode == HttpStatusCode.Created)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseObject = JsonConvert.DeserializeObject(responseString);
                standardResponse = new CreateSuccessResponse
                {
                    StatusCode = response.StatusCode,
                    OneNoteClientUrl = responseObject.links.oneNoteClientUrl.href,
                    OneNoteWebUrl = responseObject.links.oneNoteWebUrl.href
                };
            }
            else
            {
                standardResponse = new StandardErrorResponse
                {
                    StatusCode = response.StatusCode,
                    Message = await response.Content.ReadAsStringAsync()
                };
            }

            // Extract the correlation id.  Apps should log this if they want to collect data to diagnose failures with Microsoft support 
            IEnumerable<string> correlationValues;
            if (response.Headers.TryGetValues("X-CorrelationId", out correlationValues))
            {
                standardResponse.CorrelationId = correlationValues.FirstOrDefault();
            }

            return standardResponse;
        }

        #endregion
    }
}
