using System;

class Program
{
    static void Main()
    {        
        double x, m, n;
        Console.WriteLine("input m & n, with decimal (e.g. 1.0):");
        if (double.TryParse(Console.ReadLine(), out m) && double.TryParse(Console.ReadLine(), out n))
        {
            Console.WriteLine($"1: {m---n}");
            Console.WriteLine($"2: {(m++)<n}");
            Console.WriteLine($"3: {(n++)>m}");
        } else
        {
            Console.WriteLine("Input data incorrect, (m or n), not numerical?");
        }
        Console.WriteLine("input x with decimal (e.g. 1.0):: ");
        if(double.TryParse(Console.ReadLine(), out x) || x < -1 || x > 1)
        {
            Console.WriteLine($"4 : {Math.Pow(x, 4) - Math.Cos(Math.Asin(x))}");
        } else
        {
            Console.WriteLine("Input data incorrect, x is not numerical or is out of bounds (-1 <= x <= 1).");
        }

    }

    static bool Task2(double x1, double y1)
    {        
        return Math.Sqrt(x1 * x1 + y1 * y1) <= 1;
    }
}