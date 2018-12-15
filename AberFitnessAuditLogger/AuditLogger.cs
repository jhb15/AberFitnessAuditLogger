using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace AberFitnessAuditLogger
{
    public class AuditLogger : IAuditLogger
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<AuditLogger> logger;
        private readonly Uri loggingEndpoint;
        private readonly string serviceName;
        private readonly string gatekeeperUrl;
        private readonly string clientId;
        private readonly string clientSecret;
        private DiscoveryCache discoveryCache;

        public AuditLogger(IHttpClientFactory httpClientFactory, ILogger<AuditLogger> logger)
        {
            var gladosUrl = Environment.GetEnvironmentVariable("Audit__GladosUrl");
            this.logger = logger;
            this.httpClient = httpClientFactory.CreateClient();
            this.loggingEndpoint = new Uri($"{gladosUrl}/api/audit/new");
            this.serviceName = Assembly.GetEntryAssembly().GetName().Name;
            this.gatekeeperUrl = Environment.GetEnvironmentVariable($"{serviceName}__GatekeeperUrl");
            this.clientId = Environment.GetEnvironmentVariable($"{serviceName}__ClientId");
            this.clientSecret = Environment.GetEnvironmentVariable($"{serviceName}__ClientSecret");
            this.discoveryCache = new DiscoveryCache(gatekeeperUrl);
        }

        private async Task<string> GetTokenAsync()
        {
            var discovery = await discoveryCache.GetAsync();
            if (discovery.IsError)
            {
                logger.LogError(discovery.Error);
                throw new Exception("Couldn't read discovery document.");
            }

            var tokenRequest = new ClientCredentialsTokenRequest
            {
                Address = discovery.TokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = "glados"
            };
            var response = await httpClient.RequestClientCredentialsTokenAsync(tokenRequest);
            if (response.IsError)
            {
                logger.LogError(response.Error);
                throw new Exception("Couldn't retrieve access token.");
            }
            return response.AccessToken;
        }

        public async Task log(string userId, string content)
        {
            var logContent = new LogEntry
            {
                Content = content,
                ServiceName = serviceName,
                UserId = userId,
                Timestamp = GetMillisSinceEpoch().ToString()
            };

            logger.LogDebug($"Sending audit data: {JsonConvert.SerializeObject(logContent)}");
            httpClient.SetBearerToken(await GetTokenAsync());
            var response = await httpClient.PostAsJsonAsync(loggingEndpoint, logContent);
            var responseContent = await response.Content.ReadAsStringAsync();
            logger.LogDebug("Received response:", responseContent);
        }

        private long GetMillisSinceEpoch()
        {
            return (long)DateTime.Now.ToUniversalTime().Subtract(
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            ).TotalMilliseconds;
        }
    }
}
