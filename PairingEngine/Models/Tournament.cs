using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PairingEngine.Models
{
    public class Tournament
    {
        public int NumberOfRounds { get; set; }
        public IList<Round> RoundList { get; set; }
        public IList<RoundResult> Standings { get; set; }
        public IEnumerable<Player> Players { get; set; }
    }
}
