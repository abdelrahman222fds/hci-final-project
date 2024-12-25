using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;

namespace HCI_project
{
    internal class SavannaLevel : Level
    {
        private List<Character> characters = new List<Character>();
        private Character boy;
        private List<Character> foodItems = new List<Character>();
        private int score = 0;
        private readonly Random random = new Random();
        private readonly int maxScore = 2;
        private readonly int foodWidth = 50;
        private readonly int foodHeight = 50;
        private SoundPlayer lionSound = new SoundPlayer("sounds/lion.wav");
        private SoundPlayer monkeySound = new SoundPlayer("sounds/monkey.wav");
        private SoundPlayer giraffeSound = new SoundPlayer("sounds/giraffe.wav");
        public bool Won = false;
        public int Width { get; set; }
        public int Height { get; set; }

        public SavannaLevel(Bitmap coverImage, string name, int width, int height) : base(coverImage, name)
        {
            Width = width;
            Height = height;

            InitializeCharacters();
        }

        private void InitializeCharacters()
        {
            // Add lion
            characters.Add(new Character
            {
                Name = "Lion",
                Image = new Bitmap("images/savanna level/characters/lion.png"), // Replace with your image path
                X = Width - 200,
                Y = 0,
                Width = 200,
                Height = 200
            });

            // Add monkey
            characters.Add(new Character
            {
                Name = "Monkey",
                Image = new Bitmap("images/savanna level/characters/monkey.png"), // Replace with your image path
                X = Width - 200,
                Y = Height / 2 - 80,
                Width = 200,
                Height = 200
            });

            // Add giraffe
            characters.Add(new Character
            {
                Name = "Giraffe",
                Image = new Bitmap("images/savanna level/characters/giraffe.png"), // Replace with your image path
                X = Width - 200,
                Y = Height - 200,
                Width = 200,
                Height = 200
            });

            // Add boy
            boy = new Character
            {
                Name = "Boy",
                Image = new Bitmap("images/savanna level/characters/dora.png"), // Replace with your image path
                X = 50,
                Y = Height / 2 - 100,
                Width = 100,
                Height = 100
            };

            // Add food items
            AddFood("Meat", "images/savanna level/food/meat.png", 3);
            AddFood("Banana", "images/savanna level/food/banana.png", 3);
            AddFood("Grass", "images/savanna level/food/grass.png", 3);
        }
        private void AddFood(string name, string imagePath, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int x, y;
                do
                {
                    x = random.Next(100, Width - 100);
                    y = random.Next(50, Height - 50);
                } while (IsPositionInRestrictedColumns(x));

                foodItems.Add(new Character
                {
                    Name = name,
                    Image = new Bitmap(imagePath),
                    X = x,
                    Y = y,
                    Width = foodWidth,
                    Height = foodHeight
                });
            }
        }
        private bool IsPositionInRestrictedColumns(int x)
        {
            return x < 200 || x > Width - 200;
        }
        private void OnGameWon()
        {
            Console.WriteLine("You won the game!");
            Won = true;
        }
        public void Draw(Graphics g)
        {
            g.DrawImage(coverImage, 0, 0, Width, Height);
            foreach (var character in characters)
            {
                character.Draw(g);
            }

            foreach (var food in foodItems)
            {
                food.Draw(g);
            }

            boy.Draw(g);

            // Draw score
            Font font = new Font("Arial", 14, FontStyle.Bold);
            Brush brush = new SolidBrush(Color.Black);
            g.DrawString($"Score: {score}", font, brush, 10, 10);
        }
        public void Update()
        {
            if (boy.CarryingItem == null)
            {
                foreach (var food in foodItems)
                {
                    if (boy.IsColliding(food))
                    {
                        boy.CarryingItem = food;
                        foodItems.Remove(food);
                        break;
                    }
                }
            }

            foreach (var character in characters)
            {
                if (boy.CarryingItem != null && boy.IsColliding(character))
                {
                    if (character.Name == "Lion" && boy.CarryingItem.Name == "Meat" ||
                        character.Name == "Monkey" && boy.CarryingItem.Name == "Banana" ||
                        character.Name == "Giraffe" && boy.CarryingItem.Name == "Grass")
                    {
                        if (character.Name == "Lion" && boy.CarryingItem.Name == "Meat")
                        {                             
                            lionSound.Play();
                        }
                        else if (character.Name == "Monkey" && boy.CarryingItem.Name == "Banana")
                        {
                            monkeySound.Play();
                        }
                        else if (character.Name == "Giraffe" && boy.CarryingItem.Name == "Grass")
                        {
                               giraffeSound.Play();
                        }
                        boy.CarryingItem = null;
                        score++;
                    }
                }
            }

            if (score >= maxScore)
            {
                // Game is won
                OnGameWon();
            }
        }
        public void MoveBoy(int dx, int dy)
        {
            if (boy.X + dx > 0 && boy.X + boy.Width + dx < Width) {boy.X += dx;}
            if (boy.Y + dy > 0 && boy.Y + boy.Height + dy < Height) { boy.Y += dy; }
            if (boy.CarryingItem != null)
            {
                boy.CarryingItem.X = boy.X;
                boy.CarryingItem.Y = boy.Y;
            }
        }
    }
}
