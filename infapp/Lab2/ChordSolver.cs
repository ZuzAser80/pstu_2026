using System;

// Метод хорд (метод секущих с неподвижным концом). Неподвижен тот конец
// отрезка, для которого f(x)*f''(x) > 0; начальным приближением служит
// противоположный конец. Вторая производная на [a, b] знакопостоянна, что
// обеспечивает сходимость. Последовательность приближений монотонна и
// подходит к корню с одной стороны.
static class ChordSolver
{
    public static RootResult Solve(double a, double b, double epsilon)
    {
        var strongAtA = Equation.Value(a) * Equation.SecondDerivative(a) > 0;
        var fixedEnd = strongAtA ? a : b;             // неподвижный конец
        var x = strongAtA ? b : a;                    // начальное приближение
        var x0 = x;

        Console.WriteLine("2. МЕТОД ХОРД");
        Console.WriteLine("   Отрезок [" + NumberFormat.Format(a, 2) + "; "
            + NumberFormat.Format(b, 2) + "], точность eps = "
            + NumberFormat.Format(epsilon, 6));
        Console.WriteLine("   f''(x) = 6x - 0.2 > 0 на отрезке; f(x)*f''(x) > 0 при x = "
            + NumberFormat.Format(fixedEnd, 2) + ", поэтому неподвижен конец "
            + (strongAtA ? "a" : "b") + ", x0 = " + NumberFormat.Format(x0, 2) + ".");
        Console.WriteLine();
        Console.WriteLine(string.Format("{0,4}{1,14}{2,15}{3,14}{4,16}",
            "n", "x(n)", "f(x(n))", "x(n+1)", "|x(n+1)-x(n)|"));

        var steps = 0;
        double next;
        double delta;
        do
        {
            var fx = Equation.Value(x);
            next = x - fx / (fx - Equation.Value(fixedEnd)) * (x - fixedEnd);
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

        return new RootResult("хорд",
            "a=" + NumberFormat.Format(a, 1) + ", b=" + NumberFormat.Format(b, 1)
            + ", x0=" + NumberFormat.Format(x0, 1),
            x0, x, steps);
    }

    private static string Format(double value, int digits)
    {
        return NumberFormat.Format(value, digits);
    }
}
