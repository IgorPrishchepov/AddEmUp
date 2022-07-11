using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace winner
{
    public class winner
    {
        public static string ErrorMessage;

        public static void Main(string[] args)
        {
            try
            {
                var dealedPlayers = CliParser.ParseCli(args);
                var winners = new Dealer().StartGame(dealedPlayers);
                OutputHandler.PublishGameResults(args, winners);
            }
            catch(Exception ex)
            {
                ErrorMessage = string.Format("ERROR: {0}." + "\n" + "Please, try again!", ex.Message);
                OutputHandler.PublishError(args);
                Console.WriteLine(ErrorMessage);
            }
           
        }
    }

    static class CliParser
    {
        internal static IEnumerable<Player> ParseCli(string[] args)
        {
            ValidateInputParams(args);
            var playerRows = ParseInputFile(args);
            return InitializePlayers(playerRows);
        }

        static void ValidateInputParams(string[] args)
        {
            if (args.Length != 4)
            {
                throw new Exception("Command line args must be of format '--in inputFile --out outputFile' or ''--out outputFile --in inputFile'");
            }

            args.ToList().ForEach(a => a = a.ToLowerInvariant());

            ValidateCommandFormat(args, SharedData.In, SharedData.Out);
        }

        static void ValidateCommandFormat(string[] args, params string[] commands)
        {
            commands.ToList().ForEach(command =>
            {
                if (!args.Contains(command))
                {
                    throw new Exception(string.Format("Command line args must contain '{0}' command", command));
                }

                if (args.ToList().Where(a => a.Equals(command)).Count() > 1)
                {
                    throw new Exception(string.Format("Command line args must contain only 1 '{0}' command", command));
                }

                var inputArgIndex = args.ToList().IndexOf(command);
                if (inputArgIndex == 1 || inputArgIndex == 3)
                {
                    throw new Exception("Command line args must be of format '--in inputFile --out outputFile' or ''--out outputFile --in inputFile'");
                }
            });
        }


        static string[] ParseInputFile(string[] args)
        {
            var filePath = args[args.ToList().IndexOf(SharedData.In) + 1];

            string[] inputFileLines = File.ReadAllLines(filePath);
            if (inputFileLines.Length != 5) 
            {
                throw new Exception(string.Format("Input file '{0}' must contain 5 rows, one for each player card's hand, but has {1} rows",
                    filePath, inputFileLines.Length));
            }

            var regex = new Regex(@"\S+:(.{2,3},){4}.{2,3}$");

            inputFileLines.ToList().ForEach(r =>
            {
                if (!regex.IsMatch(r))
                {
                    throw new Exception(string.Format("Input file contains incorrect data in row: {0}", r));
                }
            });

            return inputFileLines;
        }

        static IEnumerable<Player> InitializePlayers(string[] playerRows)
        {
            var dealedPlayers = new List<Player>();
            playerRows.ToList().ForEach(row =>
            {
                var playerName = row.Split(':')[0];

                if (dealedPlayers.Any(p => p.Name.Equals(playerName)))
                {
                    throw new Exception(string.Format("There are duplicate player names {0}", playerName));
                }

                var player = new Player(name: row.Split(':')[0]);

                var splittedRow = row.Split(':');

                var playerHand = splittedRow[1].Split(',');

                playerHand.ToList().ForEach(card =>
                {
                    string face, suit;
                    if (card.Count() == 3)
                    {
                        face = (card[0].ToString() + card[1].ToString()).ToUpperInvariant().Trim();
                        suit = card[2].ToString().ToUpperInvariant().Trim();
                    }
                    else
                    {
                        face = card[0].ToString().ToUpperInvariant().Trim();
                        suit = card[1].ToString().ToUpperInvariant().Trim();
                    }

                    if (!SharedData.Faces.ContainsKey(face))
                    {
                        throw new Exception(string.Format("One of players {0} has card with invalid face {1}", player.Name, face));
                    }

                    if (!SharedData.Suits.ContainsKey(suit))
                    {
                        throw new Exception(string.Format("One of players {0} has card with invalid suit {1}", player.Name, suit));
                    }

                    player.Hand.Add(new Card(face, suit));
                });

                dealedPlayers.Add(player);
            });

            return dealedPlayers;
        }
    }

    class Dealer
    {
        readonly IList<Card> dealedCards = new List<Card>();
        List<Player> _validPlayers;

        internal IEnumerable<Player> StartGame(IEnumerable<Player> players)
        {
            ValidatePlayerHands(players);
            _validPlayers = players.ToList();
            return DefineWinners();
        }

        void ValidatePlayerHands(IEnumerable<Player> players)
        {
            players.ToList().ForEach(player =>
            {
                player.Hand.ToList().ForEach(card =>
                {
                    if (dealedCards.Any(c => c.Suit == card.Suit && c.Face == card.Face))
                    {
                        throw new Exception(string.Format("Player {0} has duplicated card {1}{2}", player.Name, card.Face, card.Suit));
                    }
                    else
                    {
                        dealedCards.Add(card);
                    }
                });
            });
        }

        IEnumerable<Player> DefineWinners()
        {
            var calculatedPlayersByFace = _validPlayers.Select(p => new Player(p.Name) { Hand = p.Hand, Score = CalculateHandValueByFace(p.Hand) });
            var winnersByFaces = calculatedPlayersByFace.Where(p => p.Score == calculatedPlayersByFace.Max(h => h.Score));

            if (winnersByFaces.Count() == 1)
            {
                return winnersByFaces;
            }

            else
            {
                return DefineWinnersAmongTiedPlayers(winnersByFaces);
            }
        }

        IEnumerable<Player> DefineWinnersAmongTiedPlayers(IEnumerable<Player> winnersByFaces)
        {
            var calculatedPlayersBySuit = winnersByFaces.Select(w => new Player(w.Name) { Hand = w.Hand, Score = CalculateHandValueBySuit(w.Hand) });
            return calculatedPlayersBySuit.Where(w => w.Score == calculatedPlayersBySuit.Max(h => h.Score));
        }

        int CalculateHandValueByFace(IEnumerable<Card> hand) 
        {
            return hand.Sum(card => SharedData.Faces[card.Face]);
        }

        int CalculateHandValueBySuit(IEnumerable<Card> hand)
        {
            return hand.Sum(card => SharedData.Suits[card.Suit]);
        }



    }

    static class OutputHandler
    {
        internal static void PublishGameResults(string[] args, IEnumerable<Player> winners)
        {
            var outputFilePath = args[3];
            var outputMessage = string.Empty;

            if (winners.Count() == 1)
            {
                var winPlayer = winners.First();
                outputMessage = string.Format("{0}:{1}", winPlayer.Name, winPlayer.Score);
            }
            else
            {
                winners.ToList().ForEach(w =>
                {
                    outputMessage += string.Format("{0},", w.Name);
                });
                outputMessage = outputMessage.Remove(startIndex: outputMessage.Length - 1);
                outputMessage += string.Format(":{0}", winners.First().Score);
            }

            File.WriteAllText(outputFilePath, outputMessage);
        }

        internal static void PublishError(string[] args)
        {
            string outputFilePath = null;
            try
            {
                outputFilePath = args[3];
            }
            catch { }

            if (outputFilePath != null) File.WriteAllText(outputFilePath, "ERROR");
        }
    }
    class Player
    {
        public Player(string name)
        {
            Name = name;
            Hand = new List<Card>();
            Score = 0;
        }

        internal string Name { get; private set; }
        internal IList<Card> Hand { get; set; }
        internal int Score { get; set; }

    }

    class Card
    {
        public Card(string face, string suit)
        {
            Face = face;
            Suit = suit;
        }
        internal string Face { get; private set; }
        internal string Suit { get; private set; }
    }

    static class SharedData
    {
        internal const string In = "--in";
        internal const string Out = "--out";

        internal static IDictionary<string, int> Suits = new Dictionary<string, int>
        {
            { "S", 4 },
            { "H", 3 },
            { "D", 2 },
            { "C", 1 }
        };

        internal static IDictionary<string, int> Faces = new Dictionary<string, int>
        {
            { "2", 2 },
            { "3", 3 },
            { "4", 4 },
            { "5", 5 },
            { "6", 6 },
            { "7", 7 },
            { "8", 8 },
            { "9", 9 },
            { "10", 10 },
            { "J", 11 },
            { "Q", 12 },
            { "K", 13 },
            { "A", 1 },
        };
    }
}
