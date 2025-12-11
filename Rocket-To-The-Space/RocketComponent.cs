using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Rocket_To_The_Space
{
    internal abstract class RocketComponent
    {
        public string Name { get; private set; }
        public int Weight { get; private set; }
        public int Cost { get; set; }
        public Image Texture { get;}
        protected RocketComponent(string name, int weight, int cost, Image texture)
        {
            Name = name;
            Weight = weight;
            Cost = cost;
            Texture = texture;
        }
    }
}
