using System;
namespace Neo.Plugins
{
    public class Loger
    {
        public static void Common(string msg)
        {
            if (Settings.Default.Log)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(msg);
            }
        }

        public static void Warning(string msg)
        {
            if (Settings.Default.Log)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(msg);
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }

        public static void Error(string msg)
        {
            if (Settings.Default.Log)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(msg);
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }
    }
}
