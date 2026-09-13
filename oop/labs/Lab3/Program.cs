using System;



class Program
{
    static void Main()
    {
        const double EPS = 1e-4;

        int n = 10, k = 10, n1 = 1;
        double y, x = 0, a = .1, b = .8, sn = 0, se = 0;
        for (int j = 1; j <= k; j++)
        {
            x += (double)((b - a) / k);
            double currentSN = (x * x) / 2, currentSE = (x * x) / 2;

            for (int i = 1; i < n; i++)
            {
                sn += currentSN;
                currentSN *= (-x * x) * (n * (2 * n - 1)) / ((n + 1) * (2 * n + 1));
            }

            while (Math.Abs(currentSE) > EPS)
            {
                se += currentSE;
                currentSE *= (-x * x) * (n1 * (2 * n1 - 1)) / ((n1 + 1) * (2 * n1 + 1));
                n1++;
            }

            y = x * Math.Atan(x) - Math.Log(Math.Sqrt(1 + x * x));
            System.Console.WriteLine($"X: {x}, SN: {sn}, SE: {se}, Y: {y}");
        }
    }

}