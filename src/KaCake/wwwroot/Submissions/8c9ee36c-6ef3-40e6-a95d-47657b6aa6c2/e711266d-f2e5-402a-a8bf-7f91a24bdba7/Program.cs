using System;
using System.IO;
using System.Linq;
class Program {
	static void Main() {
		Console.WriteLine(string.Join(Environment.NewLine, Directory.EnumerateFileSystemEntries(Environment.CurrentDirectory)));
	}
}