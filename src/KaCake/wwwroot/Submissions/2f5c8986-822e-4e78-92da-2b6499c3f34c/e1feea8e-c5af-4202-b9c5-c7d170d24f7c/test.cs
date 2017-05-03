using System;
using System.Linq;
class Program {
	static void Main() {
		Console.WriteLine("Started");
		Console.WriteLine(string.Join(Environment.NewLine, Enumerable.Range(1, 10)));
		Console.WriteLine("Ended");
	}
}