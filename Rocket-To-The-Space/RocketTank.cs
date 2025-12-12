using System.Windows.Controls;

namespace Rocket_To_The_Space
{
    internal class RocketTank : RocketComponent
    {
        public int FuelCapacity { get; }
        public int fuelAmount { get; set; }
        public RocketTank(string name, int weight, int cost, Image texture,Canvas canvas, int fuelCapacity) : base(name, weight, cost, texture, canvas)
        {
            this.FuelCapacity = fuelCapacity;
        }
    }
}
