using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        public double X { get; set; }
        public double Y { get; set; }
        public Rocket()
        {
            Components = new List<RocketComponent>();
            Speed = 8;
            RocketBox = new Rectangle();
            RocketBox.Width = 64;
            RocketBox.Height = 128;
        }

        public void Init(UCGame uc)
        {
            uc.gameCanvas.Children.Add(RocketBox);
            RocketBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            RocketBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            Canvas.SetZIndex(this.RocketBox, 0b10); // easter egg :)
            this.Y = uc.gameCanvas.Height - RocketBox.Height - 100;
            this.X = (uc.gameCanvas.Width - RocketBox.Width) / 2;
        }

        public void ResetPosition(UCGame uc)
        {
            this.Y = uc.gameCanvas.Height - RocketBox.Height - 100;
            this.X = (uc.gameCanvas.Width - RocketBox.Width) / 2;
            UpdateComponents();
        }

        public bool Update(ref decimal money)
        {
            bool result = UpdateStats();
            this.Y -= this.Speed;
            money += (decimal)(this.Speed * 0.0005);
            Math.Round(money, 2);
            UpdateComponents();
            return result;
        }

        public void DrainFuel(object? sender, EventArgs e)
        {
            foreach (var component in Components)
            {
                if (component is RocketTank tank)
                {
                    if (tank.FuelAmount > 0)
                    {
                        tank.FuelAmount--;
                        return;
                    }
                }
            }
        }

        public int GetTotalFuel()
        {
            int totalFuel = 0;
            foreach (var component in Components)
            {
                if (component is RocketTank tank)
                {
                    totalFuel += tank.FuelAmount;
                }
            }
            return totalFuel;
        }

        public void Draw(Camera cam)
        {
            Canvas.SetTop(this.RocketBox, this.Y - cam.Y);
            Canvas.SetLeft(this.RocketBox, this.X - cam.X);
            foreach (var component in Components)
            {
                component.Draw(cam);
            }
        }

        public bool AddComponent(RocketComponent component)
        {
            component.IsAttachedToRocket = true;
            bool ok = false;
            if (component is RocketCapsule)
            {
                uint count = 0;
                foreach (var comp in Components)
                {
                    if (comp is RocketCapsule)
                        count++;
                }
                if (count < 1)
                {
                    Components.Add(component);
                    ok = true;
                }

            }
            if (component is RocketEngine)
            {
                uint count = 0;
                foreach (var comp in Components)
                {
                    if (comp is RocketEngine)
                        count++;
                }
                if (count < 1)
                {
                    Components.Add(component);
                    ok = true;
                }
            }
            if (component is RocketTank)
            {
                uint count = 0;
                foreach (var comp in Components)
                {
                    if (comp is RocketTank)
                        count++;
                }
                if (count < 4)
                {
                    Components.Add(component);
                    ok = true;
                }
            }

            UpdateStats();
            return ok;
        }

        public void AddComponentToCanvas(RocketComponent component, Canvas canvas)
        {
            canvas.Children.Add(component.Texture);
            UpdateComponents();
        }

        public void RemoveComponent(Canvas canvas, RocketComponent component)
        {
            Components.Remove(component);
            canvas.Children.Remove(component.Texture);
            UpdateStats();
        }

        public List<RocketComponent> GetComponents()
        {
            return Components;
        }

        public void UpdateComponents()
        {
            uint tankCount = 0;
            foreach (var component in Components)
            {
                if (!component.IsAttachedToRocket)
                {
                    continue;
                }
                if (component is RocketCapsule)
                {
                    component.SetX(this.X + (this.RocketBox.Width - component.Texture.Width) / 2);
                    component.SetY(this.Y);
                }
                else if (component is RocketTank)
                {
                    component.SetX(this.X + (this.RocketBox.Width - component.Texture.Width) / 2);
                    component.SetY(this.Y + 32 + (32 * tankCount));
                    tankCount++;
                }
            }
            foreach (var component in Components)
            {
                if (!component.IsAttachedToRocket)
                {
                    continue;
                }
                if (component is RocketEngine)
                {
                    component.SetX(this.X + (this.RocketBox.Width - component.Texture.Width) / 2);
                    component.SetY(this.Y + 26 + (32 * tankCount));
                }
                Canvas.SetZIndex(component.Texture, 2);
            }
        }

        public bool UpdateStats()
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

            if (GetTotalFuel() <= 0)
            {
                Speed = 0;
                return true;
            }
            return false;
        }

        public bool IsReadyForLaunch(bool launchStatus)
        {
            bool hasCapsule = false;
            bool hasEngine = false;
            foreach (var component in Components)
            {
                if (component is RocketCapsule)
                {
                    hasCapsule = true;
                }
                else if (component is RocketEngine)
                {
                    hasEngine = true;
                }

            }
            return hasCapsule && hasEngine && GetTotalFuel() > 0 && !launchStatus;
        }
    }
}
