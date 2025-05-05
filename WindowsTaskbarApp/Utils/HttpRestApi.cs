using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WindowsTaskbarApp.Utils
{
    public class HttpRestApi
    {
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>
        /// Sends an HTTP request with the specified parameters.
        /// </summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="method">The HTTP method (GET, POST, PUT, DELETE, etc.).</param>
        /// <param name="headers">Optional headers to include in the request.</param>
        /// <param name="body">Optional body content for POST/PUT requests.</param>
        /// <returns>The HTTP response as a string.</returns>
        public static async Task<string> SendRequestAsync(
            string url,
            string method,
            Dictionary<string, string> headers = null,
            string body = null)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("URL cannot be null or empty.", nameof(url));

            if (string.IsNullOrEmpty(method))
                throw new ArgumentException("HTTP method cannot be null or empty.", nameof(method));

            var request = new HttpRequestMessage(new HttpMethod(method), url);

            // Add headers if provided
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }

            // Add body if provided and method supports it
            if (!string.IsNullOrEmpty(body) && (method == "POST" || method == "PUT"))
            {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            try
            {
                var response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}