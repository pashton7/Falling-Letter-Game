using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using GameEngine;
namespace Desktop_App
{

    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);


        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);

        [DllImport("user32.dll")]
        private static extern int PeekMessage(out Program.NativeMessage message, IntPtr window, uint filterMin, uint filterMax, uint remove);

        [STAThread]
        static void Main()
        {
            Program.mainForm = new Form1();
            Program.mainForm.BackColor = Color.Coral; // Set the background color to Coral

            Program.mainForm.Size = Screen.PrimaryScreen.WorkingArea.Size; // Set to full screen size
            Program.mainForm.StartPosition = FormStartPosition.Manual;
            Program.mainForm.Location = new Point(0, 0); // Set location fo window to the center
            Program.mainForm.TopMost = true; // Show window on top of all other windows
            Program.mainForm.AllowTransparency = true;
            Program.mainForm.TransparencyKey = Color.Coral;
            Program.OriginalWindowStyle = (IntPtr)((long)((ulong)Program.GetWindowLong(Program.mainForm.Handle, -20)));
            Program.PassthruWindowStyle = (IntPtr)((long)((ulong)(Program.GetWindowLong(Program.mainForm.Handle, -20) | 524288U | 32U)));
            Program.SetWindowPassThru(true);
            Program.canvas = new BufferedPanel();
            Program.canvas.Dock = DockStyle.Fill;
            Program.canvas.BackColor = Color.Transparent; // Set the background color to Transparent
            Program.canvas.BringToFront();
            Program.canvas.Paint += Program.Render;

            Program.mainForm.Controls.Add(Program.canvas);
            MainGame.Init(); // Initialize the game engine and load assets

            gameTimer = new System.Windows.Forms.Timer(); // Create a timer to control the game loop
            gameTimer.Interval = 16; // Attempt to run at 60FPS

            gameTimer.Tick += (s, e) =>
            {
                Time.TickTime();
                MainGame.Update();
                Program.canvas.Invalidate();
            };

            gameTimer.Start();

            Application.EnableVisualStyles();

            Application.Run(mainForm);
        }

        public static Form mainForm;
        public static IntPtr OriginalWindowStyle;
        public static IntPtr PassthruWindowStyle;
        private static BufferedPanel canvas;

        public static System.Windows.Forms.Timer gameTimer;


        public struct NativeMessage
        {
            public IntPtr handle;
            public uint msg;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public Point location;
        }

        public static void SetWindowPassThru(bool pass)
        {
            if (pass)
            {
                Program.SetWindowLong(Program.mainForm.Handle, -20, Program.PassthruWindowStyle);
                return;
            }
            Program.SetWindowLong(Program.mainForm.Handle, -20, Program.OriginalWindowStyle);
        }

        private static void Render(object sender, PaintEventArgs e)
        {
            
            MainGame.Render(e.Graphics); // Render the game objects onto the canvas
        }

        public static bool IsApplicationIdle()
        {
            Program.NativeMessage nativeMessage;
            return Program.PeekMessage(out nativeMessage, IntPtr.Zero, 0U, 0U, 0U) == 0;
        }



    }
}