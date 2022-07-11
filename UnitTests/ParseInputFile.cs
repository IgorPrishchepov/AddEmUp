using FluentAssertions;
using System;
using System.Threading;
using Xunit;

namespace UnitTests
{
    [Collection("Collection_1")]
    public class ParseInputFile
    {
        [Fact(DisplayName = "No file found")]
        public void NoInputFileFoundTest()
        {
            winner.winner.Main(new string[] { "--in", "inputFile", "--out", "wew" });
            winner.winner.ErrorMessage.Should().Be($@"ERROR: Could not find file '{AppDomain.CurrentDomain.BaseDirectory}inputFile'.." + "\n" + "Please, try again!");
        }

        [Theory(DisplayName = "File with incorrect number of rows")]
        [InlineData(@"InputFiles\4rowsfile.txt", 4)]
        [InlineData(@"InputFiles\emptyFile.txt", 0)]
        public void InputFileNoRowsTest(string fileName, int numOfRows)
        {
            winner.winner.Main(new string[] { "--in", fileName, "--out", "wew" });
            winner.winner.ErrorMessage.Should().Be($@"ERROR: Input file '{fileName}' must contain 5 rows, one for each player card's hand, but has {numOfRows} rows." + "\n" + "Please, try again!");
        }

        [Theory(DisplayName = "File with rows not matching mask: name:[],[],[],[],[]")]
        [InlineData(@"InputFiles\RegexTests\nocoloninrow.txt", "Name36S,8D,3D,JH,2D")]
        [InlineData(@"InputFiles\RegexTests\nocardinrow.txt", "Name3:6S,8D,3D,JH")]
        [InlineData(@"InputFiles\RegexTests\noplayername.txt", ":5H,3S,KH,AS,9D")]
        public void InputFileInvalidRowsTest(string fileName, string invalidRow)
        {
            winner.winner.Main(new string[] { "--in", fileName, "--out", "wew" });
            winner.winner.ErrorMessage.Should().Be($@"ERROR: Input file contains incorrect data in row: {invalidRow}." + "\n" + "Please, try again!");
        }

        [Theory(DisplayName = "File with rows not matching mask: name:[],[],[],[],[]")]
        [InlineData(@"InputFiles\InvalidCards\cardwithInvalidFaceX.txt", "Name3", "X")]
        [InlineData(@"InputFiles\InvalidCards\cardwithInvalidFace11.txt", "Name4", "11")]
        [InlineData(@"InputFiles\InvalidCards\cardwithInvalidFace1.txt", "Name2", "1")]
        public void InputFileInvalidCardFaceTest(string fileName, string userName, string invalidFace)
        {
            winner.winner.Main(new string[] { "--in", fileName, "--out", "wew" });
            winner.winner.ErrorMessage.Should().Be($@"ERROR: One of players {userName} has card with invalid face {invalidFace}." + "\n" + "Please, try again!");
        }

        [Theory(DisplayName = "File with rows not matching mask: name:[],[],[],[],[]")]
        [InlineData(@"InputFiles\InvalidCards\cardwithInvalidSuitX.txt", "Name1", "A")]
        [InlineData(@"InputFiles\InvalidCards\cardwithInvalidSuit2.txt", "Name1", "2")]

        public void InputFileInvalidCardSuitTest(string fileName, string userName, string invalidSuit)
        {
            winner.winner.Main(new string[] { "--in", fileName, "--out", "wew" });
            winner.winner.ErrorMessage.Should().Be($@"ERROR: One of players {userName} has card with invalid suit {invalidSuit}." + "\n" + "Please, try again!");
        }

        [Fact]
        public void InputFileDuplicatedPlayerNamesTest()
        {
            winner.winner.Main(new string[] { "--in", "InputFiles/InvalidCards/duplicatedPlayers.txt", "--out", "wew" });
            winner.winner.ErrorMessage.Should().Be($@"ERROR: There are duplicate player names Name2." + "\n" + "Please, try again!");
        }
    }
}
