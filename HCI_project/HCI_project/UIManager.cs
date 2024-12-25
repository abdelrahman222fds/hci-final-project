using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCI_project
{
    internal class UIManager
    {
        public int width = 560, height = 600, levelsCount = 3;
        Bitmap bk = new Bitmap("images/bk.jpg");
        public void displayMainMenu(Graphics g, User user)
        {

            // Background color for the menu
            g.DrawImage(bk, 0, 0, width, height);
            
            List<Level> levels;
            
            if (user == null)
            {
                return;
            }
            else
            {
                levels = user.levels;
            }

            // Dimensions for level boxes
            int numberOfBoxesPerRow = 5;
            int boxWidth = width / numberOfBoxesPerRow; // Each box is 1/4th of the screen width
            int boxHeight = height / 5; // Each box is 1/5th of the screen height
            int padding = 20; // Space between boxes
            

            // Font for level names
            Font font = new Font("Arial", 10, FontStyle.Bold);
            Brush textBrush = new SolidBrush(Color.Black);
            Brush textBrushCompleted = new SolidBrush(Color.Green);

            g.DrawString($"Welcome {user.username}", font, textBrush, 0, 0);

            // Starting position for the first box
            int startX = padding;
            int startY = height / 4;

            // Iterate through levels and draw each box
            for (int i = 0; i < levels.Count; i++)
            {
                Level level = levels[i];

                // Calculate the current box position
                int currentX = startX + (i % numberOfBoxesPerRow) * (boxWidth + padding); // Wrap after 3 boxes
                int currentY = startY + (i / numberOfBoxesPerRow) * (boxHeight + padding); // Move to next row

                // Draw the box background
                Brush boxBrush = new SolidBrush(Color.LightGray);
                g.FillRectangle(boxBrush, currentX, currentY, boxWidth, boxHeight);

                // Draw the cover image inside the box
                if (level.coverImage != null)
                {
                    g.DrawImage(level.coverImage, currentX + padding / 2, currentY + padding / 2, boxWidth - padding, boxHeight - padding * 3);
                }

                // Draw the level name below the cover image
                string status = " Not Completed";

                if(level.completed)
                {
                    status = " Completed";
                }
                SizeF textSize = g.MeasureString( $"{i + 1}. " + level.name + status, font);
                float textX = currentX + (boxWidth - textSize.Width) / 2;
                float textY = currentY + boxHeight - padding * 1.5f;
                if(level.completed)
                {
                    g.DrawString($"{i + 1}. " + level.name + status, font, textBrushCompleted, textX, textY);
                }
                else
                {
                    g.DrawString($"{i + 1}. " + level.name + status, font, textBrush, textX, textY);
                }
            }
        }

        public void DisplayProgressBar(Graphics g, List<Level> levels)
        {
            int completedLevels = 0;
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].completed)
                {
                    completedLevels++;
                }
            }
            Brush emptyProgressBarBrush = new SolidBrush(Color.Gray);
            Brush filledProgressBarBrush = new SolidBrush(Color.LightGray);

            Rectangle emptyProgressBar = new Rectangle(width / 5, height / 8, (width / 5) * 3, height / 50);
            Rectangle filledProgressBar = new Rectangle(emptyProgressBar.X, emptyProgressBar.Y,
                                                        emptyProgressBar.Width / levelsCount * completedLevels,
                                                        emptyProgressBar.Height);

            g.FillRectangle(emptyProgressBarBrush, emptyProgressBar);
            g.FillRectangle(filledProgressBarBrush, filledProgressBar);

            string progressText = $"Completed Levels: {completedLevels} / {levelsCount}";
            Font font = new Font("Arial", 12, FontStyle.Bold);
            Brush textBrush = new SolidBrush(Color.Black);

            SizeF textSize = g.MeasureString(progressText, font);
            float textX = emptyProgressBar.X + (emptyProgressBar.Width - textSize.Width) / 2;
            float textY = emptyProgressBar.Y - textSize.Height - 5; // Place text slightly above the progress bar

            g.DrawString(progressText, font, textBrush, textX, textY);
        }

        public void DisplayLevelEndScreen(bool success)
        {

        }
    }
}
