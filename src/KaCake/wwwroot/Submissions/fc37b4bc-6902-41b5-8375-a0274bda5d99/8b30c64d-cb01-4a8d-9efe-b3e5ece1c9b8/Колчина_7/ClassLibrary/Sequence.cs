using System;


namespace ClassLibrary
{
    public class Sequence
    {
        int[] s;
        //конструктор класса
        public Sequence(int N)
        {
            if (N < 0) throw new ArgumentOutOfRangeException(null, "Некорректная длина массива!");
            if (N == 0) s = null;
            else
            {
                s = new int[N];
                if (N>=1) s[0] = 0;
                if (N>=2)s[1] = 0;
                if (N>=3) s[2] = 1;
                if (N>=4)
                  for (int i = 3; i < N; i++)
                    s[i] = s[i - 3] + s[i - 2] + s[i - 1];
            }
        }

        //индексатор класса
        public int this[int ind]
        {
            get
            {
                if (ind >= s.Length) throw new IndexOutOfRangeException("Индекс лежит вне диапазонов индексов s!");
                return s[ind];
            }
        }

        //переопределение метода ToString()
        public override string ToString()
        {
            string str = String.Empty;
            foreach (int i in s)
            {
                str += i.ToString() + "; ";
            }
            return str;
        }

        //возвращает массив объектов типа Sequence
        public static Sequence[] TestSequence(int[] test)
        {
            Sequence[] A = new Sequence[test.Length];
            for (int i = 0; i < A.Length; i++)
            {
                try
                {
                    A[i] = new Sequence(test[i]);
                }
                catch (ArgumentOutOfRangeException)
                {
                    A[i] = null;
                    continue;
                }

            }
            return A;
        }
    }
}
