using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using GameEngine;

namespace Desktop_App
{

    public static class MainGame
    {
        public const float gravity = 100f;
        private static GridCollisionEngine collisionEngine;

        private static List<FallingLetter> fallingLetters = new List<FallingLetter>();
       

        public static void Init()
        {
            // Load any assets, engine features, or configs here.
            collisionEngine = new GridCollisionEngine(300f); // Initialize the grid collision engine with a cell size of 300

        }





        public static void Update()
        {
            if (Program.GetAsyncKeyState(Keys.Escape) != 0)
            {
                Application.Exit(); // Exit the application if the Escape key is pressed
            }



            foreach (var letter in fallingLetters)
            {
                letter.Tick(Time.deltaTime()); // Update the position of the falling letter
            }

            collisionEngine.UpdateGrid(fallingLetters); // Update the grid with the current positions of the falling letters

            for (int i = 0; i < 15; i++)
            {
                collisionEngine.CheckCollisions(); // Check for collisions between the falling letters
            }

        }

        public static void Render(Graphics g)
        {
            foreach (var letter in fallingLetters) // render each falling letter
            {
                letter.Render(g); // Render the falling letter
            }
        }

        public static void Form1_KeyPress(String letter) // Function to add a new falling letter when a key is pressed
        {
            FallingLetter newLetter = new FallingLetter();
            newLetter.init();
            newLetter.letter = letter;
            fallingLetters.Add(newLetter);
        }
    }
}