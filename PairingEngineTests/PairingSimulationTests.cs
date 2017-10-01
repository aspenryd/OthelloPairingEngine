using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PairingEngine;
using PairingEngine.Models;

namespace PairingEngineTests
{
    [TestClass]
    public class PairingSimulationTests
    {
        [TestMethod]
        public void GenerateTournament()
        {
            var tournament = PairingSimulation.GenerateTournament(7, 20);
            Assert.AreEqual(7, tournament.NumberOfRounds);
            Assert.AreEqual(20, tournament.Players.Count());
        }
        [TestMethod]
        public void SimulateTournament()
        {
            var numRounds = 13;
            var tournament = PairingSimulation.GenerateTournament(numRounds, 60);
            PairingSimulation.SimulateTournament(tournament);
            Assert.AreEqual(numRounds, tournament.RoundList.Count);
            Assert.AreEqual(numRounds, tournament.Standings.Count);
//            Assert.IsFalse(AnyPlayerMetOpponentTwice(tournament));
            Console.WriteLine($"Top {GetTopPlayersMet(tournament)} played eachother");
            var finalStandings = tournament.Standings.Last();
            PrintStandings(finalStandings);
            PrintRoundByRoundResultsWithStandings(tournament.RoundList, tournament.Standings);

        }



        [TestMethod]
        public void GeneratePlayerList()
        {
            var players = PairingSimulation.GeneratePlayerList(20, 1800, 2200);
            
            Assert.AreEqual(20, players.Count());
            Assert.IsTrue(players.Min(p=>p.Rating)> 1800);
            Assert.IsTrue(players.Max(p => p.Rating) < 2200);
            //Check that all PlayerId are unique
            Assert.AreEqual(players.Count(), players.Select(p=>p.PlayerId).Distinct().Count());
            //Check that all Ratings are unique
            Assert.AreEqual(players.Count(), players.Select(p => p.Rating).Distinct().Count());
        }

        private int GetTopPlayersMet(Tournament tournament)
        {
            for (int i = 0; i < tournament.NumberOfRounds; i++)
            {
                if (!DidTopPlayserMeet(tournament, i))
                    return i;
            }
            return tournament.NumberOfRounds;
        }

        private bool DidTopPlayserMeet(Tournament tournament, int num)
        {            
            var finalResults = tournament.Standings.Last().PlayerResults;
            foreach (var playerResult in finalResults)
            {
                if (finalResults.IndexOf(playerResult) > num) return true;
                var allOpponents = GetAllOpponents(tournament, playerResult.Player);
                for (int j = 0; j < num; j++)
                {
                    if (!allOpponents.Any(p => p.PlayerId == finalResults[j].Player.PlayerId))
                        return false;
                }
            }
            return true;
        }

        private bool AnyPlayerMetOpponentTwice(Tournament tournament)
        {

            foreach (var tournamentPlayer in tournament.Players)
            {
                var allOpponents = GetAllOpponents(tournament, tournamentPlayer);
                if (tournament.NumberOfRounds > allOpponents.Select(p => p.PlayerId).Distinct().Count())
                    return true;
            }
            return false;
        }

        private IEnumerable<Player> GetAllOpponents(Tournament tournament, Player tournamentPlayer)
        {
            var playersGames = PappPairing.GetAllPreviousGames(tournament.RoundList, tournamentPlayer);
            var allOpponents = playersGames.Select(g => GetOpponent(g, tournamentPlayer));
            return allOpponents;
        }

        private Player GetOpponent(Game game, Player tournamentPlayer)
        {
            if (game.BlackPlayer.PlayerId == tournamentPlayer.PlayerId)
                return game.WhitePlayer;
            return game.BlackPlayer;

        }

        private void PrintRoundByRoundResultsWithStandings(IList<Round> tournamentRoundList, IList<RoundResult> tournamentStandings)
        {
            foreach (var round in tournamentRoundList)
            {
                PrintRound(round);
                PrintStandings(tournamentStandings.SingleOrDefault(s => s.RoundNumber == round.RoundNumber));
            }
        }

        private void PrintStandings(RoundResult roundResult)
        {
            Console.WriteLine($"Standings after round {roundResult.RoundNumber}");
            foreach (var results in roundResult.PlayerResults.OrderByDescending(r => r.Score).ThenByDescending(r => r.MBQ))
            {
                Console.WriteLine($"{results.Score} {results.MBQ} {results.Player.PlayerId} ({results.Player.Rating})",
                    results.Score);
            }
            Console.WriteLine("");
        }

        private void PrintRoundByRoundResults(IList<Round> tournamentRoundList)
        {
            foreach (var round in tournamentRoundList)
            {
                PrintRound(round);
            }
        }

        private static void PrintRound(Round round)
        {
            Console.WriteLine($"Round {round.RoundNumber}");
            foreach (var roundGame in round.Games)
            {
                Console.WriteLine(
                    $"{roundGame.BlackPlayer.Rating} {roundGame.BlackResult}-{roundGame.WhiteResult} {roundGame.WhitePlayer.Rating}");
            }
            Console.WriteLine("");
        }

    }
}
