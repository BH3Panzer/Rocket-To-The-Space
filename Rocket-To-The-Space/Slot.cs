using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Rocket_To_The_Space
{
    internal class Slot
    {
        public Image Image { get; private set; }
        public Image SelectedImage { get; private set; }
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
    }
}
