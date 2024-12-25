using System;
using System.Collections.Generic;
using System.Drawing;

namespace HCI_project
{
    internal class Character
    {
        public Bitmap Image { get; set; }
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Character CarryingItem { get; set; } // The item the boy is carrying

        // Constructor to initialize the character
        public Character(Bitmap image, string name, int x, int y, int width, int height)
        {
            Image = image;
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        // Default constructor
        public Character() { }

        // Method to draw the character on the screen
        public void Draw(Graphics g)
        {
            if (Image != null)
            {
                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.DrawImage(Image, X, Y, Width, Height);

                // If the character is carrying an item, draw the carried item
                if (CarryingItem != null)
                {
                    g.DrawImage(CarryingItem.Image, X + Width / 2 - CarryingItem.Width / 2, Y + Height / 2 - CarryingItem.Height / 2, CarryingItem.Width, CarryingItem.Height);
                }
            }
            else
            {
                // If no image is set, draw a placeholder rectangle
                Brush placeholderBrush = new SolidBrush(Color.Gray);
                g.FillRectangle(placeholderBrush, X, Y, Width, Height);
                Font font = new Font("Arial", 8);
                Brush textBrush = new SolidBrush(Color.Black);
                g.DrawString(Name, font, textBrush, X + Width / 4, Y + Height / 3);
            }
        }

        // Method to check if a point (e.g., from a mouse click or gesture) hits the character
        public bool IsHit(int pointX, int pointY)
        {
            return pointX >= X && pointX <= X + Width && pointY >= Y && pointY <= Y + Height;
        }

        // Method to move the character by specified offsets
        public void Move(int offsetX, int offsetY)
        {
            X += offsetX;
            Y += offsetY;

            // If carrying an item, move the item along with the character
            if (CarryingItem != null)
            {
                CarryingItem.X = X + Width / 2 - CarryingItem.Width / 2;
                CarryingItem.Y = Y + Height / 2 - CarryingItem.Height / 2;
            }
        }

        // Method to reset the character's position
        public void ResetPosition(int startX, int startY)
        {
            X = startX;
            Y = startY;

            // Reset the carrying item position
            if (CarryingItem != null)
            {
                CarryingItem.X = X + Width / 2 - CarryingItem.Width / 2;
                CarryingItem.Y = Y + Height / 2 - CarryingItem.Height / 2;
            }
        }

        // Method to check for collision with another character
        public bool IsColliding(Character other)
        {
            Rectangle thisRect = new Rectangle(X, Y, Width, Height);
            Rectangle otherRect = new Rectangle(other.X, other.Y, other.Width, other.Height);

            return thisRect.IntersectsWith(otherRect);
        }

        // Method to check if the character is carrying an item and whether it can be dropped at the given location
        public bool CanDropItemAt(Character animal)
        {
            if (CarryingItem == null) return false;

            // Check if the animal can eat the carried item
            if (animal.Name == "Lion" && CarryingItem.Name == "Meat")
                return true;
            if (animal.Name == "Monkey" && CarryingItem.Name == "Banana")
                return true;
            if (animal.Name == "Giraffe" && CarryingItem.Name == "Grass")
                return true;

            return false;
        }

        // Method to drop the item when the animal eats it
        public void DropItem()
        {
            CarryingItem = null;
        }
    }
}
