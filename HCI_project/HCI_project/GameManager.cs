using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;

namespace HCI_project
{
    internal class GameManager
    {

        // form control components
        public bool ShowTextBox = false;

        // main components
        public SocketClient socketClient;
        public UIManager uiManager;

        public string serverMessage = "";

        // game manager components
        public int Width = 560, Height = 600;
        private int timeCount = 0;

        // Levels: Savanna, NorthPole
        SavannaLevel savannaLevel;
        NorthPoleLevel northPoleLevel;
        int speed = 20;
        int maxLevelsNum = 2;

        // User
        public User user = null;
            // Login
            bool loggedIn = false;
            bool loggingIn = false;

            bool waitingForLogin = false;
            int countLogin = 0, maxLoginAttempts = 3, waitLogin = 0, countTimerLogin = 0;
            
            //Register
            bool Registering = false;
            public string userName = "";
            public bool submitName = false;
                

        // thumps up or down
        bool startThumpsDetection = false, thumbDetectionStarted = false;
        bool StopThumpsDetection = false;
        bool resultLike = false, thumpDetcted = false;
        int countThumps = 0, maxThumps = 10;

        // detect level
        bool detectLevel = false;
        bool levelDetected = false;
        int selectedLevel = -1; 

        // handtracking
        bool startHandTracking = false, stopHandTracking = false;

        // emotions detection
        bool startEmotionsDetection = false, stopEmotionsDetection = false, detectEmotions = false;
        string emotion = "";

        // wait counter
        int Wct = 0;
        bool startWait = false;

        public GameManager() { }
        public GameManager(SocketClient socketClient, int Width, int Height) 
        { 
            this.Width = Width;
            this.Height = Height;
            this.socketClient = socketClient;
        }
        public void init()
        {
            uiManager = new UIManager();
            uiManager.width = Width;
            uiManager.height = Height;

            loggingIn = true;
            waitingForLogin = true;
        }
        private void displayMessage(Graphics g, string text, int paddingY = 0, int paddingX = 0)
        {
            Font font = new Font("Arial", 20, FontStyle.Bold);
            Brush textBrush = new SolidBrush(Color.Black);
            SizeF textSize = g.MeasureString(text, font);
            g.DrawString(text, font, textBrush, (Width - textSize.Width) / 2 + paddingX, (Height - textSize.Height) / 2 + paddingY);
        }


        public void draw(Graphics g)
        {
            uiManager.displayMainMenu(g, user);
            
            if (user != null)
            { 
                if (levelDetected)
                {
                    if (selectedLevel == 0)
                    {
                        savannaLevel.Draw(g);
                    }
                    else if (selectedLevel == 1)
                    {
                        northPoleLevel.Draw(g);
                    }
                    if (emotion != "")
                    {
                        Font font = new Font("Arial", 20, FontStyle.Bold);
                        Brush textBrush = new SolidBrush(Color.Black);
                        g.DrawString($"I think you are felling {emotion}", font, textBrush, 10, 30);
                    }
                }
                else
                {
                    uiManager.DisplayProgressBar(g, user.levels); 
                }
            }
            else 
            {
                if (loggingIn)
                {
                    displayMessage(g, "Logging in, please wait...");
                }
                else
                {
                    if (thumpDetcted)
                    {
                        if (resultLike)
                        {
                            displayMessage(g, "Thump detected, you liked. Now tell me your name.");
                            if (!Registering)
                            {
                                ShowTextBox = true;
                            }
                            Registering = true;
                        }
                        else
                        {
                            displayMessage(g, "Thump detected, you disliked");
                            loggingIn = true;
                            waitingForLogin = true;
                            socketClient.SendFaceIdLogin();
                        }
                    }
                    else
                    {
                        displayMessage(g, "Hmm... It seems like you are new.");
                        displayMessage(g, "Do a like (Thumps up) if you wanna tell me your name and regester", 50);
                        displayMessage(g, "Or do a dislike (Thumps down) if you wanna try to login", 100);
                    }
                    
                }
            }
        }
        private void initUser()
        {
            user = new User();
            user.levels = new List<Level>();
            user.levels.Add(new Level(new Bitmap("images//savanna level//savanna.jpg"), "Savanna"));
            user.levels.Add(new Level(new Bitmap("images//northpole level//north pole.jpg"), "North Pole"));
        }
        public void timer()
        {
            if (timeCount > 50)
            {
                ProcessServerMessage();
                HandleLogin();
                HandleThumbsDetection();
                HandleRegistration();
                HandleLevelDetection();
                HandleHandTracking();
                HandleEmotionDetection();
                HandleLevelProgression();
                HandleWaitTimer();
                serverMessage = "";
            }
            timeCount++;
        }

