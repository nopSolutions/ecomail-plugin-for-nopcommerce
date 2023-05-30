using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.Ecomail.Services
{
    /// <summary>
    /// Represents HTTP client to request third-party services
    /// </summary>
    public class EcomailHttpClient
    {
        #region Fields

        private readonly EcomailSettings _ecomailSettings;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public EcomailHttpClient(EcomailSettings ecomailSettings,
            HttpClient httpClient,
            ILogger logger)
        {
            //configure client
            httpClient.BaseAddress = new Uri(EcomailDefaults.BaseApiUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(ecomailSettings.RequestTimeout ?? EcomailDefaults.RequestTimeout);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, EcomailDefaults.UserAgent);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, MimeTypes.ApplicationJson);

            _ecomailSettings = ecomailSettings;
            _httpClient = httpClient;
            _logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// HTTP request services
        /// </summary>
        /// <param name="apiUri">Request URL</param>
        /// <param name="data">Data to send</param>
        /// <param name="httpMethod">Request type</param>
        /// <returns>The asynchronous task whose result contains response details</returns>
        public async Task<string> RequestAsync(string apiUri, string data, HttpMethod httpMethod)
        {
            var requestUri = new Uri(apiUri);

            var logMessage = string.Empty;
            if (_ecomailSettings.LogRequests)
            {
                logMessage = $"{httpMethod.Method.ToUpper()} {requestUri.PathAndQuery}{Environment.NewLine}";
                logMessage += $"Host: {requestUri.Host}{Environment.NewLine}";
                if (httpMethod != HttpMethod.Get)
                {
                    logMessage += $"Content-Type: {MimeTypes.ApplicationJson}{Environment.NewLine}";
                    logMessage += $"{Environment.NewLine}{data}{Environment.NewLine}";
                }
            }

            var request = new HttpRequestMessage
            {
                Method = httpMethod,
                RequestUri = requestUri
            };

            request.Headers.TryAddWithoutValidation(EcomailDefaults.ApiKeyHeader, _ecomailSettings.ApiKey);

            if (httpMethod != HttpMethod.Get && !string.IsNullOrEmpty(data))
                request.Content = new StringContent(data, Encoding.UTF8, MimeTypes.ApplicationJson);

            var httpResponse = await _httpClient.SendAsync(request)
                ?? throw new NopException("No service response");

            var response = await httpResponse.Content.ReadAsStringAsync();

            if (_ecomailSettings.LogRequests)
            {
                logMessage += $"{Environment.NewLine}Response:{Environment.NewLine}";
                logMessage += !httpResponse.IsSuccessStatusCode
                    ? $"{httpResponse.StatusCode}: {httpResponse.RequestMessage?.ToString()}"
                    : response;
                await _logger.InsertLogAsync(LogLevel.Debug, $"{EcomailDefaults.SystemName} request details", logMessage);
            }

            if (string.IsNullOrEmpty(response))
                throw new NopException("Response is empty");

            return response;
        }

        #endregion
    }
}