using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PairingEngine.Models
{
    public class Player
    {

        public Player(int playerID, int rating)
        {
            PlayerId = playerID;
            this.Rating = rating;
        }

        public int PlayerId { get; set; }
        public string Name { get; set; }
        public int Rating { get; set; }
        public string Country { get; set; }
    }
}
