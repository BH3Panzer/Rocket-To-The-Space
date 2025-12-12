using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_To_The_Space
{
    internal class Rocket
    {
        public List<RocketComponent> Components { get; set; }
        public double Speed { get; private set; }
        public Rocket()
        {
            Components = new List<RocketComponent>();
            Speed = 64;
        }
    }
}
