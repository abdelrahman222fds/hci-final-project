using System;
using System.Net.Sockets;
using System.Text;

namespace HCI_project
{
    public class SocketClient
    {
        private string _host;
        private int _port;
        private TcpClient _client;
        private NetworkStream _stream;

        // Message format constants
        private const string MSG_START_END = "&";
        private const string MSG_TYPE_SEPARATOR = "$";
        private const string MSG_DATA_SEPARATOR = "#";

        public SocketClient(string host = "127.0.0.1", int port = 65432)
        {
            _host = host;
            _port = port;
        }

        public void Connect()
        {
            try
            {
                _client = new TcpClient(_host, _port);
                _stream = _client.GetStream();
                Console.WriteLine($"Connected to server at {_host}:{_port}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to server: {ex.Message}");
            }
        }

        public void Send(string message)
        {
            if (_stream != null && _stream.CanWrite)
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                _stream.Write(data, 0, data.Length);
            }
        }

        public string Receive()
        {
            if (_stream != null && _stream.CanRead)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                return Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }
            return null;
        }

        public void Disconnect()
        {
            if (_stream != null) _stream.Close();
            if (_client != null) _client.Close();
            Console.WriteLine("Disconnected from server.");
        }

        // Decode a received message
        public (string messageType, string[] data) Decode(string message)
        {
            if (message.StartsWith(MSG_START_END) && message.EndsWith(MSG_START_END))
            {
                // Strip the start and end markers
                string trimmedMessage = message.Trim(MSG_START_END.ToCharArray());

                // Split the message type and data
                string[] parts = trimmedMessage.Split(new char[] { Convert.ToChar(MSG_TYPE_SEPARATOR) }, 2);

                if (parts.Length == 2)
                {
                    string[] data1 = parts[1].Split(new char[] { Convert.ToChar(MSG_DATA_SEPARATOR) });
                    string messageType = data1[0];
                    string[] data = new string[data1.Length - 1];
                    for (int i = 1; i < data1.Length; i++)
                    {
                        data[i - 1] = data1[i];
                    }
                    return (messageType, data);
                }
            }

            Console.WriteLine("Invalid message format.");
            return (null, null);
        }
        // Prepares and sends a formatted message
        private void SendMessage(string messageType)
        {
            string message = $"{MSG_START_END}{MSG_TYPE_SEPARATOR}{messageType}{MSG_START_END}";
            Send(message);
        }
        private void SendMessage(string messageType, string[] data)
        {
            string message = $"{MSG_START_END}{MSG_TYPE_SEPARATOR}{messageType}";
            foreach (string item in data)
            {
                message += $"{MSG_DATA_SEPARATOR}{item}";
            }
            message += $"{MSG_START_END}";
            Send(message);
        }
        // Functions to send specific messages
        public void SendFaceIdLogin() => SendMessage("face_login");
        public void SendFaceIdRegister() => SendMessage("face_register");
        public void SendFaceIdRegisterUserName(string name) => SendMessage(name);
        public void SendDetectTUIO() => SendMessage("detect_TUIO");
        public void SendEndProgram() => SendMessage("end_program");
        public void SendStartHandTracking() => SendMessage("start_hand_tracking");
        public void SendEndHandTracking() => SendMessage("end_hand_tracking");
        public void SendStartEmotionDetection() => SendMessage("start_emotion_detection");
        public void SendEndEmotionDetection() => SendMessage("end_emotion_detection");
        public void SendStartThumbsDetection() => SendMessage("start_like_detection");
        public void SendEndThumbsDetection() => SendMessage("end_like_detection");
        public void SendUpdateUserLevels(string username, int level) => SendMessage("update_passed_levels", new string[] {username, level.ToString() });
        public void SendUpdateUserLevels() => SendMessage("update_passed_levels");

    }
}
