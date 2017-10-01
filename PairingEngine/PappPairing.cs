using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using PairingEngine.Models;

namespace PairingEngine
{
    public class PappPairing
    {
        public static void PairNextRound(Tournament tournament)
        {
            var lastRoundsResults = tournament.Standings != null ? tournament.Standings.Last() : null;
            tournament.RoundList.Add(PairNextRound(tournament, lastRoundsResults, tournament.Players));
        }

        public static Round PairNextRound(Tournament tournament, RoundResult lastRoundsResults, IEnumerable<Player> players)
        {
            var round = new Round();
            round.RoundNumber = lastRoundsResults == null ? 1 : lastRoundsResults.RoundNumber + 1;

            var allPlayers = players.OrderByDescending(p=>p.Rating).ToList();
            if (lastRoundsResults != null)
            {
                var results = lastRoundsResults.PlayerResults.OrderByDescending(r => r.Score).ThenByDescending(r => r.MBQ);
                allPlayers = results.Select(p=>p.Player).ToList();
            }

            var playersLeft = allPlayers.Select(p=>p).ToList();
            foreach (var playerToPair in allPlayers)
            {

                if (playersLeft.Any() && playersLeft.Any(player => player.PlayerId == playerToPair.PlayerId))
                {
                    var pairedGame = GetBestMatch(round.RoundNumber, playersLeft, playerToPair, tournament);
                    RemovePlayer(playersLeft, pairedGame.BlackPlayer.PlayerId);
                    RemovePlayer(playersLeft, pairedGame.WhitePlayer.PlayerId);
                    round.Games.Add(pairedGame);
                }
            }
            return round;
        }

        private static Game GetBestMatch(int roundNumber, List<Player> playersLeft, Player player, Tournament tournament)
        {
            Player whitePlayer;
            Player blackPlayer;
            var playerGames = GetAllPreviousGames(tournament.RoundList, player);
            if (MostlyPlayedAs(playerGames, player) == GameColorResult.Black)
            {
                whitePlayer = player;
                blackPlayer = GetOpponent(GameColorResult.Black, player, playersLeft, tournament.RoundList, tournament.Standings);
            }
            else
            {
                blackPlayer = player;
                whitePlayer = GetOpponent(GameColorResult.White, player, playersLeft, tournament.RoundList, tournament.Standings);
            }
                    
            return new Game(roundNumber + 1, blackPlayer, whitePlayer);
        }

        private static Player GetOpponent(GameColorResult wantedColorResult, Player playerToPair, List<Player> playersLeft, IEnumerable<Round> previousRounds, IEnumerable<RoundResult> standings)
        {
            var opponentsByWeight = new List<OpponentWeight>();
            foreach (var player in playersLeft)
            {
                var opponenWeight = CalculateWeight(player, playerToPair, wantedColorResult, previousRounds, standings);
                opponentsByWeight.Add(new OpponentWeight(player, opponenWeight));
            }

            var orderedByWeight = opponentsByWeight.OrderBy(o => o.Weight).ToList();
            return orderedByWeight.First().Player;            
        }

        private static double CalculateWeight(Player player, Player playerToPair, GameColorResult wantedColorResult, IEnumerable<Round> previousRounds, IEnumerable<RoundResult> standings)
        {
            var weight = 1d;
            if (player.PlayerId == playerToPair.PlayerId) return 1000000;
            var lastStandings = standings?.Last();
            var roundNumber = previousRounds.Any() ? previousRounds.Last().RoundNumber+1 : 1;
            var playerGames = GetAllPreviousGames(previousRounds, player);
            weight += ScorePenalty(roundNumber, lastStandings, player, playerToPair);
            weight += FloatingPenalty(previousRounds, standings, player, playerToPair);
            weight += OpponentPenalty(NumberOfTimesPlayerMetPreviously(playerToPair, playerGames));
            weight += ColorPenalty(wantedColorResult, MostlyPlayedAs(playerGames, player));
            weight += CountryPenalty(roundNumber, player, playerToPair);
            return weight;
        }

