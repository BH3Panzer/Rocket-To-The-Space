using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Rocket_To_The_Space
{
    internal class RocketCapsule : RocketComponent
    {
        public double AirFrictionMultiplier { get; }
        public int ControlMultiplier { get; } 
        public RocketCapsule(string name, int weight, int cost, Image texture, double airFrictionMultiplicator, int controlMultiplier) : base(name, weight, cost, texture)
        {
            this.AirFrictionMultiplier = airFrictionMultiplicator;
            this.ControlMultiplier = controlMultiplier;

        }
    }
}
