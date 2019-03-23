using MSTeams.Karma;
using System;
using Xunit;

namespace MSTeams_Karma.Tests
{
    public class KarmaLogicTests
    {
        [Theory]
        [InlineData("<at>Aaron Rosenberger</at>++", "<at>Aaron Rosenberger</at>", true)]
        [InlineData("<at>Aaron Rosenberger</at> ++", "<at>Aaron Rosenberger</at>", true)]
        [InlineData("<at>Aaron Rosenberger</at>++++", "<at>Aaron Rosenberger</at>", true)]
        [InlineData("<at>Aaron Rosenberger</at> ++++", "<at>Aaron Rosenberger</at>", true)]
        [InlineData("<at>Aaron Rosenberger</at>+++++++++++++++++", "<at>Aaron Rosenberger</at>", true)]
        [InlineData("<at>Aaron Rosenberger</at> +++++++++++++++++", "<at>Aaron Rosenberger</at>", true)]
        [InlineData("<at>Aaron Rosenberger</at>--", "<at>Aaron Rosenberger</at>", true)]
        [InlineData("<at>Aaron Rosenberger</at> --", "<at>Aaron Rosenberger</at>", true)]
        [InlineData("<at>Aaron Rosenberger</at>----", "<at>Aaron Rosenberger</at>", true)]
        [InlineData("<at>Aaron Rosenberger</at> ----", "<at>Aaron Rosenberger</at>", true)]
        [InlineData("<at>Aaron Rosenberger</at>-----------------", "<at>Aaron Rosenberger</at>", true)]
        [InlineData("<at>Aaron Rosenberger</at> -----------------", "<at>Aaron Rosenberger</at>", true)]
        [InlineData("<at>Aaron Rosenberger</at>+-", "<at>Aaron Rosenberger</at>", false)]
        [InlineData("<at>Aaron Rosenberger</at> +-", "<at>Aaron Rosenberger</at>", false)]
        [InlineData("<at>Aaron Rosenberger</at>+++-", "<at>Aaron Rosenberger</at>", false)]
        [InlineData("<at>Aaron Rosenberger</at> +++-", "<at>Aaron Rosenberger</at>", false)]
        [InlineData("<at>Aaron Rosenberger</at>++++++++++++++++-", "<at>Aaron Rosenberger</at>", false)]
        [InlineData("<at>Aaron Rosenberger</at> ++++++++++++++++-", "<at>Aaron Rosenberger</at>", false)]
        [InlineData("<at>Aaron Rosenberger</at>-+", "<at>Aaron Rosenberger</at>", false)]
        [InlineData("<at>Aaron Rosenberger</at> -+", "<at>Aaron Rosenberger</at>", false)]
        [InlineData("<at>Aaron Rosenberger</at>---+", "<at>Aaron Rosenberger</at>", false)]
        [InlineData("<at>Aaron Rosenberger</at> ---+", "<at>Aaron Rosenberger</at>", false)]
        [InlineData("<at>Aaron Rosenberger</at>----------------+", "<at>Aaron Rosenberger</at>", false)]
        [InlineData("<at>Aaron Rosenberger</at> ----------------+", "<at>Aaron Rosenberger</at>", false)]
        public void Test_MentionedUserWasGivenKarma(string message, string mentionedUserText, bool expected)
        {
            var result = KarmaLogic.MentionedUserWasGivenKarma(message, mentionedUserText);

            Assert.Equal(result, expected);
        }
    }
}
