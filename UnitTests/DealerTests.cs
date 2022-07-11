using FluentAssertions;
using System.IO;
using System.Threading;
using Xunit;

namespace UnitTests
{
    [Collection("Collection_1")]
    public class DealerTests
    {
        [Theory(DisplayName = "Verify duplicated cards handling")]
        [InlineData(@"InputFiles\InvalidCards\duplicatedCardsOnePlayer.txt", "Name1", "10C")]
        [InlineData(@"InputFiles\InvalidCards\duplicatedCardsDiffPlayers.txt", "Name4", "9C")]
        public void DuplicatedCardsTest(string inputFile, string userName, string card)
        {
            Thread.Sleep(500);
            winner.winner.Main(new string[] { "--in", inputFile, "--out", "out" });
            winner.winner.ErrorMessage.Should().Be($"ERROR: Player {userName} has duplicated card {card}." + "\n" + "Please, try again!");
        }

        [Theory]
        [InlineData(@"InputFiles\ValidInputs\validdeal_1.txt", "Name1:37")]
        [InlineData(@"InputFiles\ValidInputs\validdeal_2.txt", "Name1:16")]
        [InlineData(@"InputFiles\ValidInputs\validdeal_3.txt", "Name2:14")]
        [InlineData(@"InputFiles\ValidInputs\validdeal_4.txt", "Name1,Name2:14")]
        [InlineData(@"InputFiles\ValidInputs\validdeal_5.txt", "Name1:14")]
        [InlineData(@"InputFiles\ValidInputs\validdeal_6.txt", "Name3:15")]
        [InlineData(@"InputFiles\ValidInputs\validdeal_7.txt", "Name1,Name2,Name3:13")]
        public void PostitiveTest(string inputFile, string expectedoutput)
        {
            winner.winner.Main(new string[] { "--in", inputFile, "--out", "output.txt" });
            var actualOutput = File.ReadAllText("output.txt");
            actualOutput.Should().Be(expectedoutput);
        }

        [Theory]
        [InlineData(@"InputFiles\RegexTests\nocoloninrow.txt")]
        [InlineData(@"InputFiles\RegexTests\nocardinrow.txt")]
        [InlineData(@"InputFiles\RegexTests\noplayername.txt")]
        [InlineData(@"InputFiles\InvalidCards\cardwithInvalidFaceX.txt")]
        [InlineData(@"InputFiles\InvalidCards\cardwithInvalidFace11.txt")]
        [InlineData(@"InputFiles\InvalidCards\cardwithInvalidFace1.txt")]
        [InlineData(@"InputFiles/InvalidCards/duplicatedPlayers.txt")]
        [InlineData(@"InputFiles\InvalidCards\cardwithInvalidSuitX.txt")]
        [InlineData(@"InputFiles\InvalidCards\cardwithInvalidSuit2.txt")]
        [InlineData(@"InputFiles\InvalidCards\morethan5players.txt")]
        [InlineData(@"InputFiles\InvalidCards\lessthan5players.txt")]
        public void NegativeTest(string inputFile)
        {
            winner.winner.Main(new string[] { "--in", inputFile, "--out", "output.txt" });
            var actualOutput = File.ReadAllText("output.txt");
            actualOutput.Should().Be("ERROR");
        }
    }
}
