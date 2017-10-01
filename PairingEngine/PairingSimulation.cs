using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using PairingEngine.Models;
using TestSimpleRNG;

namespace PairingEngine
{
    public class PairingSimulation
    {
        readonly static Random _rng = new Random((int)DateTime.Now.Ticks);

        public static Tournament GenerateTournament(int numRounds, int numPlayers)
        {
            return new Tournament()
            {
                NumberOfRounds = numRounds,
                Players = GeneratePlayerList(numPlayers, 1600, 2500),
                RoundList = new List<Round>()
            };
        }

        public static void SimulateTournament(Tournament tournament)
        {
            while (tournament.RoundList.Count < tournament.NumberOfRounds)
            {
                PairRoundAndGenerateResults(tournament);
                CalculateStandings(tournament);
            }
        }

        private static void CalculateStandings(Tournament tournament)
        {
            var lastResult = tournament.RoundList.Last();
            var lastStanding = tournament.Standings?.Last();
            if (tournament.Standings == null)
                tournament.Standings = new List<RoundResult>();
            var roundresults = new RoundResult() {RoundNumber = tournament.RoundList.Last().RoundNumber};

            foreach (var lastResultGame in lastResult.Games)
            {
                var blackStandings =
                    lastStanding?.PlayerResults?.SingleOrDefault(
                        p => p.Player.PlayerId == lastResultGame.BlackPlayer.PlayerId);
                var whiteStandings =
                    lastStanding?.PlayerResults?.SingleOrDefault(
                        p => p.Player.PlayerId == lastResultGame.WhitePlayer.PlayerId);
                
                roundresults.PlayerResults.Add(CreateNewPlayerResults(lastResultGame.BlackPlayer, blackStandings, CalcScore(lastResultGame.BlackResult), CalcMbq(lastResultGame.BlackResult, whiteStandings)));
                roundresults.PlayerResults.Add(CreateNewPlayerResults(lastResultGame.WhitePlayer, whiteStandings, CalcScore(lastResultGame.WhiteResult), CalcMbq(lastResultGame.WhiteResult, blackStandings)));
            }
            roundresults.PlayerResults = roundresults.PlayerResults.OrderByDescending(r => r.Score).ThenByDescending(r => r.MBQ).ToList();
            tournament.Standings.Add(roundresults);
        }

        private static double CalcScore(int result)
        {
            if (result == 32) return 0.5;
            if (result > 32) return 1;
            return 0;
        }

        private static PlayerResult CreateNewPlayerResults(Player player, PlayerResult lastStanding, double newScore, double newMbq)
        {
            return new PlayerResult()
            {
                Player = player,
                Score = lastStanding != null ? lastStanding.Score + newScore : newScore,
                MBQ = lastStanding != null ? lastStanding.MBQ + newMbq : newMbq,
            };
        }

        private static double CalcMbq(int discResult, PlayerResult whiteStandings)
        {
            if (whiteStandings == null)
                return discResult;
            return discResult + whiteStandings.Score * 6;
        }

        public static void PairRoundAndGenerateResults(Tournament tournament)
        {
            var games = new List<Game>();
            PappPairing.PairNextRound(tournament);
            var pairdGames = tournament.RoundList.Last();
            foreach (var game in pairdGames.Games)
            {
                var resultGame = game;
                RandomizeResult(ref resultGame);
                games.Add(resultGame);
            }
            tournament.RoundList.Last().Games = games;
        }


        public static IEnumerable<Player> GeneratePlayerList(int count, int minrating, int maxrating)
        {
            var list = new List<Player>();
            for (var i = 0; i < count; i++)
            {
                list.Add(new Player(1000+i, GenerateRating(minrating, maxrating)));
            }
            return list;
        }

        

        public static void RandomizeResult(ref Game game)
        {
            //SimpleRNG.SetSeedFromSystemTime();
            var blackFavor = (game.BlackPlayer.Rating - game.WhitePlayer.Rating)/40;
            var simulatedResult = SimpleRNG.GetNormal(32, 7) + blackFavor;
            game.BlackResult = MakeProperResult(simulatedResult);
            game.WhiteResult = 64 - game.BlackResult;
        }

        private static int MakeProperResult(double simulatedResult)
        {
            if (simulatedResult < 0) return 0;
            if (simulatedResult > 64) return 64;
            return (int) simulatedResult;
        }
        private static int GenerateRating(int minrating, int maxrating)
        {            
            return _rng.Next(minrating, maxrating);
        }
    }
}
