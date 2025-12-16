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

        public override bool Equals(object? obj)
        {
            RocketCapsule? otherCapsule = obj as RocketCapsule;
            return base.Equals(obj) && otherCapsule?.ControlMultiplier == ControlMultiplier;
        }

        public override RocketComponent Clone()
        {
            return new RocketCapsule(Name, Weight, Cost, Utils.GetCopy(Texture), Canvas, ControlMultiplier);
        }
    }
}
