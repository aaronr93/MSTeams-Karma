using Microsoft.Bot.Connector;
using MSTeams.Karma.Util;

namespace MSTeams.Karma.Util
{
    public class CustomCredsProvider : SimpleCredentialProvider
    {
        // I'm sure there's a better way to do this...
        public CustomCredsProvider()
        {
            var appIdTask = KeyVaultHelper.GetSecret("MicrosoftAppId");
            appIdTask.Wait();
            base.AppId = appIdTask.Result;

            var passwordTask = KeyVaultHelper.GetSecret("MicrosoftAppPassword");
            passwordTask.Wait();
            base.Password = passwordTask.Result;
        }
    }
}