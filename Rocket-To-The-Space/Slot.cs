using System.Windows.Controls;
using System.Windows;

namespace Rocket_To_The_Space
{
    internal class Slot
    {
        public Image Image { get;}
        public Image SelectedImage { get; }
        private RocketComponent? component;
        private int componentQuantity;
        private bool isSelected;
        public double X { get; set; }
        public double Y { get; set; }
        public Slot(Image image, Image selectedImage, double x, double y)
        {
            Image = image;
            SelectedImage = selectedImage;
            isSelected = false;
            X = x;
            Y = y;
        }

        public void Select()
        {
            isSelected = true;
            Image.Visibility = Visibility.Hidden;
            SelectedImage.Visibility = Visibility.Visible;
        }

        public void Deselect()
        {
            isSelected = false;
            Image.Visibility = Visibility.Visible;
            SelectedImage.Visibility = Visibility.Hidden;
        }

        public void Hide()
        {
            Image.Visibility = Visibility.Hidden;
            SelectedImage.Visibility = Visibility.Hidden;
            if (component != null)
            {
                component.Texture.Visibility = Visibility.Hidden;
            }
        }

        public void Show()
        {
            if (isSelected)
            {
                SelectedImage.Visibility = Visibility.Visible;
            }
            else
            {
                Image.Visibility = Visibility.Visible;
            }
        }

        public void SetComponent(RocketComponent component)
        {
            if (this.component != null)
            {
                component.Canvas.Children.Remove(component.Texture);
            }
            this.component = component;
        }

        public RocketComponent GetRocketComponent()
        {
            return component;
        }

        public void SetQuantity(int quantity)
        {
            componentQuantity = quantity;
        }

        public int GetQuantity()
        {
            return componentQuantity;
        }

        public void Draw()
        {
            Canvas.SetLeft(Image, X);
            Canvas.SetTop(Image, Y);
            Canvas.SetLeft(SelectedImage, X);
            Canvas.SetTop(SelectedImage, Y);
            if (component != null)
            {
                component.SetX((Image.ActualWidth - component.Texture.ActualWidth) / 2);
                component.SetY((Image.ActualHeight - component.Texture.ActualHeight) / 2);
                component.Draw();
            }
        }
    }
}
