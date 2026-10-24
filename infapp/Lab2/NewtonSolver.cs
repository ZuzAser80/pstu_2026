using System;

// Метод Ньютона (метод касательных). За начальное приближение берётся тот
// конец отрезка, для которого f(x)*f''(x) > 0 (условие сходимости Фурье).
// В точке x_n проводится касательная, её пересечение с осью Ox даёт x_{n+1}:
//     x_{n+1} = x_n - f(x_n) / f'(x_n).
// Метод сходится квадратично, приближение идёт с одной стороны.
static class NewtonSolver
{
    public static RootResult Solve(double a, double b, double epsilon)
    {
        var strongAtA = Equation.Value(a) * Equation.SecondDerivative(a) > 0;
        var x = strongAtA ? a : b;
        var x0 = x;

        Console.WriteLine("3. МЕТОД НЬЮТОНА (КАСАТЕЛЬНЫХ)");
        Console.WriteLine("   Отрезок [" + NumberFormat.Format(a, 2) + "; "
            + NumberFormat.Format(b, 2) + "], точность eps = "
            + NumberFormat.Format(epsilon, 6));
        Console.WriteLine("   f(x)*f''(x) > 0 при x = " + NumberFormat.Format(x0, 2)
            + ", поэтому x0 = " + NumberFormat.Format(x0, 2) + ".");
        Console.WriteLine();
        Console.WriteLine(string.Format("{0,4}{1,14}{2,15}{3,14}{4,16}",
            "n", "x(n)", "f(x(n))", "x(n+1)", "|x(n+1)-x(n)|"));

        var steps = 0;
        double next;
        double delta;
        do
        {
            var fx = Equation.Value(x);
            next = x - fx / Equation.Derivative(x);
            delta = Math.Abs(next - x);
            steps++;
            Console.WriteLine(string.Format("{0,4}{1,14}{2,15}{3,14}{4,16}",
                steps, Format(x, 8), Format(fx, 8), Format(next, 8), Format(delta, 8)));
            x = next;
        } while (delta >= epsilon && Math.Abs(Equation.Value(x)) >= epsilon);

        Console.WriteLine();
        Console.WriteLine("   Корень: x = " + NumberFormat.Format(x, 8)
            + ", f(x) = " + NumberFormat.Format(Equation.Value(x), 8)
            + ", шагов: " + steps);
        Console.WriteLine();

        return new RootResult("касательных (Ньютона)",
            "a=" + NumberFormat.Format(a, 1) + ", b=" + NumberFormat.Format(b, 1)
            + ", x0=" + NumberFormat.Format(x0, 1),
            x0, x, steps);
    }

    private static string Format(double value, int digits)
    {
        return NumberFormat.Format(value, digits);
    }
}
