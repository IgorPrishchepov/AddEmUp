using FluentAssertions;
using System;
using System.Threading;
using Xunit;

namespace UnitTests
{
    [Collection("Collection_1")]
    public class ParseCliTests
    {
        [Theory(DisplayName = "args != 4")]
        [InlineData("")]
        [InlineData("--in")]
        [InlineData("--in asrer --we we we")]
        [InlineData("--in wewe asrer")]
        [InlineData("wer --in --out srre")]
        [InlineData("--out erer erete --in")]
        [InlineData("--in --out wew srre")]
        [InlineData("--in erer erete --out")]
        [InlineData("lj --in wew --out")]
        [InlineData("erer --out erete --in")]
        public void NotFourArgsTest(string args)
        {
            var cli = args.Split(' ');
            winner.winner.Main(cli);
            winner.winner.ErrorMessage.Should().Be("ERROR: Command line args must be of format '--in inputFile --out outputFile' or ''--out outputFile --in inputFile'." + "\n" + "Please, try again!");
        }

        [Theory(DisplayName = "args not contain --in")]
        [InlineData("-in we --out ewr")]
        [InlineData("--out reer ere erer")]
        public void ArgsNotHaveInTest(string args)
        {
            var cli = args.Split(' ');
            winner.winner.Main(cli);
            winner.winner.ErrorMessage.Should().Be("ERROR: Command line args must contain '--in' command." + "\n" + "Please, try again!");
        }

        [Theory(DisplayName = "args not contain --in")]
        [InlineData("--in we -out ewr")]
        [InlineData("--in reer ere erer")]
        public void ArgsNotHaveOutTest(string args)
        {
            var cli = args.Split(' ');
            winner.winner.Main(cli);
            winner.winner.ErrorMessage.Should().Be("ERROR: Command line args must contain '--out' command." + "\n" + "Please, try again!");
        }

        [Fact(DisplayName = "args contain more than 1 --out")]
        public void MoreThanOneOutTest()
        {
            winner.winner.Main(new string[] { "--out", "sew", "--in", "--out" });
            winner.winner.ErrorMessage.Should().Be("ERROR: Command line args must contain only 1 '--out' command." + "\n" + "Please, try again!");
        }

        [Fact(DisplayName = "args contain more than 1 --in")]
        public void MoreThanOneInTest()
        {
            winner.winner.Main(new string[] { "--in", "wwwr", "--out", "--in" });
            winner.winner.ErrorMessage.Should().Be("ERROR: Command line args must contain only 1 '--in' command." + "\n" + "Please, try again!");
        }
    }
}
