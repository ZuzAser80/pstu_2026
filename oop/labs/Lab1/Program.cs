using System;

class Program
{
    static void Main()
    {        
        int m, n;
        double x, result;
        bool isParsed;
        do
        {
            Console.WriteLine("input n: ");
            isParsed = int.TryParse(Console.ReadLine(), out n);
        } while (!isParsed);
        do
        {
            Console.WriteLine("input m: ");
            isParsed = int.TryParse(Console.ReadLine(), out m);
        } while (!isParsed);
        Console.WriteLine($"1: n={n}, m={m} {m++*n}");
        Console.WriteLine($"2: n={n}, m={m}, (n++) < m = {n++<m}");
        Console.WriteLine($"3: n={n}, m={m}, (--m) > n = {(--m)>n}");
        
        do
        {
            Console.WriteLine("input x: ");
            isParsed = double.TryParse(Console.ReadLine(), out x);
            if (x < 0)
            {
                isParsed = false;
                System.Console.WriteLine("x can't be less then 0");
            }
        } while (!isParsed);
        result = Math.Pow(x - Math.Pow(x, 2) + Math.Pow(x, 5), (double)1/3);
        Console.WriteLine($"result: {result}");

        // task 2

        double x1, y1;
        do
        {
            Console.WriteLine("input x: ");
            isParsed = double.TryParse(Console.ReadLine(), out x1);
        } while (!isParsed);
        do
        {
            Console.WriteLine("input y: ");
            isParsed = double.TryParse(Console.ReadLine(), out y1);
        } while (!isParsed);
        bool isInArea = Math.Sqrt(x1 * x1 + y1 * y1) <= 1;
        Console.WriteLine($"isInArea: {isInArea}");

        // task 3

        var a = 1000f;
        var b = 0.0001;        
        var res = (MathF.Pow((float)(a - b), 3) - MathF.Pow(a, 3)) / (3 * a * (float)b * (float)b - MathF.Pow((float)b, 3) - 3 * a * a * (float)b);
        System.Console.WriteLine($"float a, double b: {res}");
    }
}