        private static double FloatingPenalty(IEnumerable<Round> previousRounds, IEnumerable<RoundResult> standings, Player player, Player playerToPair)
        {
            if (standings == null) return 0;
            var lastStandings = standings.Last();
            var standingP1 = lastStandings.PlayerResults.SingleOrDefault(p => p.Player.PlayerId == player.PlayerId);
            var standingP2 = lastStandings.PlayerResults.SingleOrDefault(p => p.Player.PlayerId == playerToPair.PlayerId);
            
            var diff = standingP2.Score - standingP1.Score;
            if (diff == 0) return 0;
            var totalFloat = CalcFloat(player, previousRounds, standings);
            if (totalFloat == 0) return 0;
            if (diff > 0 && totalFloat > 0) return 1;
            if (diff > 0 && totalFloat < 0) return -1;
            if (diff < 0 && totalFloat < 0) return 1;
            if (diff < 0 && totalFloat > 0) return -1;
            return 0;
        }

        private static double CalcFloat(Player player, IEnumerable<Round> previousRounds, IEnumerable<RoundResult> standings)
        {
            var flot = 0d;
            foreach (var round in previousRounds)
            {
                var game =
                    round.Games.SingleOrDefault(
                        g => g.BlackPlayer.PlayerId == player.PlayerId || g.WhitePlayer.PlayerId == player.PlayerId);
                if (game.Round == 1)
                    flot += 0;
                else
                {
                    var prevStanding = standings.SingleOrDefault(s => s.RoundNumber == game.Round - 1);
                    if (prevStanding != null)
                    {
                        var otherPlayer = game.BlackPlayer.PlayerId == player.PlayerId
                            ? game.WhitePlayer
                            : game.BlackPlayer;
                        var standingP1 = prevStanding.PlayerResults.SingleOrDefault(p => p.Player.PlayerId == player.PlayerId);
                        var standingP2 = prevStanding.PlayerResults.SingleOrDefault(p => p.Player.PlayerId == otherPlayer.PlayerId);
                        flot += standingP2.Score - standingP1.Score;
                    }
                }
            }
            return flot;
        }


        private static double CountryPenalty(int roundNumber, Player player, Player playerToPair)
        {
            if (player.Country != playerToPair.Country) return 0;
            return 0.1*roundNumber;
        }

        private static int OpponentPenalty(int numberOfTimesPlayerMetPreviously)
        {
            return 5*numberOfTimesPlayerMetPreviously;
        }

        private static double ScorePenalty(int roundNumber, RoundResult lastStandings, Player player, Player playerToPair)
        {
            if (lastStandings == null) return 0;
            var standingP1 = lastStandings.PlayerResults.SingleOrDefault(p => p.Player.PlayerId == player.PlayerId);
            var standingP2 = lastStandings.PlayerResults.SingleOrDefault(p => p.Player.PlayerId == playerToPair.PlayerId);
            return Math.Abs(standingP2.Score - standingP1.Score)*roundNumber/2;
        }

        private static int ColorPenalty(GameColorResult wantedColorResult, GameColorResult mostlyPlayedAs)
        {
            if (mostlyPlayedAs == GameColorResult.Draw) return 0;
            if (mostlyPlayedAs == wantedColorResult) return -1;
            return 1;
        }

        private static int NumberOfTimesPlayerMetPreviously(Player playerToPair, IEnumerable<Game> previousRounds)
        {
            if (previousRounds == null || previousRounds.Count() == 0) return 0;
            return previousRounds.Count(g => g.BlackPlayer.PlayerId == playerToPair.PlayerId
                                           || g.WhitePlayer.PlayerId == playerToPair.PlayerId);
        }

        private static GameColorResult MostlyPlayedAs(IEnumerable<Game> playerGames, Player player)
        {
            var blackCount = playerGames.Count(game => game.BlackPlayer.PlayerId == player.PlayerId);
            var whiteCount = playerGames.Count(game => game.WhitePlayer.PlayerId == player.PlayerId);
            if (blackCount > whiteCount) return GameColorResult.Black;
            if (blackCount < whiteCount) return GameColorResult.White;
            return GameColorResult.Draw;
        }

        public static IEnumerable<Game> GetAllPreviousGames(IEnumerable<Round> previousRounds, Player player)
        {
            return from round in previousRounds from roundGame in round.Games
                   where roundGame.BlackPlayer.PlayerId == player.PlayerId
                   || roundGame.WhitePlayer.PlayerId == player.PlayerId
                   select roundGame;
        }


        private static void RemovePlayer(List<Player> playersLeft, int playerId)
        {
            playersLeft.Remove(playersLeft.SingleOrDefault(p => p.PlayerId == playerId));
        }
    }
}
