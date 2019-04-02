using MSTeams.Karma.BusinessLogic;
using System;
using Xunit;

namespace MSTeams_Karma.Tests
{
    public class KarmaLogicTests
    {
        [Theory]
        [InlineData("<at>Aaron Rosenberger</at>++", "<at>Aaron Rosenberger</at>")]
        [InlineData("<at>Aaron Rosenberger</at> ++", "<at>Aaron Rosenberger</at>")]
        [InlineData("<at>Aaron Rosenberger</at>++++", "<at>Aaron Rosenberger</at>")]
        [InlineData("<at>Aaron Rosenberger</at> ++++", "<at>Aaron Rosenberger</at>")]
        [InlineData("<at>Aaron Rosenberger</at>+++++++++++++++++", "<at>Aaron Rosenberger</at>")]
        [InlineData("<at>Aaron Rosenberger</at> +++++++++++++++++", "<at>Aaron Rosenberger</at>")]
        [InlineData("<at>Aaron Rosenberger</at>--", "<at>Aaron Rosenberger</at>")]
        [InlineData("<at>Aaron Rosenberger</at> --", "<at>Aaron Rosenberger</at>")]
        [InlineData("<at>Aaron Rosenberger</at>----", "<at>Aaron Rosenberger</at>")]
        [InlineData("<at>Aaron Rosenberger</at> ----", "<at>Aaron Rosenberger</at>")]
        [InlineData("<at>Aaron Rosenberger</at>-----------------", "<at>Aaron Rosenberger</at>")]
        [InlineData("<at>Aaron Rosenberger</at> -----------------", "<at>Aaron Rosenberger</at>")]
        [InlineData("<at>Aaron Rosenberger</at>+-", null)]
        [InlineData("<at>Aaron Rosenberger</at> +-", null)]
        [InlineData("<at>Aaron Rosenberger</at>+++-", null)]
        [InlineData("<at>Aaron Rosenberger</at> +++-", null)]
        [InlineData("<at>Aaron Rosenberger</at>++++++++++++++++-", null)]
        [InlineData("<at>Aaron Rosenberger</at> ++++++++++++++++-", null)]
        [InlineData("<at>Aaron Rosenberger</at>-+", null)]
        [InlineData("<at>Aaron Rosenberger</at> -+", null)]
        [InlineData("<at>Aaron Rosenberger</at>---+", null)]
        [InlineData("<at>Aaron Rosenberger</at> ---+", null)]
        [InlineData("<at>Aaron Rosenberger</at>----------------+", null)]
        [InlineData("<at>Aaron Rosenberger</at> ----------------+", null)]
        public void TestSomeoneWasGivenKarma(string message, string expected)
        {
            var result = KarmaLogic.SomeoneWasGivenKarma(message);

            Assert.Equal(result, expected);
        }
    }
}
