using System.Windows.Controls;
using System.Windows;

namespace Rocket_To_The_Space
{
    internal class Slot
    {
        public Image Image { get;}
        public Image SelectedImage { get; }
        private RocketComponent? component;
        private bool isSelected;
        public Slot(Image image, Image selectedImage)
        {
            Image = image;
            SelectedImage = selectedImage;
            isSelected = false;
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
                this.component.Canvas.Children.Remove(this.component.Texture);
            }
            this.component = component;
        }

        public RocketComponent GetRocketComponent()
        {
            return component;
        }

        public bool IsEmpty()
        {
            return component == null;
        }
    }
}
