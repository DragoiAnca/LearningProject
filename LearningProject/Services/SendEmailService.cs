using LearningProject.Models;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace LearningProject.Services
{
    public class SendEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiUrlsOptions _apiUrls;

        public SendEmailService(HttpClient httpClient, IOptions<ApiUrlsOptions> apiUrls)
        {
            _httpClient = httpClient;
            _apiUrls = apiUrls.Value;
        }

        public async Task<string> SendEmailAsync(Email email)
        {
            var response = await _httpClient.PostAsJsonAsync(_apiUrls.SendEmail, email);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