        private void ProcessServerMessage()
        {
            if (string.IsNullOrEmpty(serverMessage)) return;

            (string messageType, string[] data) = socketClient.Decode(serverMessage);
            Console.WriteLine($"Message Type: {messageType}");
            foreach (var item in data)
            {
                Console.WriteLine($"Data: {item}");
            }

            switch (messageType)
            {
                case "face_login":
                    HandleFaceLogin(data);
                    break;
                case "like_detection":
                    HandleLikeDetection(data);
                    break;
                case "face_register":
                    HandleFaceRegistration(data);
                    break;
                case "TUIO_object":
                    HandleTUIOObject(data);
                    break;
                case "hand_tracking":
                    HandleHandTrackingData(data);
                    break;
                case "emotion_detection":
                    HandleEmotionDetectionData(data);
                    break;
            }
        }

        private void HandleFaceLogin(string[] data)
        {
            if (countLogin >= maxLoginAttempts)
            {
                loggingIn = false;
                waitingForLogin = false;
                Console.WriteLine("Failed to login");
                startThumpsDetection = true;
                countLogin = 0;
            }
            else if (data[0] != "None" && data[1] != "None")
            {
                loggingIn = false;
                loggedIn = true;
                waitingForLogin = false;
                initUser();
                user.Login(data);
                Console.WriteLine("Logged in");
                detectLevel = true;
            }
            else
            {
                waitingForLogin = true;
                countLogin++;
            }
        }

        private void HandleLikeDetection(string[] data)
        {
            if (countThumps >= maxThumps)
            {
                resultLike = data[0] == "like";
                thumbDetectionStarted = false;
                StopThumpsDetection = true;
                thumpDetcted = true;
                countThumps = 0;
            }
            countThumps++;
        }

        private void HandleFaceRegistration(string[] data)
        {
            if (data[0] == "Registration Successful")
            {
                ResetLoginState();
            }
        }

        private void HandleTUIOObject(string[] data)
        {
            selectedLevel = int.Parse(data[0]);
            if (selectedLevel >= 0 && selectedLevel <= maxLevelsNum)
            {
                levelDetected = true;
                detectLevel = false;
                InitializeLevel();
            }
            else
            {
                detectLevel = true;
                selectedLevel = -1;
            }
        }

