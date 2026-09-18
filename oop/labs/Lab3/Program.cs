using System;

class Program
{
    static void Main()
    {
        const double EPS = 1e-4;
        const int N = 10, K = 10;
        const double A = 0.1, B = 0.8;

        for (int j = 0; j <= K; j++)
        {
            double x = A + (B - A) * j / K;

            double sn = 0, snNum = x * x;
            for (int n = 1; n <= N; n++)
            {
                sn += snNum / (2.0 * n * (2 * n - 1));
                snNum *= -x * x;
            }

            double se = 0, seNum = x * x;
            int m = 1;
            double term = seNum / (2.0 * m * (2 * m - 1));
            while (Math.Abs(term) > EPS)
            {
                se += term;
                seNum *= -x * x;
                m++;
                term = seNum / (2.0 * m * (2 * m - 1));
            }

            double y = x * Math.Atan(x) - Math.Log(Math.Sqrt(1 + x * x));
            Console.WriteLine($"X: {x:F2}, SN: {sn:F4}, SE: {se:F4}, Y: {y:F4}");
        }
    }
}
