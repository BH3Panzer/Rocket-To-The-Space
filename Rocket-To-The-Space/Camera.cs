using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rocket_To_The_Space
{
    internal class Camera
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Camera(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
