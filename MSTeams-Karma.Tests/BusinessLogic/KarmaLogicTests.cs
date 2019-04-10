using MSTeams.Karma.BusinessLogic;
using System;
using Xunit;

namespace MSTeams_Karma.Tests
{
    public class KarmaLogicTests
    {
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
        [InlineData("<at>Aaron Rosenberger</at>++\n", true)]
        [InlineData("<at>Aaron Rosenberger</at>++\\n", true)]
        [InlineData("<at>Aaron Rosenberger</at>++\r\n", true)]
        [InlineData("<at>Aaron Rosenberger</at>++\\r\\n", true)]
        [InlineData("<at>Aaron Rosenberger</at> ++\n", true)]
        [InlineData("<at>Aaron Rosenberger</at> ++\\n", true)]
        [InlineData("<at>Aaron Rosenberger</at> ++\r\n", true)]
        [InlineData("<at>Aaron Rosenberger</at> ++\\r\\n", true)]
        [InlineData("<at>Aaron Rosenberger</at>--\n", true)]
        [InlineData("<at>Aaron Rosenberger</at>--\\n", true)]
        [InlineData("<at>Aaron Rosenberger</at>--\r\n", true)]
        [InlineData("<at>Aaron Rosenberger</at>--\\r\\n", true)]
        [InlineData("<at>Aaron Rosenberger</at> --\n", true)]
        [InlineData("<at>Aaron Rosenberger</at> --\\n", true)]
        [InlineData("<at>Aaron Rosenberger</at> --\r\n", true)]
        [InlineData("<at>Aaron Rosenberger</at> --\\r\\n", true)]
        public void TestSomeoneWithFullNameWasGivenKarma(string message, bool expected)
        {
            var result = KarmaLogic.SomeoneWasGivenKarma(message);

            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData("<at>Aaron</at>++", true)]
        [InlineData("<at>Aaron</at>++\n", true)]
        [InlineData("<at>Aaron</at>++\\n", true)]
        [InlineData("<at>Aaron</at>++\r\n", true)]
        [InlineData("<at>Aaron</at>++\\r\\n", true)]
        [InlineData("<at>Aaron</at> ++", true)]
        [InlineData("<at>Aaron</at> ++\n", true)]
        [InlineData("<at>Aaron</at> ++\\n", true)]
        [InlineData("<at>Aaron</at> ++\r\n", true)]
        [InlineData("<at>Aaron</at> ++\\r\\n", true)]
        [InlineData("<at>Aaron</at>--", true)]
        [InlineData("<at>Aaron</at>--\n", true)]
        [InlineData("<at>Aaron</at>--\\n", true)]
        [InlineData("<at>Aaron</at>--\r\n", true)]
        [InlineData("<at>Aaron</at>--\\r\\n", true)]
        [InlineData("<at>Aaron</at> --", true)]
        [InlineData("<at>Aaron</at> --\n", true)]
        [InlineData("<at>Aaron</at> --\\n", true)]
        [InlineData("<at>Aaron</at> --\r\n", true)]
        [InlineData("<at>Aaron</at> --\\r\\n", true)]
        public void TestSomeoneWithOneNameWasGivenKarma(string message, bool expected)
        {
            var result = KarmaLogic.SomeoneWasGivenKarma(message);

            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData("msTeams++", true)]
        [InlineData("msTeams++\n", true)]
        [InlineData("msTeams++\\n", true)]
        [InlineData("msTeams++\r\n", true)]
        [InlineData("msTeams++\\r\\n", true)]
        [InlineData("msTeams ++", true)]
        [InlineData("msTeams ++\n", true)]
        [InlineData("msTeams ++\\n", true)]
        [InlineData("msTeams ++\r\n", true)]
        [InlineData("msTeams ++\\r\\n", true)]
        [InlineData("msTeams--", true)]
        [InlineData("msTeams--\n", true)]
        [InlineData("msTeams--\\n", true)]
        [InlineData("msTeams--\r\n", true)]
        [InlineData("msTeams--\\r\\n", true)]
        [InlineData("msTeams --", true)]
        [InlineData("msTeams --\n", true)]
        [InlineData("msTeams --\\n", true)]
        [InlineData("msTeams --\r\n", true)]
        [InlineData("msTeams --\\r\\n", true)]
        [InlineData("\"msTeams\"++", true)]
        [InlineData("\"msTeams\"++\n", true)]
        [InlineData("\"msTeams\"++\\n", true)]
        [InlineData("\"msTeams\"++\r\n", true)]
        [InlineData("\"msTeams\"++\\r\\n", true)]
        [InlineData("\"msTeams\" ++", true)]
        [InlineData("\"msTeams\" ++\n", true)]
        [InlineData("\"msTeams\" ++\\n", true)]
        [InlineData("\"msTeams\" ++\r\n", true)]
        [InlineData("\"msTeams\" ++\\r\\n", true)]
        [InlineData("\"msTeams\"--", true)]
        [InlineData("\"msTeams\"--\n", true)]
        [InlineData("\"msTeams\"--\\n", true)]
        [InlineData("\"msTeams\"--\r\n", true)]
        [InlineData("\"msTeams\"--\\r\\n", true)]
        [InlineData("\"msTeams\" --", true)]
        [InlineData("\"msTeams\" --\n", true)]
        [InlineData("\"msTeams\" --\\n", true)]
        [InlineData("\"msTeams\" --\r\n", true)]
        [InlineData("\"msTeams\" --\\r\\n", true)]
        public void TestSomethingSingleWordWasGivenKarma(string message, bool expected)
        {
            var result = KarmaLogic.SomeoneWasGivenKarma(message);

            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData("\"foo bar\"++", true)]
        [InlineData("\"foo bar\"++\n", true)]
        [InlineData("\"foo bar\"++\\n", true)]
        [InlineData("\"foo bar\"++\r\n", true)]
        [InlineData("\"foo bar\"++\\r\\n", true)]
        [InlineData("\"foo bar\" ++", true)]
        [InlineData("\"foo bar\" ++\n", true)]
        [InlineData("\"foo bar\" ++\\n", true)]
        [InlineData("\"foo bar\" ++\r\n", true)]
        [InlineData("\"foo bar\" ++\\r\\n", true)]
        [InlineData("\"foo bar\"--", true)]
        [InlineData("\"foo bar\"--\n", true)]
        [InlineData("\"foo bar\"--\\n", true)]
        [InlineData("\"foo bar\"--\r\n", true)]
        [InlineData("\"foo bar\"--\\r\\n", true)]
        [InlineData("\"foo bar\" --", true)]
        [InlineData("\"foo bar\" --\n", true)]
        [InlineData("\"foo bar\" --\\n", true)]
        [InlineData("\"foo bar\" --\r\n", true)]
        [InlineData("\"foo bar\" --\\r\\n", true)]
        public void TestSomethingTwoWordWasGivenKarma(string message, bool expected)
        {
            var result = KarmaLogic.SomeoneWasGivenKarma(message);

            Assert.Equal(result, expected);
        }

