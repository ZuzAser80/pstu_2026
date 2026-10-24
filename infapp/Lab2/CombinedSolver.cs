using System;

// Комбинированный метод хорд и касательных. Корень уточняется одновременно
// с двух сторон: по недостатку — методом хорд, по избытку — методом
// касательных. Обозначим x - приближение по недостатку, y - по избытку.
// Неподвижный при хордах конец и конец для касательных выбираются по знаку
// f(x)*f''(x). На каждом шаге
//     x_{n+1} = x_n - f(x_n)/(f(x_n) - f(y_n)) * (x_n - y_n),
//     y_{n+1} = y_n - f(y_n)/f'(y_n).
// Процесс заканчивается, когда y_n - x_n < eps; корень лежит между x_n и y_n.
static class CombinedSolver
{
    public static RootResult Solve(double a, double b, double epsilon)
    {
        var strongAtA = Equation.Value(a) * Equation.SecondDerivative(a) > 0;
        // Метод хорд идёт от слабого конца, метод касательных — от сильного
        var x = strongAtA ? b : a;    // по недостатку (хорды)
        var y = strongAtA ? a : b;    // по избытку (касательные)
        var x0 = x;
        var y0 = y;

        Console.WriteLine("4. КОМБИНИРОВАННЫЙ МЕТОД ХОРД И КАСАТЕЛЬНЫХ");
        Console.WriteLine("   Отрезок [" + NumberFormat.Format(a, 2) + "; "
            + NumberFormat.Format(b, 2) + "], точность eps = "
            + NumberFormat.Format(epsilon, 6));
        Console.WriteLine("   f''(x) > 0; f(x)*f''(x) > 0 при x = "
            + NumberFormat.Format(y0, 2) + ", поэтому касательные строятся из x = "
            + NumberFormat.Format(y0, 2) + ", хорды — из x = " + NumberFormat.Format(x0, 2) + ".");
        Console.WriteLine();
        Console.WriteLine(string.Format("{0,4}{1,14}{2,14}{3,15}{4,15}{5,14}",
            "n", "x(n)", "y(n)", "f(x(n))", "f(y(n))", "y(n)-x(n)"));

        var steps = 0;
        do
        {
            var fx = Equation.Value(x);
            var fy = Equation.Value(y);
            var xNext = x - fx / (fx - fy) * (x - y);
            var yNext = y - fy / Equation.Derivative(y);
            steps++;
            Console.WriteLine(string.Format("{0,4}{1,14}{2,14}{3,15}{4,15}{5,14}",
                steps, Format(x, 8), Format(y, 8), Format(fx, 8), Format(fy, 8),
                Format(y - x, 8)));
            x = xNext;
            y = yNext;
        } while (y - x >= epsilon);

        var root = 0.5 * (x + y);
        Console.WriteLine();
        Console.WriteLine("   Корень заключён между " + NumberFormat.Format(x, 8)
            + " и " + NumberFormat.Format(y, 8));
        Console.WriteLine("   Корень: x = " + NumberFormat.Format(root, 8)
            + ", f(x) = " + NumberFormat.Format(Equation.Value(root), 8)
            + ", шагов: " + steps);
        Console.WriteLine();

        return new RootResult("комбинированный",
            "x0=" + NumberFormat.Format(x0, 1) + ", y0=" + NumberFormat.Format(y0, 1),
            x0, root, steps);
    }

    private static string Format(double value, int digits)
    {
        return NumberFormat.Format(value, digits);
    }
}