        private void HandleHandTrackingData(string[] data)
        {
            string[] coords = data[0].Split(new[] { '(', ',', ')', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            float x = float.Parse(coords[0]);
            float y = float.Parse(coords[1]);

            if (selectedLevel == 0)
            {
                AdjustCoordinates(ref x, ref y);
                savannaLevel.MoveBoy((int)(x * speed), (int)(y * speed));
            }
            if (selectedLevel == 1)
            {
                AdjustCoordinates(ref x, ref y);
                northPoleLevel.MoveBoy((int)(x * speed), (int)(y * speed));
            }
        }

        private void AdjustCoordinates(ref float x, ref float y)
        {
            x = x == 0.5f ? 0 : x == 0 ? -1 : x;
            y = y == 0.5f ? 0 : y == 0 ? -1 : y;
        }

        private void HandleEmotionDetectionData(string[] data)
        {
            emotion = data[0];
            Console.WriteLine("Emotion detected: " + emotion);
        }

        private void HandleLogin()
        {
            if (waitingForLogin)
            {
                countTimerLogin++;
                if (countTimerLogin >= waitLogin)
                {
                    countTimerLogin = 0;
                    waitingForLogin = false;
                    loggingIn = true;
                    socketClient.SendFaceIdLogin();
                    Console.WriteLine("Sent login request");
                }
            }
        }

        private void HandleThumbsDetection()
        {
            if (startThumpsDetection)
            {
                startThumpsDetection = false;
                thumbDetectionStarted = true;
                Console.WriteLine("Start thumbs detection");
                socketClient.SendStartThumbsDetection();
            }
            if (StopThumpsDetection)
            {
                StopThumpsDetection = false;
                Console.WriteLine("Stop thumbs detection");
                socketClient.SendEndThumbsDetection();
            }
        }

        private void HandleRegistration()
        {
            if (Registering && submitName)
            {
                ResetThumbsDetectionState();
                Registering = false;
                socketClient.SendFaceIdRegister();
                socketClient.SendFaceIdRegisterUserName(userName);
                Console.WriteLine("Sent username: " + userName);
                userName = "";
            }
        }

        private void HandleLevelDetection()
        {
            if (detectLevel)
            {
                detectLevel = false;
                Console.WriteLine("Detecting level");
                socketClient.SendDetectTUIO();
            }
        }

        private void HandleHandTracking()
        {
            if (startHandTracking)
            {
                startHandTracking = false;
                Console.WriteLine("Start hand tracking");
                socketClient.SendStartHandTracking();
            }
            if (stopHandTracking)
            {
                stopHandTracking = false;
                Console.WriteLine("Stop hand tracking");
                socketClient.SendEndHandTracking();
            }
        }

        private void HandleEmotionDetection()
        {
            if (detectEmotions && timeCount % 10 == 0)
            {
                startEmotionsDetection = true;
            }
            else if (detectEmotions && (timeCount - 1) % 10 == 0)
            {
                stopEmotionsDetection = true;
            }

            if (startEmotionsDetection)
            {
                startEmotionsDetection = false;
                Console.WriteLine("Start emotions detection");
                socketClient.SendStartEmotionDetection();
            }
            if (stopEmotionsDetection)
            {
                stopEmotionsDetection = false;
                Console.WriteLine("Stop emotions detection");
                socketClient.SendEndEmotionDetection();
            }
        }

        private void HandleLevelProgression()
        {
            if (levelDetected && selectedLevel == 0)
            {
                savannaLevel.Update();
                if (savannaLevel.Won)
                {
                    LevelCompleted();
                }
            }
            else if (levelDetected &&  selectedLevel == 1)
            {
                northPoleLevel.Update();
                if (northPoleLevel.Won)
                {
                    LevelCompleted();
                }
            }
        }

        private void HandleWaitTimer()
        {
            if (startWait)
            {
                Wct++;
                if (Wct == 10)
                {
                    Console.WriteLine("Sending update user levels");
                    socketClient.SendUpdateUserLevels();
                    socketClient.SendUpdateUserLevels(user.username, selectedLevel);
                    user.levels[selectedLevel].completed = true;
                    selectedLevel = -1;
                    startWait = false;
                    Wct = 0;
                }
            }
        }

        private void InitializeLevel()
        {
            if (selectedLevel == 0)
            {
                savannaLevel = new SavannaLevel(user.levels[selectedLevel].coverImage, "Savanna", Width, Height);
            }
            else if (selectedLevel == 1) 
            {
                northPoleLevel = new NorthPoleLevel(user.levels[selectedLevel].coverImage, "Savanna", Width, Height);
            }
            detectEmotions = true;
            startHandTracking = true;
        }

        private void LevelCompleted()
        {
            Console.WriteLine("Level won");
            levelDetected = false;
            startHandTracking = false;
            stopHandTracking = true;
            startWait = true;
            stopEmotionsDetection = true;
            detectEmotions = false;
        }

        private void ResetLoginState()
        {
            loggingIn = true;
            waitingForLogin = true;
            countLogin = 0;
            maxLoginAttempts = 3;
            waitLogin = 20;
            countTimerLogin = 0;
        }

        private void ResetThumbsDetectionState()
        {
            startThumpsDetection = false;
            thumbDetectionStarted = false;
            StopThumpsDetection = false;
            resultLike = false;
            thumpDetcted = false;
        }
    }
}
