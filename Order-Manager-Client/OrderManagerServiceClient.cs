﻿
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.IO;
using Microsoft.Extensions.Logging;
using System;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

using Steeltoe.Common.Discovery;

namespace OrderManagerService.Client
{
    public class OrderManagerServiceClient : IOrderManagerService
    {

        ILogger<OrderManagerServiceClient> _logger;
        IOptionsSnapshot<OrderManagerServiceOptions> _config;

        private OrderManagerServiceOptions Config
        {
            get
            {
                return _config.Value;
            }
        }

        // Lab07 Start
        DiscoveryHttpClientHandler _handler;
        // Lab07End

        // Lab10 Start
        IHttpContextAccessor _reqContext;
        // Lab10 End

        public OrderManagerServiceClient(
            IOptionsSnapshot<OrderManagerServiceOptions> config, 
            ILogger<OrderManagerServiceClient> logger,
            // Lab07 Start
            IDiscoveryClient client,
            // Lab07 End

            // Lab10 Start
            IHttpContextAccessor context = null)
            // Lab10 End
        {
            // Lab07 Start
            _handler = new DiscoveryHttpClientHandler(client);
            // Lab07 End

            // Lab10 Start
            _reqContext = context;
            // Lab10 End

            _logger = logger;
            _config = config;
  
        }



        public async Task<List<Object>> AllOrdersAsync()
        {
            // Lab05 Start
            return await HandleRequest<List<Object>>(Config.AllFortunesURL);
            // Lab05 End
        }

        public async Task<Object> GetOrderAsync()
        {
            // Lab05 Start
            return await HandleRequest<Object>(Config.RandomFortuneURL);
            // Lab05 End
        }

        private async Task<T> HandleRequest<T>(string url) where T : class
        {
            _logger?.LogDebug("FortuneService call: {url}", url);
            try
            {
                using (var client = await GetClientAsync())
                {
                    var stream = await client.GetStreamAsync(url);
                    var result = Deserialize<T>(stream);
                    _logger?.LogDebug("FortuneService returned: {result}", result);
                    return result;
                }
            }
            catch (Exception e)
            {
                _logger?.LogError("FortuneService exception: {0}", e);
                throw;
            }
        }


        private T Deserialize<T>(Stream stream) where T : class
        {
            try
            {
                using (JsonReader reader = new JsonTextReader(new StreamReader(stream)))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return (T)serializer.Deserialize(reader, typeof(T));
                }
            }
            catch (Exception e)
            {
                _logger?.LogError("FortuneService serialization exception: {0}", e);
            }
            return (T)null;
        }

        private async Task<HttpClient> GetClientAsync()
        {
            // Lab07 Start
            var client = new HttpClient(_handler, false);
            // Lab07 End

            // Lab10 Start
            if (_reqContext != null)
            {
                var token = await _reqContext.HttpContext.GetTokenAsync("access_token");

                _logger?.LogDebug("GetClientAsync access token: {token}", token);

                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }
            // Lab10 End

            return client;
        }
    }
}