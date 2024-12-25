using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace HCI_project
{
    internal class User
    {
        public string username;
        public List<Level> levels;
        
        public void Login(string[] data)
        {
            string userdata = data[1];
            List<int> passedLevels = new List<int>();

            // Parse the JSON string
            string[] data2 = userdata.Split(new char[] { '{', '}', ',', ':', '[', ']', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            username = data2[1].Split('/')[1].Split('.')[0];

            for (int i = 3; i < data2.Length; i++)
            {
                passedLevels.Add(int.Parse(data2[i]));
            }
            for (int i = 0; i < levels.Count; i++)
            {
                levels[i].completed = passedLevels.Contains(i);
            }
        }
    }
}
