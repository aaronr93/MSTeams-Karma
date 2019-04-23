using MSTeams.Karma.BusinessLogic;
using System;
using Xunit;

namespace MSTeams.Karma.Tests.BusinessLogic
{
    public class KarmaLogicTests
    {
        [Theory]
        [InlineData("test? <at>karma</at> <at>Aaron Rosenberger</at>+++", true)]
        [InlineData("test? <at>karma</at> <at>Aaron Rosenberger</at> +++", true)]
        public void TestSomeoneReceivedKarmaInWholeMessage(string message, bool expected)
        {
            var result = KarmaLogic.SomeoneReceivedKarmaInWholeMessage(message);

            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData("<at>Aaron Rosenberger</at>++", true)]
        [InlineData("<at>Aaron Rosenberger</at> ++", true)]
        [InlineData("<at>Aaron Rosenberger</at>++++", true)]
        [InlineData("<at>Aaron Rosenberger</at> ++++", true)]
        [InlineData("<at>Aaron Rosenberger</at>+++++++++++++++++", true)]
        [InlineData("<at>Aaron Rosenberger</at> +++++++++++++++++", true)]
        [InlineData("<at>Aaron Rosenberger</at>--", true)]
        [InlineData("<at>Aaron Rosenberger</at> --", true)]
        [InlineData("<at>Aaron Rosenberger</at>----", true)]
        [InlineData("<at>Aaron Rosenberger</at> ----", true)]
        [InlineData("<at>Aaron Rosenberger</at>-----------------", true)]
        [InlineData("<at>Aaron Rosenberger</at> -----------------", true)]
        [InlineData("<at>Aaron Rosenberger</at>+-", false)]
        [InlineData("<at>Aaron Rosenberger</at> +-", false)]
        [InlineData("<at>Aaron Rosenberger</at>+++-", false)]
        [InlineData("<at>Aaron Rosenberger</at> +++-", false)]
        [InlineData("<at>Aaron Rosenberger</at>++++++++++++++++-", false)]
        [InlineData("<at>Aaron Rosenberger</at> ++++++++++++++++-", false)]
        [InlineData("<at>Aaron Rosenberger</at>-+", false)]
        [InlineData("<at>Aaron Rosenberger</at> -+", false)]
        [InlineData("<at>Aaron Rosenberger</at>---+", false)]
        [InlineData("<at>Aaron Rosenberger</at> ---+", false)]
        [InlineData("<at>Aaron Rosenberger</at>----------------+", false)]
        [InlineData("<at>Aaron Rosenberger</at> ----------------+", false)]
        public void TestSomeoneWithFullNameIsKarmaString(string message, bool expected)
        {
            var result = KarmaLogic.IsKarmaString(message);

            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData("<at>Aaron</at>++", true)]
        [InlineData("<at>Aaron</at> ++", true)]
        [InlineData("<at>Aaron</at>--", true)]
        [InlineData("<at>Aaron</at> --", true)]
        public void TestSomeoneWithOneNameIsKarmaString(string message, bool expected)
        {
            var result = KarmaLogic.IsKarmaString(message);

            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData("msTeams++", true)]
        [InlineData("msTeams ++", true)]
        [InlineData("msTeams--", true)]
        [InlineData("msTeams --", true)]
        [InlineData("\"msTeams\"++", true)]
        [InlineData("\"msTeams\" ++", true)]
        [InlineData("\"msTeams\"--", true)]
        [InlineData("\"msTeams\" --", true)]
        public void TestSomethingSingleWordIsKarmaString(string message, bool expected)
        {
            var result = KarmaLogic.IsKarmaString(message);

            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData("\"foo bar\"++", true)]
        [InlineData("\"foo bar\" ++", true)]
        [InlineData("\"foo bar\"--", true)]
        [InlineData("\"foo bar\" --", true)]
        public void TestSomethingTwoWordIsKarmaString(string message, bool expected)
        {
            var result = KarmaLogic.IsKarmaString(message);

            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData("\"foo bar baz foobarbaz\"++", true)]
        [InlineData("\"foo bar baz foobarbaz\" ++", true)]
        [InlineData("\"foo bar baz foobarbaz\"--", true)]
        [InlineData("\"foo bar baz foobarbaz\" --", true)]
        public void TestSomethingMultiWordIsKarmaString(string message, bool expected)
        {
            var result = KarmaLogic.IsKarmaString(message);

            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData("foobarbaz\"++", false)]
        [InlineData("foobarbaz\" ++", false)]
        [InlineData("foobarbaz\"--", false)]
        [InlineData("foobarbaz\" --", false)]
        [InlineData("\"foo\" bar baz foobarbaz\"++", false)]
        [InlineData("\"foo\" bar baz foobarbaz\" ++", false)]
        [InlineData("\"foo\" bar baz foobarbaz\"--", false)]
        [InlineData("\"foo\" bar baz foobarbaz\" --", false)]
        public void TestSomethingMultiWordWasNOTGivenKarma(string message, bool expected)
        {
            var result = KarmaLogic.IsKarmaString(message);

            Assert.Equal(result, expected);
        }
    }
}
