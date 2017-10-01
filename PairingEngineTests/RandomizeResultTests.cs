using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PairingEngine;
using PairingEngine.Models;
using TestSimpleRNG;

namespace PairingEngineTests
{
    [TestClass]
    public class RandomizeResultTests
    {
        [TestInitialize]
        public void Setup()
        {
            SimpleRNG.SetSeedFromSystemTime();
        }

        [TestMethod]
        public void TestStandardDeviationForGame()
        {
            const int numSamples = 100000;
            var rs = new RunningStat();
            rs.Clear();
            var mean = 32;
            var stdev = 7;
            for (int i = 0; i < numSamples; ++i)
                rs.Push(SimpleRNG.GetNormal(mean, stdev));
            PrintStatisticResults("normal", mean, stdev * stdev, rs.Mean(), rs.Variance(), rs.resultlist.Min(), rs.resultlist.Max());
            PrintGameResultStats(rs);
        }


        [TestMethod]
        public void TestStandardDeviationForGame_SameRankedPlayers()
        {
            var game = new Game(2000, 2000);
            RunSimulationAndPrintResults(game);
        }

        [TestMethod]
        public void TestStandardDeviationForGame_BetterRankedBlackPlayer_by050()
        {
            var game = new Game(2000, 1950);
            RunSimulationAndPrintResults(game);
        }

        [TestMethod]
        public void TestStandardDeviationForGame_BetterRankedWhitePlayer_by050()
        {
            var game = new Game(2000, 2050);
            RunSimulationAndPrintResults(game);
        }


        [TestMethod]
        public void TestStandardDeviationForGame_BetterRankedBlackPlayer_by100()
        {
            var game = new Game(2000, 1900);
            RunSimulationAndPrintResults(game);
        }

        [TestMethod]
        public void TestStandardDeviationForGame_BetterRankedWhitePlayer_by100()
        {
            var game = new Game(2000, 2100);
            RunSimulationAndPrintResults(game);
        }

        [TestMethod]
        public void TestStandardDeviationForGame_BetterRankedBlackPlayer_by200()
        {
            var game = new Game(2000, 1800);
            RunSimulationAndPrintResults(game);
        }

        [TestMethod]
        public void TestStandardDeviationForGame_BetterRankedWhitePlayer_by200()
        {
            var game = new Game(2000, 2200);
            RunSimulationAndPrintResults(game);
        }


        private static void RunSimulationAndPrintResults(Game game)
        {
            const int numSamples = 100000;
            var rs = new RunningStat();
            rs.Clear();

            for (int i = 0; i < numSamples; ++i)
            {
                PairingSimulation.RandomizeResult(ref game);
                rs.Push(game.BlackResult);
            }
            PrintStatisticResults("normal", 32, 7*7, rs.Mean(), rs.Variance(), rs.resultlist.Min(), rs.resultlist.Max());
            PrintGameResultStats(rs);
        }


        private static string CalcResult(RunningStat rs, Func<double, bool> del)
        {
            var calcResult = rs.resultlist.Count(del);
            var percentage = (double)calcResult / (double)rs.resultlist.Count();
            return $"{calcResult} ({percentage.ToString("P")})";
        }
        private static void PrintGameResultStats(RunningStat rs)
        {
            Console.WriteLine(
                $"Black wins {CalcResult(rs, r => r > 32)}, Draws {CalcResult(rs, r => r == 32)}, White wins {CalcResult(rs, r => r < 32)}");
        }

        static void PrintStatisticResults (string name, double expectedMean, double expectedVariance, double computedMean, double computedVariance, double min, double max)
        {
            Console.WriteLine("Testing {0}", name);
            Console.WriteLine("Expected mean:     {0}, computed mean:     {1}", expectedMean, computedMean);
            Console.WriteLine("Expected variance: {0}, computed variance: {1}", expectedVariance, computedVariance);
            Console.WriteLine("Min: {0}, Max: {1}", min, max);
            Console.WriteLine("");
        }

    }
}
