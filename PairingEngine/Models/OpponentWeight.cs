using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PairingEngine.Models
{
    public class OpponentWeight
    {

        public OpponentWeight(Player player, double opponenWeight)
        {
            this.Player = player;
            this.Weight = opponenWeight;
        }

        public Player Player { get; set; }
        public double Weight { get; set; }

    }
}
