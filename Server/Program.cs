using System;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            
            while(true)
            {
                string command = Console.ReadLine();
                Console.WriteLine("EXECUTED: " + command);
            }
        }
    }
}
