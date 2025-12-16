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
        public double X { get; private set; }
        public double Y { get; private set; }
        public Rocket()
        {
            Components = new List<RocketComponent>();
            Speed = 8;
            RocketBox = new Rectangle();
            RocketBox.Width = 64;
            RocketBox.Height = 128;
            RocketBox.Fill = System.Windows.Media.Brushes.Red;
        }

        public void Init(UCGame uc)
        {
            uc.gameCanvas.Children.Add(RocketBox);
            RocketBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            RocketBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            Canvas.SetZIndex(this.RocketBox, 0b10); // easter egg :)
            this.Y = uc.gameCanvas.Height - RocketBox.Height - 90;
            this.X = (uc.gameCanvas.Width - RocketBox.Width) / 2;
        }

        public void Update()
        {
            UpdateStats();
            this.Y -= this.Speed;
        }

        public void Draw(Camera cam)
        {
            Canvas.SetTop(this.RocketBox, this.Y - cam.Y);
            Canvas.SetLeft(this.RocketBox, this.X - cam.X);
        }

        public void UpdateStats()
        {
            Speed = 0;
            foreach (var component in Components)
            {
                if (component is RocketEngine engine)
                {
                    Speed += engine.ThrustPower;
                }
                else if (component is RocketBooster booster)
                {
                    Speed += booster.ThrustPower;
                }
            }

            foreach (var component in Components)
            {
                Speed -= component.Weight;
            }
        }
    }
}
