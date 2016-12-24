/*
Дисциплина: "Программирование"
Группа: БПИ163_1
Студент: Новоселов Дмитрий
Задача:
Дата: 2016.11.28
Вариант: 6
*/

using System;

namespace Новоселов
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			int n;
			do
			{
				n = parseInt("N=");
				int[] array = new int[n];
				Sequence seqInst = new Sequence(ref n, ref array);
				for (int k = 0; k < n; k++)
				{
					Console.WriteLine(array[k]);
				}
			} while (Console.ReadKey(true).Key != ConsoleKey.Escape);
		}

		//Получение N
		static int parseInt(string cr)
		{
			int inpt;
			do
			{
				Console.Write(cr);
			} while (!int.TryParse(Console.ReadLine(), out inpt) || inpt < 3);
			return inpt;
		}
	}

	class Sequence
	{
		public Sequence(ref int n, ref int[] array)
		{
			for (int i = 0; i < n; i++)
			{
				array[i] = i * i * i;
			}
		}


		//public int[] seq { get { return seq; } set { x = value; } }

		//protected int[] seq;
	}
}
