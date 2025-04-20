using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsTaskbarApp.Utils
{
    public static class UrlTester
    {
        public static async Task TestUrlAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("URL is empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetStringAsync(url);
                MessageBox.Show($"Response from {url}:\n{response}", "HTTP Response", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error testing URL: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}