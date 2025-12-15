using System.Windows.Controls;

namespace Rocket_To_The_Space
{
    internal class RocketBooster : RocketComponent
    {
        public int ThrustPower { get; }
        public int MaxEnergy { get; }
        public RocketBooster(string name, int weight, decimal cost, Image texture, Canvas canvas, int thrustPower, int maxEnergy) : base(name, weight, cost, texture, canvas)
        {
            ThrustPower = thrustPower;
            MaxEnergy = maxEnergy;
        }
    }
}
