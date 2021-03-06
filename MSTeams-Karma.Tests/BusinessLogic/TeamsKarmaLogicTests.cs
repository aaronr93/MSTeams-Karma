﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using MSTeams.Karma.BusinessLogic;
using Microsoft.Bot.Connector;
using NSubstitute;
using FluentAssertions;
using MSTeams.Karma.Models;
using MSTeams.Karma;
using Newtonsoft.Json;
using System.IO;
using System.Threading;

namespace MSTeams.Karma.Tests.BusinessLogic
{
    public class TeamsKarmaLogicTests
    {
        private TeamsKarmaLogic GetTestTeamsKarmaLogic()
        {
            var mockDb = Substitute.For<IDocumentDbRepository<KarmaModel>>();
            mockDb.GetItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(callInfo => new KarmaModel
            {
                Id = callInfo.ArgAt<string>(0),
                Score = 0
            });

            var testKarmaLogic = new KarmaLogic(mockDb);
            return new TeamsKarmaLogic(testKarmaLogic);
        }

        private TeamsScoreboardLogic GetTestTeamsScoreboardLogic()
        {
            var mockDb = Substitute.For<IDocumentDbRepository<ScoreboardModel>>();
            return new TeamsScoreboardLogic(mockDb);
        }

        private TeamsScoreLogic GetTestTeamsScoreLogic()
        {
            var mockDb = Substitute.For<IDocumentDbRepository<KarmaModel>>();
            return new TeamsScoreLogic(mockDb);
        }

        private Activity GetTestActivityFromFile(string filename)
        {
            Activity activity = null;

            using (StreamReader r = new StreamReader(filename))
            {
                string json = r.ReadToEnd();
                activity = JsonConvert.DeserializeObject<Activity>(json);
            }

            activity.Text = Utilities.TrimWhitespace(activity.Text);

            return activity;
        }

        [Theory]
        [InlineData("TestMultiWordKarmaMessage01")]
        [InlineData("TestMultiWordKarmaMessage02")]
        public async Task TestMultiWordKarmaMessage(string filename)
        {
            Activity testActivity = GetTestActivityFromFile($@"TestActivities\{filename}.json");
            TeamsKarmaLogic testTeamsLogic = GetTestTeamsKarmaLogic();
            
            var actual = await testTeamsLogic.GetKarmaResponseTextsAsync(testActivity, default(CancellationToken));

            actual.Should().BeEquivalentTo(new List<string>
            {
                "\"giving karma to phrases\"'s karma has increased to 1"
            }, options => options.WithoutStrictOrdering());
        }

        [Theory]
        [InlineData("TestSimpleKarmaMessage01")]
        [InlineData("TestSimpleKarmaMessage02")]
        [InlineData("TestSimpleKarmaMessage03")]
        [InlineData("TestSimpleKarmaMessage04")]
        public async Task TestSimpleKarmaMessage(string filename)
        {
            Activity testActivity = GetTestActivityFromFile($@"TestActivities\{filename}.json");
            TeamsKarmaLogic testTeamsLogic = GetTestTeamsKarmaLogic();
            
            var actual = await testTeamsLogic.GetKarmaResponseTextsAsync(testActivity, default(CancellationToken));

            actual.Should().BeEquivalentTo(new List<string>
            {
                "msteams's karma has decreased to -1"
            }, options => options.WithoutStrictOrdering());
        }

        [Fact]
        public async Task TestMultipleKarmaSimultaneouslyAsync01()
        {
            Activity testActivity = GetTestActivityFromFile(@"TestActivities\TestMultipleKarmaSimultaneouslyAsync01.json");
            TeamsKarmaLogic testTeamsLogic = GetTestTeamsKarmaLogic();
            
            var actual = await testTeamsLogic.GetKarmaResponseTextsAsync(testActivity, default(CancellationToken));

            actual.Should().BeEquivalentTo(new List<string>
            {
                "<at>Chris Pearson</at>'s karma has increased to 1",
                "<at>Serena</at>'s karma has increased to 1",
                "msteams's karma has decreased to -1",
                "\"test message\"'s karma has increased to 1"
            }, options => options.WithoutStrictOrdering());
        }

        [Fact]
        public async Task TestMultipleKarmaSimultaneouslyAsync02()
        {
            Activity testActivity = GetTestActivityFromFile(@"TestActivities\TestMultipleKarmaSimultaneouslyAsync02.json");
            TeamsKarmaLogic testTeamsLogic = GetTestTeamsKarmaLogic();
            
            var actual = await testTeamsLogic.GetKarmaResponseTextsAsync(testActivity, default(CancellationToken));

            actual.Should().BeEquivalentTo(new List<string>
            {
                "<at>Ashley Raba</at>'s karma has increased to 5",
                "\"giving karma to phrases\"'s karma has decreased to -1"
            }, options => options.WithoutStrictOrdering());
        }
    }
}
