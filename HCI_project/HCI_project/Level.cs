using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCI_project
{
    internal class Level
    {
        public Bitmap coverImage;
        public string name;
        public bool completed = false;

        public Level(Bitmap coverImage, string name)
        { 
            this.coverImage = coverImage; 
            this.name = name;
        }
    }
}
