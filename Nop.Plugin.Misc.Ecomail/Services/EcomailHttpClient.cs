using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Net.Http.Headers;
using Nop.Core;

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

        #endregion

        #region Ctor

        public EcomailHttpClient(EcomailSettings ecomailSettings,
            HttpClient httpClient)
        {
            //configure client
            httpClient.BaseAddress = new Uri(EcomailDefaults.EcomailApiBaseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(ecomailSettings.RequestTimeout ?? EcomailDefaults.RequestTimeout);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.UserAgent, EcomailDefaults.UserAgent);
            httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, MimeTypes.ApplicationJson);

            _ecomailSettings = ecomailSettings;
            _httpClient = httpClient;
        }

        #endregion

        #region Methods

        /// <summary>
        /// HTTP request services
        /// </summary>
        /// <param name="apiUri">Request URL</param>
        /// <param name="data">Data to send</param>
        /// <param name="httpMethod">Request type</param>
        /// <param name="apiKey">API key</param>
        /// <returns>The asynchronous task whose result contains response details</returns>
        public async Task<string> RequestAsync(string apiUri, string data, HttpMethod httpMethod, string apiKey = "")
        {
            var request = new HttpRequestMessage
            {
                Method = httpMethod,
                RequestUri = new Uri(apiUri)
            };

            request.Headers.TryAddWithoutValidation(EcomailDefaults.ApiKeyHeader,
                !string.IsNullOrEmpty(apiKey) ? apiKey : _ecomailSettings.ApiKey);

            if (httpMethod != HttpMethod.Get && !string.IsNullOrEmpty(data))
                request.Content = new StringContent(data, Encoding.UTF8, MimeTypes.ApplicationJson);

            var httpResponse = await _httpClient.SendAsync(request);
            if (httpResponse is null)
                throw new NopException("No service response");

            var response = await httpResponse.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(response))
                throw new NopException("Response is empty");

            return response;
        }

        #endregion
    }
}