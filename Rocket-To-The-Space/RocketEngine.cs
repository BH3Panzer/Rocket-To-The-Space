using System.Windows.Controls;

namespace Rocket_To_The_Space
{
    internal class RocketEngine : RocketComponent
    {
        public int ThrustPower { get ;}
        public RocketEngine(string name, int weight, decimal cost, Image texture, Canvas canvas, int thrustPower) : base(name, weight, cost, texture, canvas)
        {
            ThrustPower = thrustPower;
        }

        public override bool Equals(object? obj)
        {
            RocketEngine? otherEngine = obj as RocketEngine;
            return base.Equals(obj) && otherEngine.ThrustPower == ThrustPower;
        }

        public override RocketEngine Clone()
        {
            return new RocketEngine(Name, Weight, Cost, Utils.GetCopy(Texture), Canvas, ThrustPower);
        }
    }
}
