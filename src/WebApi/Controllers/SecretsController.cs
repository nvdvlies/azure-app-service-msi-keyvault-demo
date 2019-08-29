using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecretsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly TelemetryClient _telemetry;

        public SecretsController(
            IConfiguration configuration,
            TelemetryClient telemetry
        )
        {
            _configuration = configuration;
            _telemetry = telemetry;
        }

        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            try
            {
                _telemetry.TrackEvent("SecretsRequested");

                var keyvaultName = _configuration["KeyvaultName"];
                var secretName = "AppSecret";

                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                var secretBundle = await keyVaultClient.GetSecretAsync($"https://{keyvaultName}.vault.azure.net/secrets/{secretName}")
                        .ConfigureAwait(false);

                return secretBundle.Value;
            }
            catch (KeyVaultErrorException keyVaultException)
            {
                _telemetry.TrackException(keyVaultException);
                return keyVaultException.Message;
            }
        }
    }
}
