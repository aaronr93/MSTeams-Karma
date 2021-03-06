﻿using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using MSTeams.Karma.Models;
using MSTeams.Karma.BusinessLogic;
using MSTeams.Karma.Controllers;
using System.Collections.Generic;

namespace MSTeams.Karma
{
    public static class IocConfig
    {
        private static IContainer _container;

        public static void ConfigureBuilder()
        {
            var builder = new ContainerBuilder();
            
            builder.Register(a => new DocumentDbRepository<KarmaModel>("karma-collection")).As<IDocumentDbRepository<KarmaModel>>();
            builder.Register(a => new DocumentDbRepository<ScoreboardModel>("karma-collection")).As<IDocumentDbRepository<ScoreboardModel>>();
            builder.Register(a => new DocumentDbRepository<TeamsChannelMetadataModel>("teamsChannelMetadata")).As<IDocumentDbRepository<TeamsChannelMetadataModel>>();
            builder.RegisterType<KarmaLogic>();
            builder.RegisterType<MessageLogic>().SingleInstance();
            builder.RegisterType<TeamsKarmaLogic>();
            builder.RegisterType<TeamsToggleLogic>();
            builder.RegisterType<TeamsScoreboardLogic>();
            builder.RegisterType<TeamsScoreLogic>();
            builder.RegisterType<MessagesController>();

            _container = builder.Build();

            var webApiResolver = new AutofacWebApiDependencyResolver(_container);
            GlobalConfiguration.Configuration.DependencyResolver = webApiResolver;
        }

        public static T Resolve<T>()
        {
            if (_container == null)
            {
                ConfigureBuilder();
            }

            return _container.Resolve<T>();
        }

    }
}