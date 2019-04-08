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

namespace MSTeams_Karma.Tests
{
    public class TeamsLogicTests
    {
        [Fact]
        public async Task TestMultipleKarmaSimultaneouslyAsync()
        {
            var activityStr = "{\"type\":\"message\",\"id\":\"1554657902343\",\"timestamp\":\"2019-04-07T17:25:02.438+00:00\",\"localTimestamp\":\"2019-04-07T13:25:02.438-04:00\",\"localTimezone\":null,\"serviceUrl\":\"https://smba.trafficmanager.net/amer/\",\"channelId\":\"msteams\",\"from\":{\"id\":\"29:1fQqVzt4MuItPSs1HCcB9u-a5o2Eh9rJTL01vXfDhxkhe3LQzopQmAg-cIjWqvkPr-lsxvcitU-Qj2CuGOxMCbQ\",\"name\":\"Aaron Rosenberger\",\"aadObjectId\":\"94e09f70-60e1-4abe-935a-aa4247ad9a6d\",\"role\":null},\"conversation\":{\"isGroup\":true,\"conversationType\":\"channel\",\"id\":\"19:60f98ff1f9524d6d8df6bf80cfe094b3@thread.skype;messageid=1554657902343\",\"name\":null,\"aadObjectId\":null,\"role\":null},\"recipient\":{\"id\":\"28:e58eef39-fbb7-4a72-9e9d-7d5e83ffcab8\",\"name\":\"karma\",\"aadObjectId\":null,\"role\":null},\"textFormat\":\"plain\",\"attachmentLayout\":null,\"membersAdded\":null,\"membersRemoved\":null,\"reactionsAdded\":null,\"reactionsRemoved\":null,\"topicName\":null,\"historyDisclosed\":null,\"locale\":\"en-US\",\"text\":\"\\r\\n<at>karma</at> <at>Chris Pearson</at>++ <at>Jonathan Gomez</at>+ msteams-- \\\"test message\\\"++ <at>Serena</at>++\\n\",\"speak\":null,\"inputHint\":null,\"summary\":null,\"suggestedActions\":null,\"attachments\":[{\"contentType\":\"text/html\",\"contentUrl\":null,\"content\":\"<div>\\r\\n<div itemprop=\\\"copy-paste-block\\\"><span itemscope=\\\"\\\" itemtype=\\\"http://schema.skype.com/Mention\\\" itemid=\\\"0\\\">karma</span> <span itemscope=\\\"\\\" itemtype=\\\"http://schema.skype.com/Mention\\\" itemid=\\\"1\\\">Chris Pearson</span>&#43;&#43; <span itemscope=\\\"\\\" itemtype=\\\"http://schema.skype.com/Mention\\\" itemid=\\\"2\\\">Jonathan Gomez</span>&#43; msteams-- &quot;test message&quot;&#43;&#43; <span itemscope=\\\"\\\" itemtype=\\\"http://schema.skype.com/Mention\\\" itemid=\\\"3\\\">Serena</span>&#43;&#43;</div>\\n</div>\",\"name\":null,\"thumbnailUrl\":null}],\"entities\":[{\"type\":\"mention\",\"mentioned\":{\"id\":\"28:e58eef39-fbb7-4a72-9e9d-7d5e83ffcab8\",\"name\":\"karma\"},\"text\":\"<at>karma</at>\"},{\"type\":\"mention\",\"mentioned\":{\"id\":\"29:1MepEOqCf-l48xb8RQI0jQzqgGmYEWEoi62N3M3zcun8YA7LWMAnTfKPJ1HR2e8ktTaPP-k5IMcgC8Z2ZKbNCgw\",\"name\":\"Chris Pearson\"},\"text\":\"<at>Chris Pearson</at>\"},{\"type\":\"mention\",\"mentioned\":{\"id\":\"29:18yKBlzF9g4wLByTaF7rWMNk8nCgz0YHz70oegQxsVXINf7ciQK4gl62e1-qCLJM-rsX1D5-ghoPV4C-WtrFSWQ\",\"name\":\"Jonathan Gomez\"},\"text\":\"<at>Jonathan Gomez</at>\"},{\"type\":\"mention\",\"mentioned\":{\"id\":\"29:1WPs5iftnzawvfw-1jSAjdwllOQxJPreETRaHMoC1UCmY-nm2Dy-c45pTk4KiYMg4wVVT4lcegJlm0LPyxpqI6Q\",\"name\":\"Serena\"},\"text\":\"<at>Serena</at>\"},{\"type\":\"clientInfo\",\"locale\":\"en-US\",\"country\":\"US\",\"platform\":\"Web\"}],\"channelData\":{\"teamsChannelId\":\"19:60f98ff1f9524d6d8df6bf80cfe094b3@thread.skype\",\"teamsTeamId\":\"19:ec07fb854f154c03878754a8a065bb78@thread.skype\",\"channel\":{\"id\":\"19:60f98ff1f9524d6d8df6bf80cfe094b3@thread.skype\"},\"team\":{\"id\":\"19:ec07fb854f154c03878754a8a065bb78@thread.skype\"},\"tenant\":{\"id\":\"b685d613-ee91-455e-b491-42963f3387a1\"}},\"action\":null,\"replyToId\":null,\"label\":null,\"valueType\":null,\"value\":null,\"name\":null,\"relatesTo\":null,\"code\":null,\"expiration\":null,\"importance\":null,\"deliveryMode\":null,\"listenFor\":null,\"textHighlights\":null,\"semanticAction\":null}";
            var activity = Newtonsoft.Json.JsonConvert.DeserializeObject<Activity>(activityStr);

            var mockDb = Substitute.For<IDocumentDbRepository<KarmaModel>>();
            mockDb.GetItemAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(callInfo => new KarmaModel
            {
                Id = callInfo.ArgAt<string>(0),
                Score = 0
            });

            var testKarmaLogic = new KarmaLogic(mockDb);
            var testTeamsLogic = new TeamsLogic(testKarmaLogic);

            var expected = new List<string>
            {
                "<at>ChrisPearson</at>'s karma has increased to 1",
                "<at>Serena</at>'s karma has increased to 1",
                "msteams's karma has decreased to -1",
                "\"test message\"'s karma has increased to 1"
            };

            var actual = await testTeamsLogic.GetKarmaResponseTextsAsync(activity);

            actual.Should().BeEquivalentTo(expected, options => options.WithoutStrictOrdering());
        }
    }
}