        [Theory]
        [InlineData("\"foo bar baz foobarbaz\"++", true)]
        [InlineData("\"foo bar baz foobarbaz\"++\n", true)]
        [InlineData("\"foo bar baz foobarbaz\"++\\n", true)]
        [InlineData("\"foo bar baz foobarbaz\"++\r\n", true)]
        [InlineData("\"foo bar baz foobarbaz\"++\\r\\n", true)]
        [InlineData("\"foo bar baz foobarbaz\" ++", true)]
        [InlineData("\"foo bar baz foobarbaz\" ++\n", true)]
        [InlineData("\"foo bar baz foobarbaz\" ++\\n", true)]
        [InlineData("\"foo bar baz foobarbaz\" ++\r\n", true)]
        [InlineData("\"foo bar baz foobarbaz\" ++\\r\\n", true)]
        [InlineData("\"foo bar baz foobarbaz\"--", true)]
        [InlineData("\"foo bar baz foobarbaz\"--\n", true)]
        [InlineData("\"foo bar baz foobarbaz\"--\\n", true)]
        [InlineData("\"foo bar baz foobarbaz\"--\r\n", true)]
        [InlineData("\"foo bar baz foobarbaz\"--\\r\\n", true)]
        [InlineData("\"foo bar baz foobarbaz\" --", true)]
        [InlineData("\"foo bar baz foobarbaz\" --\n", true)]
        [InlineData("\"foo bar baz foobarbaz\" --\\n", true)]
        [InlineData("\"foo bar baz foobarbaz\" --\r\n", true)]
        [InlineData("\"foo bar baz foobarbaz\" --\\r\\n", true)]
        public void TestSomethingMultiWordWasGivenKarma(string message, bool expected)
        {
            var result = KarmaLogic.SomeoneWasGivenKarma(message);

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
        [InlineData("foobarbaz\"++\n", false)]
        [InlineData("foobarbaz\"++\\n", false)]
        [InlineData("foobarbaz\"++\r\n", false)]
        [InlineData("foobarbaz\"++\\r\\n", false)]
        [InlineData("foobarbaz\" ++\n", false)]
        [InlineData("foobarbaz\" ++\\n", false)]
        [InlineData("foobarbaz\" ++\r\n", false)]
        [InlineData("foobarbaz\" ++\\r\\n", false)]
        [InlineData("foobarbaz\"--\n", false)]
        [InlineData("foobarbaz\"--\\n", false)]
        [InlineData("foobarbaz\"--\r\n", false)]
        [InlineData("foobarbaz\"--\\r\\n", false)]
        [InlineData("foobarbaz\" --\n", false)]
        [InlineData("foobarbaz\" --\\n", false)]
        [InlineData("foobarbaz\" --\r\n", false)]
        [InlineData("foobarbaz\" --\\r\\n", false)]
        [InlineData("\"foo\" bar baz foobarbaz\"++\n", false)]
        [InlineData("\"foo\" bar baz foobarbaz\"++\\n", false)]
        [InlineData("\"foo\" bar baz foobarbaz\"++\r\n", false)]
        [InlineData("\"foo\" bar baz foobarbaz\"++\\r\\n", false)]
        [InlineData("\"foo\" bar baz foobarbaz\" ++\n", false)]
        [InlineData("\"foo\" bar baz foobarbaz\" ++\\n", false)]
        [InlineData("\"foo\" bar baz foobarbaz\" ++\r\n", false)]
        [InlineData("\"foo\" bar baz foobarbaz\" ++\\r\\n", false)]
        [InlineData("\"foo\" bar baz foobarbaz\"--\n", false)]
        [InlineData("\"foo\" bar baz foobarbaz\"--\\n", false)]
        [InlineData("\"foo\" bar baz foobarbaz\"--\r\n", false)]
        [InlineData("\"foo\" bar baz foobarbaz\"--\\r\\n", false)]
        [InlineData("\"foo\" bar baz foobarbaz\" --\n", false)]
        [InlineData("\"foo\" bar baz foobarbaz\" --\\n", false)]
        [InlineData("\"foo\" bar baz foobarbaz\" --\r\n", false)]
        [InlineData("\"foo\" bar baz foobarbaz\" --\\r\\n", false)]
        public void TestSomethingMultiWordWasNOTGivenKarma(string message, bool expected)
        {
            var result = KarmaLogic.SomeoneWasGivenKarma(message);

            Assert.Equal(result, expected);
        }
    }
}
