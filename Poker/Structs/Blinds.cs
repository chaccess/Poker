using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker.Structs
{
    public struct Blinds(int small, int big)
    {
        public int Small = small;
        public int Big = big;

        public override readonly string ToString()
        {
            return $"{Small} / {Big}";
        }
    }
}
