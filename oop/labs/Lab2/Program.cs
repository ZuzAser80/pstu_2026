using System;

class Program
{
    static void Main()
    {
        int n, k, sum;
        sum = 0;
        Console.WriteLine("input n: ");
        if (int.TryParse(Console.ReadLine(), out n))
        {
            for (int i = 0; i < n; i++)
            {
                if (int.TryParse(Console.ReadLine(), out k))
                {
                    sum += k % 2 == 0 ? 0 : k;
                } else
                {
                    Console.WriteLine("incorrect k input, couldn't parse");
                    return;
                }
            }
        } else
        {
            Console.WriteLine("incorrect n input, couldn't parse");
        }
    }

}