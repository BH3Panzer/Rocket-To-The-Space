using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Rocket_To_The_Space
{
    internal class Rocket
    {
        public List<RocketComponent> Components { get; set; }
        public double Speed { get; private set; }

        public Rectangle RocketBox { get; private set; }
        public Rocket()
        {
            Components = new List<RocketComponent>();
            Speed = 8;
            RocketBox = new Rectangle();
            RocketBox.Width = 64;
            RocketBox.Height = 256;
            RocketBox.Fill = System.Windows.Media.Brushes.Red;
        }

        public void Init(UCGame uc)
        {
            uc.gameCanvas.Children.Add(RocketBox);
            RocketBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            RocketBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            Canvas.SetTop(this.RocketBox, uc.gameCanvas.Height - RocketBox.Height - 100);
            Canvas.SetLeft(this.RocketBox, (uc.gameCanvas.Width - RocketBox.Width) / 2);
        }

        public void Update(UCGame uc)
        {
            Canvas.SetTop(this.RocketBox, Canvas.GetTop(this.RocketBox) - Speed);
        }
    }
}
