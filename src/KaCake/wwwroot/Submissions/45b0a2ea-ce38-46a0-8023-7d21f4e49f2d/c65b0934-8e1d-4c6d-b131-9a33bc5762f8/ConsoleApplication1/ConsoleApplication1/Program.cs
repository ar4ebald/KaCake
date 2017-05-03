using System;


class Program
{
    static void Main(string[] args)
    {
        int a, b; double z;
        while (true)
        {
            Console.WriteLine("Введите длину a прямоугольника: ");
            if (int.TryParse(Console.ReadLine(), out a))
                break;
        }
        while (true)
        {
            Console.WriteLine("Введите ширину b прямоугольника: ");
            if (int.TryParse(Console.ReadLine(), out b))
                break;
        }
        z = (double)(((a * b) / 4));
        Console.WriteLine("Площадь треугольника: " + z.ToString("f3"));
    }
}
