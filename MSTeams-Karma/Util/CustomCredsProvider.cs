using Microsoft.Bot.Connector;
using MSTeams.Karma.Util;
using System.Configuration;

namespace MSTeams.Karma.Util
{
    public class CustomCredsProvider : SimpleCredentialProvider
    {
        public CustomCredsProvider()
        {
            base.AppId = ConfigurationManager.AppSettings["MicrosoftAppId"];
            base.Password = ConfigurationManager.AppSettings["MicrosoftAppPassword"];
        }
    }
}