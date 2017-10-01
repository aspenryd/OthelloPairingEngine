using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PairingEngine.Models
{
    public class RoundResult
    {
        public int RoundNumber { get; set; }
        
        public IList<PlayerResult> PlayerResults { get; set; }

        public RoundResult()
        {
            PlayerResults = new List<PlayerResult>();
        }
    }
}
