using System;
using System.Diagnostics;

namespace GameEngine
{
    public static class Time
    {
        private static float lastTime;
        static Time()
        {
            Time.timeStopwatch.Start();
            Time.TickTime();
        }

        public static void TickTime()
        {
            lastTime = Time.time;
            Time.time = (float)Time.timeStopwatch.Elapsed.TotalSeconds;
        }

        public static float deltaTime()
        {
            return Time.time - lastTime;
        }

        public static Stopwatch timeStopwatch = new Stopwatch();
        public static float time;
    }
}