using System;
using System.Threading.Tasks;
using System.Web.Configuration;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;

namespace MSTeams.Karma.Util
{
    public class KeyVaultHelper
    {
        private static string KeyVaultBaseUri => WebConfigurationManager.AppSettings["AzureKeyVaultBaseUri"];

        public static async Task<string> GetSecret(string key)
        {
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();

            var keyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            var secret = await keyVaultClient.GetSecretAsync($"{KeyVaultBaseUri}/secrets/{key}")
                .ConfigureAwait(false);

            return secret.Value;
        }

    }
}