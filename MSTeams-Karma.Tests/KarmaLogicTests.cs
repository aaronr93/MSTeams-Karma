﻿using MSTeams.Karma.BusinessLogic;
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
        public void TestSomeoneWasGivenKarma(string message, bool expected)
        {
            var result = KarmaLogic.SomeoneWasGivenKarma(message);

            Assert.Equal(result, expected);
        }
    }
}
