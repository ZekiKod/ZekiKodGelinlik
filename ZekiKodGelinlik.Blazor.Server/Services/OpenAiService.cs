using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ZekiKodGelinlik.Blazor.Server.Services
{
    public class OpenAiService
    {
        private readonly HttpClient _httpClient;
        private readonly DatabaseSchemaService _schemaService;
        private const string OpenAiApiUrl = "https://api.openai.com/v1/chat/completions";
        private readonly string _apiKey;

        public OpenAiService(HttpClient httpClient, DatabaseSchemaService schemaService, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _schemaService = schemaService;
            _apiKey = "sk-i_xeISjve8wP0IvRWnPfzVUBIM3FwLxfq42kuThiWcT3BlbkFJTXt1pq1qk5cwsA603PQTyniw2CKDRlhuKvVJc5EW4A";
        }

        // Soru sorma işlemi
        public async Task<string> AskQuestionAsync(string question)
        {
            string schemaInfo = _schemaService.GetCachedSchema();
            if (string.IsNullOrEmpty(schemaInfo))
            {
                throw new InvalidOperationException("Şema bilgisi cache'te bulunamadı.");
            }

            var messageContent = $"{schemaInfo}\n\nSoru: {question}";

            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "system", content = "You are a database assistant." },
                    new { role = "user", content = messageContent }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, OpenAiApiUrl)
            {
                Content = JsonContent.Create(requestBody)
            };
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }

        // Şemayı OpenAI'ye göndermek
        public async Task CacheSchemaAsync(string schemaInfo)
        {
            var systemMessage = "Aşağıdaki veritabanı şemasını kalıcı olarak saklayın ve bu şema üzerinde gelecek soruları yanıtlayın.";
            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "system", content = systemMessage },
                    new { role = "user", content = schemaInfo }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, OpenAiApiUrl)
            {
                Content = JsonContent.Create(requestBody)
            };
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            // Şemanın başarılı bir şekilde gönderildiğini doğrulamak için yanıtı kontrol edebilirsiniz
            if (responseContent.Contains("successful"))
            {
                _schemaService.CacheSchema(schemaInfo);
            }
            else
            {
                throw new InvalidOperationException("Şema gönderimi başarısız oldu.");
            }
        }
    }
}
