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

        public static string GetSecret(string key)
        {
            AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();

            var keyVaultClient = new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

            return keyVaultClient.GetSecretAsync($"{KeyVaultBaseUri}/secrets/{key}").Result.Value;
        }

    }
}