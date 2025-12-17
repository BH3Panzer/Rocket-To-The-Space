using System.Windows.Controls;

namespace Rocket_To_The_Space
{
    internal class RocketTank : RocketComponent
    {
        public int FuelCapacity { get; }
        public int FuelAmount { get; set; }
        public RocketTank(string name, int weight, decimal cost, Image texture,Canvas canvas, int fuelCapacity) : base(name, weight, cost, texture, canvas)
        {
            this.FuelCapacity = fuelCapacity;
            this.FuelAmount = fuelCapacity;
        }

        public override bool Equals(object? obj)
        {
            RocketTank? otherTank = obj as RocketTank;
            return base.Equals(obj) && otherTank.FuelCapacity == FuelCapacity;
        }

        public override RocketComponent Clone()
        {
            return new RocketTank(Name, Weight, Cost, Utils.GetCopy(Texture), Canvas, FuelCapacity);
        }
    }
}
