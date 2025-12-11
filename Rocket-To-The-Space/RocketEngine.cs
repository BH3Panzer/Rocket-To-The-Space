using System.Windows.Controls;

namespace Rocket_To_The_Space
{
    internal class RocketEngine : RocketComponent
    {
        public int ThrustPower { get ;}
        public RocketEngine(string name, int weight, int cost, Image texture, int thrustPower) : base(name, weight, cost, texture)
        {
            ThrustPower = thrustPower;
        }
    }
}
