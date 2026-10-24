using System;

// Метод простых итераций. Уравнение f(x) = 0 приводится к виду x = phi(x),
// где phi(x) = x - f(x)/k. Параметр k выбирается одного знака с f'(x) и по
// модулю больше Q/2, где Q = max|f'(x)| на [a, b]; тогда |phi'(x)| <= q < 1
// и последовательность x_{n+1} = phi(x_n) сходится к корню. Оценка точности:
//     |xi - x_n| <= q/(1-q) * |x_n - x_{n-1}|.
static class IterationSolver
{
    public static RootResult Solve(double a, double b, double epsilon)
    {
        var q0 = MaxDerivative(a, b);
        var k = Equation.Derivative(0.5 * (a + b));
        if (Math.Abs(k) <= 0.5 * q0)
        {
            k = Math.Sign(Equation.Derivative(a)) * q0;
        }
        var q = MaxPhiDerivative(a, b, k);

        var x = 0.5 * (a + b);
        var x0 = x;

        Console.WriteLine("5. МЕТОД ИТЕРАЦИЙ");
        Console.WriteLine("   Отрезок [" + NumberFormat.Format(a, 2) + "; "
            + NumberFormat.Format(b, 2) + "], точность eps = "
            + NumberFormat.Format(epsilon, 6));
        Console.WriteLine("   max|f'| = " + NumberFormat.Format(q0, 4)
            + "; принято k = " + NumberFormat.Format(k, 4)
            + " (знак k совпадает со знаком f', |k| > max|f'|/2).");
        Console.WriteLine("   phi(x) = x - f(x)/k; |phi'(x)| <= q = "
            + NumberFormat.Format(q, 4) + " < 1 - условие сходимости выполнено.");
        Console.WriteLine("   Начальное приближение x0 = " + NumberFormat.Format(x0, 2) + ".");
        Console.WriteLine();
        Console.WriteLine(string.Format("{0,4}{1,14}{2,15}{3,16}",
            "n", "x(n)", "f(x(n))", "|x(n)-x(n-1)|"));

        var steps = 0;
        var delta = double.PositiveInfinity;
        do
        {
            var fx = Equation.Value(x);
            var next = x - fx / k;
            delta = Math.Abs(next - x);
            steps++;
            Console.WriteLine(string.Format("{0,4}{1,14}{2,15}{3,16}",
                steps, Format(x, 8), Format(fx, 8), Format(delta, 8)));
            x = next;
        } while (delta >= epsilon);

        var error = q / (1 - q) * delta;
        Console.WriteLine();
        Console.WriteLine("   Корень: x = " + NumberFormat.Format(x, 8)
            + ", f(x) = " + NumberFormat.Format(Equation.Value(x), 8)
            + ", шагов: " + steps);
        Console.WriteLine("   Апостериорная оценка погрешности: |xi - x| <= q/(1-q)*|dx| = "
            + NumberFormat.Format(error, 8));
        Console.WriteLine();

        return new RootResult("итераций",
            "a=" + NumberFormat.Format(a, 1) + ", b=" + NumberFormat.Format(b, 1)
            + ", x0=" + NumberFormat.Format(x0, 2),
            x0, x, steps);
    }

    // max|f'(x)| на отрезке (перебор с мелким шагом)
    private static double MaxDerivative(double a, double b)
    {
        var result = 0.0;
        const int samples = 1000;
        for (var i = 0; i <= samples; i++)
        {
            var x = a + (b - a) * i / samples;
            result = Math.Max(result, Math.Abs(Equation.Derivative(x)));
        }
        return result;
    }

    // max|phi'(x)| = max|1 - f'(x)/k| на отрезке
    private static double MaxPhiDerivative(double a, double b, double k)
    {
        var result = 0.0;
        const int samples = 1000;
        for (var i = 0; i <= samples; i++)
        {
            var x = a + (b - a) * i / samples;
            result = Math.Max(result, Math.Abs(1 - Equation.Derivative(x) / k));
        }
        return result;
    }

    private static string Format(double value, int digits)
    {
        return NumberFormat.Format(value, digits);
    }
}
