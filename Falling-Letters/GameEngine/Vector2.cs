using System;

namespace GameEngine
{
   public struct Vector2
    {
        public float x;
        public float y;
        public static readonly Vector2 Zero = new Vector2(0,0);

        public static readonly Vector2 One = new Vector2(1,1);

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vector2 operator +(Vector2 a, Vector2 b) // Handle addition of two Vector2 objects
        {
            return new Vector2(a.x + b.x ,a.y + b.y);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b) // Handle subtraction of two Vector2 objects
        {
            return new Vector2(a.x - b.x ,a.y - b.y);
        }

        public static Vector2 operator -(Vector2 a) // Handle negation of a Vector2 object
        {
            return new Vector2(-a.x, -a.y);
        }

		public static Vector2 operator *(Vector2 a, Vector2 b)
		{
			return new Vector2(a.x * b.x, a.y * b.y);
		}

        public static Vector2 operator *(Vector2 a, float b) // Handle multiplication of a Vector2 object by a scalar
        {
            return new Vector2(a.x * b, a.y * b);
        }

         public static Vector2 operator *(float b, Vector2 a) // Handle multiplication of a Vector2 object by a scalar
        {
            return new Vector2(a.x * b, a.y * b);
        }

        public static Vector2 operator /(Vector2 a, float b) // Handle division of a Vector2 object by a scalar
        {
            return new Vector2(a.x / b, a.y / b);
        }

        public static Vector2 GetFromAngleDegrees(float angle)
		{
			return new Vector2((float)Math.Cos((double)(angle * 0.0174532924f)), (float)Math.Sin((double)(angle * 0.0174532924f)));
		}

        public static float Magnitude(Vector2 a) // Calculate and return magnitude of Vector2
        {
            return (float)Math.Sqrt(a.x * a.x + a.y * a.y);
        }


        public static float Dot(Vector2 a, Vector2 b)
        {
            return a.x * b.x + a.y * b.y;
        }

        public static float Distance(Vector2 a, Vector2 b)
        {
            return Vector2.Magnitude(a - b);
        }

        public static Vector2 Lerp(Vector2 a, Vector2 b, float c)
        {
            return a + (b - a) * c;
        }

        public static Vector2 Normalize(Vector2 a)
        {
            float magnitude = Vector2.Magnitude(a);
            if (magnitude == 0f)
            {
                return Vector2.Zero;
            }
            return a / magnitude;
        }

         public static Vector2 Unit(Vector2 a) // Calculate and return unit vector of Vector2
        {
            Vector2 normalized = Vector2.Normalize(a);
            float magnitude = Vector2.Magnitude(normalized);
            return normalized / magnitude;
        }

        public static float Cross(Vector2 a, Vector2 b)
        {
            return (a.x * b.y) - (a.y * b.x);
        }

        public static Vector2 Cross(float a, Vector2 b)
        {
            return new Vector2(-a * b.y, a * b.x);
        }

        public static bool operator ==(Vector2 a, Vector2 b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(Vector2 a, Vector2 b)
        {
            return a.x != b.x && a.y != b.y;
        }

    }
}