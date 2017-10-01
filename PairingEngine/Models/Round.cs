using System.Collections.Generic;

namespace PairingEngine.Models
{
    public class Round
    {
        public int RoundNumber { get; set; }
        public IList<Game> Games { get; set; }

        public Round()
        {
            Games = new List<Game>();
        }
    }
}