using System.Windows.Controls;

namespace Rocket_To_The_Space
{
    internal class RocketBooster : RocketComponent
    {
        public int ThrustPower { get; }
        public int MaxEnergy { get; }
        public RocketBooster(string name, int weight, int cost, Image texture, int thrustPower, int maxEnergy) : base(name, weight, cost, texture)
        {
            ThrustPower = thrustPower;
            MaxEnergy = maxEnergy;
        }
    }
}
