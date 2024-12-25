using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;

namespace HCI_project
{
    internal class NorthPoleLevel : Level
    {
        private List<Character> characters = new List<Character>();
        private Character boy;
        private List<Character> foodItems = new List<Character>();
        private int score = 0;
        private readonly Random random = new Random();
        private readonly int maxScore = 9;
        private readonly int foodWidth = 50;
        private readonly int foodHeight = 50;
        private SoundPlayer polarBearSound = new SoundPlayer("sounds/polarbear.wav");
        private SoundPlayer sealSound = new SoundPlayer("sounds/seal.wav");
        private SoundPlayer penguinSound = new SoundPlayer("sounds/penguin.wav");
        public bool Won = false;
        public int Width { get; set; }
        public int Height { get; set; }

        public NorthPoleLevel(Bitmap coverImage, string name, int width, int height) : base(coverImage, name)
        {
            Width = width;
            Height = height;

            InitializeCharacters();
        }

        private void InitializeCharacters()
        {
            // Add Polar Bear
            characters.Add(new Character
            {
                Name = "Polar Bear",
                // 
                Image = new Bitmap("images/northpole level/characters/polarbear.jpg"), // Replace with your image path
                X = Width - 200,
                Y = 0,
                Width = 200,
                Height = 200
            });

            // Add Seal
            characters.Add(new Character
            {
                Name = "Seal",
                Image = new Bitmap("images/northpole level/characters/seal.jpg"), // Replace with your image path
                X = Width - 200,
                Y = Height / 2 - 80,
                Width = 200,
                Height = 200
            });

            // Add Penguin
            characters.Add(new Character
            {
                Name = "Penguin",
                Image = new Bitmap("images/northpole level/characters/penguin.jpg"), // Replace with your image path
                X = Width - 200,
                Y = Height - 200,
                Width = 200,
                Height = 200
            });

            // Add boy
            boy = new Character
            {
                Name = "Boy",
                Image = new Bitmap("images/northpole level/characters/dora.png"), // Replace with your image path
                X = 50,
                Y = Height / 2 - 100,
                Width = 100,
                Height = 100
            };

            // Add food items
            AddFood("Fish", "images/northpole level/food/fish.png", 3);
            AddFood("Krill", "images/northpole level/food/krill.png", 3);
            AddFood("Ice Block", "images/northpole level/food/icecube.png", 3);
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
                    if (character.Name == "Polar Bear" && boy.CarryingItem.Name == "Fish" ||
                        character.Name == "Seal" && boy.CarryingItem.Name == "Krill" ||
                        character.Name == "Penguin" && boy.CarryingItem.Name == "Ice Block")
                    {
                        if (character.Name == "Polar Bear" && boy.CarryingItem.Name == "Fish")
                        {
                            polarBearSound.Play();
                        }
                        else if (character.Name == "Seal" && boy.CarryingItem.Name == "Krill")
                        {
                            sealSound.Play();
                        }
                        else if (character.Name == "Penguin" && boy.CarryingItem.Name == "Ice Block")
                        {
                            penguinSound.Play();
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
            if (boy.X + dx > 0 && boy.X + boy.Width + dx < Width) { boy.X += dx; }
            if (boy.Y + dy > 0 && boy.Y + boy.Height + dy < Height) { boy.Y += dy; }
            if (boy.CarryingItem != null)
            {
                boy.CarryingItem.X = boy.X;
                boy.CarryingItem.Y = boy.Y;
            }
        }
    }
}
