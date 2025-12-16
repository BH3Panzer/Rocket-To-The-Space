using System.Security.RightsManagement;
using System.Windows.Controls;

namespace Rocket_To_The_Space
{
    internal abstract class RocketComponent
    {
        public string Name { get; private set; }
        public int Weight { get; private set; }
        public decimal Cost { get; set; }
        public Image Texture { get;}
        public int X { get; private set; }
        public int Y { get; private set; }
        public Canvas Canvas;
        protected RocketComponent(string name, int weight, decimal cost, Image texture, Canvas canvas)
        {
            Name = name;
            Weight = weight;
            Cost = cost;
            Texture = texture;
            Canvas = canvas;
        }

        public override bool Equals(object? obj)
        {
            RocketComponent? otherComponent = obj as RocketComponent;
            return otherComponent?.Name == Name && otherComponent?.Weight == Weight && otherComponent?.Cost == Cost;
        }

        public abstract RocketComponent Clone();
    }
}
