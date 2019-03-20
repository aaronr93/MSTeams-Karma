using Microsoft.Bot.Connector;
using MSTeams.Karma.Util;

namespace MSTeams.Karma.Util
{
    public class CustomCredsProvider : SimpleCredentialProvider
    {
        public CustomCredsProvider()
        {
            base.AppId = KeyVaultHelper.GetSecret("MicrosoftAppId");
            base.Password = KeyVaultHelper.GetSecret("MicrosoftAppPassword");
        }
    }
}