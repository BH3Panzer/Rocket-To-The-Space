using System.Windows.Controls;
using System.Windows.Media.Animation;

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

        public override bool Equals(object? obj)
        {
            RocketBooster? otherBooster = obj as RocketBooster; 
            return base.Equals(obj) && otherBooster?.ThrustPower == ThrustPower && otherBooster?.MaxEnergy == MaxEnergy;
        }

        public override RocketBooster Clone()
        {
            return new RocketBooster(Name, Weight, Cost, Utils.GetCopy(Texture), Canvas, ThrustPower, MaxEnergy);
        }
    }
}
