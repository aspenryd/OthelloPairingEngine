using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PairingEngine.Models
{
    public class Game
    {
        public Player BlackPlayer { get; set; }
        public Player WhitePlayer { get; set; }
        public int BlackResult { get; set; }
        public int WhiteResult { get; set; }
        public int Round { get; set; }

        public Game(int blackRating, int whiteRating)
        {
            BlackPlayer = new Player(0,blackRating);
            WhitePlayer = new Player(0,whiteRating);
        }

        public Game(int roundnum, Player blackPlayer, Player whitePlayer)
        {
            Round = roundnum;
            BlackPlayer = blackPlayer;
            WhitePlayer = whitePlayer;
        }
    }
}
