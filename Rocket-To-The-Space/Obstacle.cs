using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace Rocket_To_The_Space
{
    internal class Obstacle
    {
        public ObstacleType Type { get; }
        private Image obstacleImage;
        private int rotationAngle;
        private int x;
        private int y;
        public Obstacle(ObstacleType type, Image obstacleImage)
        {
            this.Type = type;
            this.obstacleImage = obstacleImage;
        }

        public void SetRotationAngle(int angle)
        {
            if (Type != ObstacleType.ASTEROID)
            {
                return;
            }
            rotationAngle = angle;
            RotateTransform rotateTransform = new RotateTransform(rotationAngle);
            obstacleImage.RenderTransform = rotateTransform;
        }
        
        public int GetRotationAngle()
        {
            return rotationAngle;
        }

        public void SetPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int GetX()
        {
            return x;
        }

        public int GetY()
        {
            return y;
        }
    }
}
