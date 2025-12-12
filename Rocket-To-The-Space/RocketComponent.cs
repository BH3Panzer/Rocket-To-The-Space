using System.Windows.Controls;

namespace Rocket_To_The_Space
{
    internal abstract class RocketComponent
    {
        public string Name { get; private set; }
        public int Weight { get; private set; }
        public int Cost { get; set; }
        public Image Texture { get;}
        private int x;
        private int y;
        private Canvas canvas;
        protected RocketComponent(string name, int weight, int cost, Image texture, Canvas canvas)
        {
            Name = name;
            Weight = weight;
            Cost = cost;
            Texture = texture;
            this.canvas = canvas;
        }

        public void SetX(int newX)
        {
            x = newX;

        }
    }
}
