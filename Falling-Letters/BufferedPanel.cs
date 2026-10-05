using System;
using System.Windows.Forms;

namespace Desktop_App
{
    public class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            this.DoubleBuffered = true;
        }
    }
}