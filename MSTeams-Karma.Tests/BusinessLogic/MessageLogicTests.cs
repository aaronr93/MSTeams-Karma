using FluentAssertions;
using MSTeams.Karma.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace MSTeams.Karma.Tests.BusinessLogic
{
    public class MessageLogicTests
    {
        [Theory]
        [InlineData("get top things", true)]
        [InlineData("<at>karma</at> get top things", true)]
        [InlineData("<at>karma</at> get top thing", true)]
        [InlineData("<at>karma</at> get top thinsg", true)]
        [InlineData("<at>karma</at> get top thnigs", true)]
        [InlineData("<at>karma</at> get top thigns", true)]
        [InlineData("<at>karma</at> get top htings", true)]
        [InlineData("<at>karma</at> get top thisng", true)]
        [InlineData("<at>karma</at>  get top things", true)]
        [InlineData("<at>karma</at>  get top things\n", true)]
        [InlineData("<at>karma</at>  get top thigns\n", true)]
        [InlineData("<at>karma</at> get top things.", true)]
        [InlineData("<at>karma</at> get top thing.", true)]
        [InlineData("<at>karma</at> get top things!", true)]
        [InlineData("<at>karma</at> get top things please.", false)]
        [InlineData("<at>karma</at> <at>Aaron Rosenberger</at> get top things", false)]
        [InlineData("<at>karma</at> \"some entity\" get top things", false)]
        [InlineData("<at>karma</at> msteams-- get top things", false)]
        public void TestGetTopThings(string message, bool expectedValue)
        {
            new MessageLogic().IsGettingScoreboard(message).Success.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("get bottom things", true)]
        [InlineData("<at>karma</at> get bottom things", true)]
        [InlineData("<at>karma</at> get bottom thing", true)]
        [InlineData("<at>karma</at> get bottom thinsg", true)]
        [InlineData("<at>karma</at> get bottom thnigs", true)]
        [InlineData("<at>karma</at> get bottom thigns", true)]
        [InlineData("<at>karma</at> get bottom htings", true)]
        [InlineData("<at>karma</at> get bottom thisng", true)]
        [InlineData("<at>karma</at>  get bottom things", true)]
        [InlineData("<at>karma</at>  get bottom things\n", true)]
        [InlineData("<at>karma</at>  get bottom thigns\n", true)]
        [InlineData("<at>karma</at> get bottom things.", true)]
        [InlineData("<at>karma</at> get bottom thing.", true)]
        [InlineData("<at>karma</at> get bottom things!", true)]
        [InlineData("<at>karma</at> get bottom things please.", false)]
        [InlineData("<at>karma</at> <at>Aaron Rosenberger</at> get bottom things", false)]
        [InlineData("<at>karma</at> \"some entity\" get bottom things", false)]
        [InlineData("<at>karma</at> msteams-- get bottom things", false)]
        public void TestGetBottomThings(string message, bool expectedValue)
        {
            new MessageLogic().IsGettingScoreboard(message).Success.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("get top users", true)]
        [InlineData("<at>karma</at> get top users", true)]
        [InlineData("<at>karma</at> get top user", true)]
        [InlineData("<at>karma</at> get top usrs", true)]
        [InlineData("<at>karma</at> get top usres", true)]
        [InlineData("<at>karma</at> get top suers", true)]
        [InlineData("<at>karma</at>  get top users", true)]
        [InlineData("<at>karma</at>  get top users\n", true)]
        [InlineData("<at>karma</at>  get top usrs\n", true)]
        [InlineData("<at>karma</at> get top users.", true)]
        [InlineData("<at>karma</at> get top user.", true)]
        [InlineData("<at>karma</at> get top users!", true)]
        [InlineData("<at>karma</at> get top users please.", false)]
        [InlineData("<at>karma</at> <at>Aaron Rosenberger</at> get top users", false)]
        [InlineData("<at>karma</at> \"some entity\" get top users", false)]
        [InlineData("<at>karma</at> msteams-- get top users", false)]
        public void TestGetTopUsers(string message, bool expectedValue)
        {
            new MessageLogic().IsGettingScoreboard(message).Success.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("get bottom users", true)]
        [InlineData("<at>karma</at> get bottom users", true)]
        [InlineData("<at>karma</at> get bottom user", true)]
        [InlineData("<at>karma</at> get bottom usrs", true)]
        [InlineData("<at>karma</at> get bottom usres", true)]
        [InlineData("<at>karma</at> get bottom suers", true)]
        [InlineData("<at>karma</at>  get bottom users", true)]
        [InlineData("<at>karma</at>  get bottom users\n", true)]
        [InlineData("<at>karma</at>  get bottom usrs\n", true)]
        [InlineData("<at>karma</at> get bottom users.", true)]
        [InlineData("<at>karma</at> get bottom user.", true)]
        [InlineData("<at>karma</at> get bottom users!", true)]
        [InlineData("<at>karma</at> get bottom users please.", false)]
        [InlineData("<at>karma</at> <at>Aaron Rosenberger</at> get bottom users", false)]
        [InlineData("<at>karma</at> \"some entity\" get bottom users", false)]
        [InlineData("<at>karma</at> msteams-- get bottom users", false)]
        public void TestGetBottomUsers(string message, bool expectedValue)
        {
            new MessageLogic().IsGettingScoreboard(message).Success.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("get  <at>Aaron Rosenberger</at>", true)]
        [InlineData("<at>karma</at> get  <at>Aaron Rosenberger</at>", true)]
        [InlineData("<at>karma</at> get <at>Aaron Rosenberger</at>", true)]
        [InlineData("<at>karma</at>  get  <at>Aaron Rosenberger</at>\n", true)]
        [InlineData("<at>karma</at> get <at>Aaron Rosenberger</at>\n", true)]
        [InlineData("<at>karma</at> get <at>Aaron Rosenberger</at>.", true)]
        [InlineData("<at>karma</at> get  <at>Aaron Rosenberger</at>.", true)]
        [InlineData("<at>karma</at> get <at>Aaron Rosenberger</at>!", true)]
        [InlineData("<at>karma</at> get  <at>Aaron Rosenberger</at> please.", false)]
        [InlineData("<at>karma</at> <at>Aaron Rosenberger</at> get", false)]
        [InlineData("<at>karma</at> \"some entity\" get", false)]
        [InlineData("<at>karma</at> msteams-- get <at>Aaron Rosenberger</at>", false)]
        [InlineData("<at>karma</at> get msteams--", false)]
        public void TestGetUser(string message, bool expectedValue)
        {
            new MessageLogic().IsGettingScore(message).Success.Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("get \"some entity\"", true)]
        [InlineData("<at>karma</at> get \"some entity\"", true)]
        [InlineData("<at>karma</at> get  \"some entity\"", true)]
        [InlineData("<at>karma</at> get msteams", true)]
        [InlineData("<at>karma</at> get  msteams", true)]
        [InlineData("<at>karma</at> msteams-- get <at>Aaron Rosenberger</at>", false)]
        [InlineData("<at>karma</at> get \"some entity\"--", false)]
        [InlineData("<at>karma</at> get thg msteams--", false)]
        [InlineData("<at>karma</at> \"some entity\" get user", false)]
        [InlineData("<at>karma</at> msteams-- get users", false)]
        public void TestGetThing(string message, bool expectedValue)
        {
            new MessageLogic().IsGettingScore(message).Success.Should().Be(expectedValue);
        }
    }
}
