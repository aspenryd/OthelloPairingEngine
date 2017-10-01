using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PairingEngine;
using PairingEngine.Models;

namespace PairingEngineTests
{
    [TestClass]
    public class PappParingTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            var tournament = PairingSimulation.GenerateTournament(7, 20);
            PappPairing.PairNextRound(tournament);
        }
    }
}
