using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using GameEngine;

namespace Desktop_App
{
    public class FallingLetter
    {
        private static Pen drawingPen;

        // Linear Physics
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Force;
        public float Mass;

        // Angular Physics
        public float Angle;
        public float AngularVelocity;
        public float Torque;
        public float Intertia;

        // Material properties
        public float Bouyency = .2f;
        public float Friction = -0.05f;

        public String letter;

        public Vector2 Size;

        public Vector2[] corners = new Vector2[4];
        public Vector2[] localCorners = new Vector2[4];


        public static Random rand = new Random();

        public static float bottomBoundary = Program.mainForm.Size.Height - 35;
        private static float rightBoundary = Program.mainForm.Size.Width;
        private RectangleF body;

        private StringFormat sf;
        private static Font font = new Font("Arial", 25);

        public void init()
        {
            // Initialize any variables
            Position = new Vector2(FallingLetter.RandomRange(0, FallingLetter.rightBoundary), 0);
            Size = new Vector2(50, 50);
            letter = "A"; // Set the letter to be displayed
            FallingLetter.drawingPen = new Pen(Brushes.Black);
            Velocity = new Vector2(0, 0); // Set the velocity of the falling letter
            body = new System.Drawing.Rectangle(FallingLetter.pointFromVector2(Position), new Size(50, 50)); // Set the size of the falling letter
            Mass = 4f;
            Intertia = Mass * (body.Width * body.Width + body.Height * body.Height) / 12f;
            Intertia *= .25f;

            sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            localCorners = new Vector2[]
            {
                new Vector2(-50/2, -50/2), // Top-Left
                new Vector2(50/2, -50/2),  // Top-Right
                new Vector2(50/2, 50/2),   // Bottom-Right
                new Vector2(-50/2, 50/2)  // Bottom-Left
            };


        }

        public void ClampToBorders()
        {
            // Clamp position to window bounds

            Vector2[] verts = GridCollisionEngine.GetVertecies(this);

            float lowestY = float.MinValue;

            foreach (var v in verts)
            {
                lowestY = Math.Max(lowestY, v.y);
            }

            if (lowestY > FallingLetter.bottomBoundary)
            {
                float penetration = lowestY - FallingLetter.bottomBoundary;
                //Console.WriteLine($"Phyics Y: {Position.y}");
                Position.y -= penetration;

                if (Velocity.y > 0)
                    Velocity.y = 0;
            }


            if (Position.x > FallingLetter.rightBoundary)
            {
                Position.x = FallingLetter.rightBoundary;
            }
            else if (Position.x < 0)
            {
                Position.x = 0;
            }
        }


        public void Tick(float dt)
        {
            // Update the physics of the falling letter based on the time delta (dt)

            // Update Linear Velocity of the falling letter
            Velocity.y += MainGame.gravity * dt;
            Position += Velocity * dt;

            // Update Angular Velocity
            AngularVelocity += (Torque * (1 / Intertia)) * dt;
            //AngularVelocity = 4f;
            Angle += AngularVelocity * dt;

            // Keep Block wihtin bounds
            ClampToBorders();

            Force = Vector2.Zero;
            Torque = 0f;
            AngularVelocity *= MathF.Exp(-3f * dt);
            if (Velocity.x != 0) Velocity.x *= MathF.Exp(-3f * dt);
            // Velocity *= 0.999f;
        }

        public void Render(Graphics g)
        {
            // Render the falling letter
            GraphicsState state = g.Save();

            g.TranslateTransform(Position.x, Position.y);

            g.RotateTransform(Angle * 180f / MathF.PI);


            body = new RectangleF(-Size.x / 2,-Size.y / 2,Size.x,Size.y); ;
            
            g.FillRectangle(Brushes.Black, body);

            g.DrawString(letter.ToString(), font, Brushes.White, body,sf);
            
            g.Restore(state);
        }

        private static Point pointFromVector2(Vector2 a)
        {
            return new Point((int)a.x, (int)a.y);
        }



        private static float RandomRange(float min, float max)
        {
            return (float)(rand.NextDouble() * (max - min) + min);
        }
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(FallingLetter.pointFromVector2(Position), new Size(50, 50));
            }
        }


    }
}