using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SportSync.Business.Settings;
using SportSync.Business.Interfaces;
using SportSync.Business.Settings;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text; // **THÊM USING CHO ENCODING**
using System.Text.Json;
using System.Threading.Tasks;

namespace SportSync.Business.Services
{
    public class ESmsSender : ISmsSender
    {
        private readonly ILogger<ESmsSender> _logger;
        private readonly HttpClient _httpClient;
        private readonly ESmsSettings _esmsSettings;

        private class ESmsRequest
        {
            public string ApiKey { get; set; }
            public string Content { get; set; }
            public string Phone { get; set; }
            public string SecretKey { get; set; }
            public string SmsType { get; set; }
            public string? Brandname { get; set; }
        }

        private class ESmsResponse
        {
            public string CodeResult { get; set; }
            public string ErrorMessage { get; set; }
        }

        public ESmsSender(
            ILogger<ESmsSender> logger,
            IHttpClientFactory httpClientFactory,
            IOptions<ESmsSettings> esmsSettings)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("ESmsClient");
            _esmsSettings = esmsSettings.Value;

            if (string.IsNullOrEmpty(_esmsSettings.ApiKey) || string.IsNullOrEmpty(_esmsSettings.ApiSecret))
            {
                _logger.LogError("eSMS ApiKey or ApiSecret is not configured.");
                throw new InvalidOperationException("eSMS settings are missing.");
            }
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            string formattedPhoneNumber = phoneNumber.Trim();
            if (formattedPhoneNumber.StartsWith("+84"))
            {
                formattedPhoneNumber = formattedPhoneNumber.Substring(1);
            }
            else if (formattedPhoneNumber.StartsWith("0"))
            {
                formattedPhoneNumber = "84" + formattedPhoneNumber.Substring(1);
            }

            var requestPayload = new ESmsRequest
            {
                ApiKey = _esmsSettings.ApiKey,
                SecretKey = _esmsSettings.ApiSecret,
                Content = message,
                Phone = formattedPhoneNumber,
                SmsType = _esmsSettings.SmsType,
                Brandname = _esmsSettings.Brandname
            };

            _logger.LogInformation("Sending OTP to {PhoneNumber} via eSMS. Content: {Content}", formattedPhoneNumber, message);

            try
            {
                var jsonPayload = JsonSerializer.Serialize(requestPayload);

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_esmsSettings.ApiEndpoint, content);


                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ESmsResponse>();
                    if (result?.CodeResult == "100")
                    {
                        _logger.LogInformation("eSMS sent successfully to {PhoneNumber}. Response Code: {Code}", formattedPhoneNumber, result.CodeResult);
                    }
                    else
                    {
                        _logger.LogError("eSMS failed to send to {PhoneNumber}. Response Code: {Code}, Message: {ErrorMessage}",
                            formattedPhoneNumber, result?.CodeResult, result?.ErrorMessage);
                        throw new Exception($"eSMS error: {result?.ErrorMessage} (Code: {result?.CodeResult})");
                    }
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("eSMS HTTP request failed for {PhoneNumber}. Status: {StatusCode}, Content: {ErrorContent}",
                        formattedPhoneNumber, response.StatusCode, errorContent);
                    throw new HttpRequestException($"eSMS request failed with status code {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while sending SMS via eSMS to {PhoneNumber}", formattedPhoneNumber);
                throw;
            }
        }
    }
}
