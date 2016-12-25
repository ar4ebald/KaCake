using System;
using ClassLibrary;


    class Methods
    {
    //проверка корректности строки
    public static bool Validate(string str)
    {
        if (str == String.Empty)
            return false;
        else
        {
            string digits = String.Empty;
            for (int i = 0; i < 10; i++)
                digits += i.ToString();
            digits += " -";
            bool flag = true;
            for (int i = 0; i < str.Length; i++)
                if (digits.IndexOf(str[i]) < 0 ) flag = false;
            if (str.IndexOf("  ") >= 0) flag = false;
            return flag;
        }
    }

    public static int[] TestArray(string str)
    {
        int[]A = null;
        if (Validate(str))
        {
             A = new int[0];
            foreach (string c in str.Split(' '))
            {
                int a = int.Parse(c);
                if ((a >= -20) && (a <= 20))
                {
                    Array.Resize<int>(ref A, A.Length + 1);
                    A[A.Length - 1] = a;
                }
            }
        }
        return A;
    }

    //вывод
    public static void PrintAll(Sequence[] s)
    {
        Console.WriteLine("Вывод последовательностей: ");
        foreach (Sequence s1 in s)
        {
            try
            {
                if (s1 == null) Console.WriteLine("N/A");
                else Console.WriteLine(s1);
            }
            catch (Exception)
            {
                Console.WriteLine("N/A");
            }
        }
    }
    }

