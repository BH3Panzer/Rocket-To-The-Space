using System.Windows.Controls;

namespace Rocket_To_The_Space
{
    internal class RocketCapsule : RocketComponent
    {
        public double ControlMultiplier { get; } 
        public RocketCapsule(string name, int weight, decimal cost, Image texture, Canvas canvas, double controlMultiplier) : base(name, weight, cost, texture, canvas)
        {
            this.ControlMultiplier = controlMultiplier;

        }
    }
}
