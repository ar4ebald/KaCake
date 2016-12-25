using System;
using System.Linq;
class Program {
	static void Main() {
		Console.WriteLine("Started");
		int[] vals = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
		Console.WriteLine(vals.Sum());
	}
}