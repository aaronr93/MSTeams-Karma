using System.Configuration;
using System.Web.Http;
using Autofac.Core;

namespace MSTeams.Karma
{
    /// <summary>
    /// Web application lifecycle management.
    /// </summary>
    /// <seealso cref="System.Web.HttpApplication" />
    public class WebApiApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// Executed on IIS app start.
        /// </summary>
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            IocConfig.ConfigureBuilder();
        }
    }
}