using System;
using System.IO;

namespace Keken
{
    class Program
    {
        public static void Main(string[] args)
        {
            int n = int.Parse(Console.ReadLine());
			int k = int.Parse(Console.ReadLine());
			Console.WriteLine(k / n);
        }
    }
}