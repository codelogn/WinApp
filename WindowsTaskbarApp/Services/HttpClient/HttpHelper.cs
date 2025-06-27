using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace WindowsTaskbarApp.Services.HttpClient
{
    public static class HttpHelper
    {
        public static async Task<string> FetchContentAsync(string url, string httpMethod = null, string httpHeader = null, string httpBody = null, string contentType = null, string accept = null, string userAgent = null)
        {
            try
            {
                using var httpClient = new System.Net.Http.HttpClient();
                var request = new HttpRequestMessage();
                request.RequestUri = new Uri(url);
                request.Method = !string.IsNullOrWhiteSpace(httpMethod) ? new HttpMethod(httpMethod) : HttpMethod.Get;

                if (!string.IsNullOrWhiteSpace(httpBody) && (request.Method == HttpMethod.Post || request.Method == HttpMethod.Put || request.Method.Method == "PATCH"))
                {
                    request.Content = new StringContent(httpBody);
                    if (!string.IsNullOrWhiteSpace(contentType))
                        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                }

                if (!string.IsNullOrWhiteSpace(accept))
                    request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(accept));

                // Set default user-agent if not provided
                if (string.IsNullOrWhiteSpace(userAgent))
                    userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.6422.112 Safari/537.36";
                request.Headers.UserAgent.ParseAdd(userAgent);

                // Parse and add custom headers (semicolon or newline separated)
                if (!string.IsNullOrWhiteSpace(httpHeader))
                {
                    var headerLines = httpHeader.Split(new[] { '\n', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var headerLine in headerLines)
                    {
                        var parts = headerLine.Split(new[] { ':' }, 2);
                        if (parts.Length == 2)
                        {
                            var headerKey = parts[0].Trim();
                            var headerValue = parts[1].Trim();
                            if (!string.IsNullOrEmpty(headerKey) && !string.IsNullOrEmpty(headerValue))
                            {
                                // Avoid adding restricted headers
                                if (!request.Headers.TryAddWithoutValidation(headerKey, headerValue))
                                {
                                    if (request.Content != null)
                                        request.Content.Headers.TryAddWithoutValidation(headerKey, headerValue);
                                }
                            }
                        }
                    }
                }

                var response = await httpClient.SendAsync(request);
                if ((int)response.StatusCode >= 400)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}: {errorContent}");
                }
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                // Optionally log or rethrow
                return string.Empty;
            }
        }
    }
}
