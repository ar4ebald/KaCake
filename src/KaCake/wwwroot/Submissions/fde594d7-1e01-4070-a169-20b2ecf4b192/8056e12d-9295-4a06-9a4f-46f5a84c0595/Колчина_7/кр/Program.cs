/* 
 КР2
Дисциплина: "Программирование" 
Группа:   163ПИ/2
Студент:  Колчина Антонина Алексеевна
Вариант:   7
Дата:     7.12.2016
*/


using System;
using ClassLibrary;



class Program
{
    static void Main()
    {
        do
        {
            string str;
            do
            {
                Console.WriteLine("Введите строку из целых чисел, разделенных одним пробелом: ");
                str = Console.ReadLine();
                if (!Methods.Validate(str))
                {
                    Console.WriteLine("Неправильный формат строки!");
                    Console.WriteLine("Строка должна состоять ТОЛЬКО из чисел, разделенных только ОДНИМ пробелом!\n");
                    Console.WriteLine("Попробуйте еще раз: ");
                }
            } while (!Methods.Validate(str));
            int[] testArray = Methods.TestArray(str);
            Sequence[] B = Sequence.TestSequence(testArray);
            Methods.PrintAll(B);
            Console.WriteLine("\nДля выхода из программы нажмите  ESC...\nДля запуска сначала - любую клавишу....");
        } while (Console.ReadKey().Key != ConsoleKey.Escape);
    }
}

