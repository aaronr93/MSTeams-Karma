using System;
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

namespace MSTeams_Karma.Tests
{
    public class TeamsLogicTests
    {
        private TeamsKarmaLogic GetTestTeamsLogic()
        {
            var mockDb = Substitute.For<IDocumentDbRepository<KarmaModel>>();
            mockDb.GetItemAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(callInfo => new KarmaModel
            {
                Id = callInfo.ArgAt<string>(0),
                Score = 0
            });

            var testKarmaLogic = new KarmaLogic(mockDb);
            return new TeamsKarmaLogic(testKarmaLogic);
        }

        private Activity GetTestActivityFromFile(string filename)
        {
            Activity activity = null;

            using (StreamReader r = new StreamReader(filename))
            {
                string json = r.ReadToEnd();
                activity = JsonConvert.DeserializeObject<Activity>(json);
            }

            return activity;
        }

        [Theory]
        [InlineData("TestMultiWordKarmaMessage01")]
        [InlineData("TestMultiWordKarmaMessage02")]
        public async Task TestMultiWordKarmaMessage(string filename)
        {
            Activity testActivity = GetTestActivityFromFile($"TestActivities\\{filename}.json");
            TeamsKarmaLogic testTeamsLogic = GetTestTeamsLogic();
            
            var actual = await testTeamsLogic.GetKarmaResponseTextsAsync(testActivity);

            actual.Should().BeEquivalentTo(new List<string>
            {
                "\"giving karma to phrases\"'s karma has increased to 1"
            }, options => options.WithoutStrictOrdering());
        }

        [Theory]
        [InlineData("TestSimpleKarmaMessage01")]
        [InlineData("TestSimpleKarmaMessage02")]
        public async Task TestSimpleKarmaMessage(string filename)
        {
            Activity testActivity = GetTestActivityFromFile($"TestActivities\\{filename}.json");
            TeamsKarmaLogic testTeamsLogic = GetTestTeamsLogic();
            
            var actual = await testTeamsLogic.GetKarmaResponseTextsAsync(testActivity);

            actual.Should().BeEquivalentTo(new List<string>
            {
                "msteams's karma has decreased to -1"
            }, options => options.WithoutStrictOrdering());
        }

        [Fact]
        public async Task TestMultipleKarmaSimultaneouslyAsync01()
        {
            Activity testActivity = GetTestActivityFromFile(@"TestActivities\TestMultipleKarmaSimultaneouslyAsync01.json");
            TeamsKarmaLogic testTeamsLogic = GetTestTeamsLogic();
            
            var actual = await testTeamsLogic.GetKarmaResponseTextsAsync(testActivity);

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
            TeamsKarmaLogic testTeamsLogic = GetTestTeamsLogic();
            
            var actual = await testTeamsLogic.GetKarmaResponseTextsAsync(testActivity);

            actual.Should().BeEquivalentTo(new List<string>
            {
                "<at>Ashley Raba</at>'s karma has increased to 5",
                "\"giving karma to phrases\"'s karma has increased to 1"
            }, options => options.WithoutStrictOrdering());
        }
    }
}
