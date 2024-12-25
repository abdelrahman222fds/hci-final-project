using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace HCI_project
{
    public partial class Form1 : Form
    {
        Bitmap bitmap;
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        int TimeCount = 1;
        //int drag = 0;

        private SocketClient socketClient; // Instance of SocketClient
        private string receivedMessage = ""; // To store the received message
        private Thread receiveThread;
        private TextBox textBox;
        private Button submit;

        GameManager GM;

        public Form1()
        {
            InitializeComponent();
            Paint += new PaintEventHandler(OnPaint);
            Load += new EventHandler(OnLoad);
            timer.Tick += new EventHandler(time);
            timer.Start();
            KeyDown += new KeyEventHandler(OnKeyDown);
            KeyUp += new KeyEventHandler(OnKeyUp);
            MouseDown += new MouseEventHandler(OnMouseDown);
            MouseMove += new MouseEventHandler(OnMouseMove);
            MouseUp += new MouseEventHandler(OnMouseUp);

            textBox = new TextBox
            {
                Multiline = true,
                Visible = false,
                Width = 300,
                Height = 100,
                Font = new Font("Arial", 16),
                TextAlign = HorizontalAlignment.Center
            };
            Controls.Add(textBox);

            submit = new Button
            {
                Text = "Submit",
                Visible = false,
                Width = 100,
                Height = 50,
                Font = new Font("Arial", 16),
                TextAlign = ContentAlignment.MiddleCenter
            };
            submit.Click += OnSubmit;
            Controls.Add(submit);
        }
        void OnSubmit(object sender, EventArgs e)
        {
            GM.userName = textBox.Text;
            textBox.Text = "";
            GM.ShowTextBox = false;
            GM.submitName = true;
        }
        void OnKeyDown(object sender, KeyEventArgs e)
        {
            
        }
        void OnKeyUp(object sender, KeyEventArgs e)
        {

        }
        void OnMouseDown(object sender, MouseEventArgs e)
        {
            //drag = 1;
        }
        void OnMouseMove(object sender, MouseEventArgs e)
        {
            /*if(drag == 1)
            {
                //drag code goes here
            }*/
        }
        void OnMouseUp(object sender, MouseEventArgs e)
        {
            //drag = 0;
        }
        private void OnLoad(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            bitmap = new Bitmap(ClientSize.Width, ClientSize.Height);
            creat_my_actors();

            // Center the TextBox in the middle of the screen
            textBox.Location = new Point((ClientSize.Width - textBox.Width) / 2, (ClientSize.Height - textBox.Height) / 2 + 150);

            // Center the Submit button below the TextBox
            submit.Location = new Point((ClientSize.Width - submit.Width) / 2, textBox.Bottom + 20);
        }
        private void ReceiveData()
        {
            try
            {
                while (true)
                {
                    string message = socketClient.Receive();
                    if (!string.IsNullOrEmpty(message))
                    {
                        // Update the received message safely in the UI thread
                        this.Invoke((MethodInvoker)delegate
                        {
                            receivedMessage = message;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving data: {ex.Message}");
            }
        }
        void DrawDubb(Graphics g)
        {
            Graphics g2 = Graphics.FromImage(bitmap);
            draw_every_thing(g2);
            g.DrawImage(bitmap, 0, 0);
        }
        void draw_every_thing(Graphics g2)
        {
            g2.Clear(Color.White);
            GM.draw(g2);

/*            savannaLevel4.Draw(g2);
*/            /*if (!string.IsNullOrEmpty(receivedMessage))
            {
                string message_type;
                string[] message_data;
                (message_type, message_data) = socketClient.Decode(receivedMessage);
                if (message_data.Length > 1)
                g2.DrawString($"Received: {message_data[1]}",
                              new Font("Arial", 16),
                              Brushes.Black,
                              new PointF(10, 10));
            }*/

        }
        void creat_my_actors()
        {
            // Initialize and connect the socket client
            socketClient = new SocketClient();
            socketClient.Connect();

            // Send an initial message
            socketClient.Send("Client connected!");

            // Start the receiving thread
            receiveThread = new Thread(ReceiveData);
            receiveThread.IsBackground = true;
            receiveThread.Start();


            GM = new GameManager(socketClient, ClientSize.Width, ClientSize.Height);
            GM.init();
        }
        void OnPaint(object sender, PaintEventArgs e)
        {
            GM.Width = ClientSize.Width; GM.Height = ClientSize.Height;
            GM.uiManager.width = ClientSize.Width; GM.uiManager.height = ClientSize.Height;
            DrawDubb(e.Graphics);
        }
        void time(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(receivedMessage))
            {
                Console.WriteLine("client: "+receivedMessage);
                GM.serverMessage = receivedMessage;
                receivedMessage = "";
            }
            GM.timer();

            textBox.Visible = GM.ShowTextBox;
            submit.Visible = GM.ShowTextBox;
            TimeCount++;
            DrawDubb(CreateGraphics());
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // Disconnect the socket client on form close
            socketClient.SendEndEmotionDetection();
            socketClient.SendEndHandTracking();
            socketClient.SendEndThumbsDetection();
            socketClient.SendEndProgram();
            socketClient.Disconnect();
            base.OnFormClosed(e);
        }
    }
}
