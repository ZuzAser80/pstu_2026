using System;

// Метод половинного деления (дихотомии). Отрезок [a, b], на концах которого
// f(a)*f(b) < 0, делится пополам; выбирается та половина, где знаки функции
// на концах различны. Метод сходится всегда и приближается к корню с двух
// сторон: левые концы a_n образуют неубывающую, а правые b_n — невозрастающую
// последовательность. Условие останова: b - a < eps.
static class BisectionSolver
{
    public static RootResult Solve(double a, double b, double epsilon)
    {
        var startA = a;
        var startB = b;

        Console.WriteLine("1. МЕТОД ПОЛОВИННОГО ДЕЛЕНИЯ");
        Console.WriteLine("   Отрезок [" + NumberFormat.Format(a, 2) + "; "
            + NumberFormat.Format(b, 2) + "], точность eps = "
            + NumberFormat.Format(epsilon, 6));
        Console.WriteLine("   f(a) = " + NumberFormat.Format(Equation.Value(a), 4)
            + ", f(b) = " + NumberFormat.Format(Equation.Value(b), 4)
            + "; f(a)*f(b) < 0 - корень отделён.");
        Console.WriteLine();
        Console.WriteLine(string.Format("{0,4}{1,14}{2,14}{3,14}{4,15}{5,14}",
            "n", "a", "b", "c=(a+b)/2", "f(c)", "b-a"));

        var steps = 0;
        double c;
        do
        {
            c = 0.5 * (a + b);
            steps++;
            Console.WriteLine(string.Format("{0,4}{1,14}{2,14}{3,14}{4,15}{5,14}",
                steps, Format(a, 8), Format(b, 8), Format(c, 8),
                Format(Equation.Value(c), 8), Format(b - a, 8)));
            if (Equation.Value(a) * Equation.Value(c) <= 0)
            {
                b = c;
            }
            else
            {
                a = c;
            }
        } while (b - a >= epsilon);

        var root = 0.5 * (a + b);
        Console.WriteLine();
        Console.WriteLine("   Корень: x = " + NumberFormat.Format(root, 8)
            + ", f(x) = " + NumberFormat.Format(Equation.Value(root), 8)
            + ", шагов: " + steps);
        Console.WriteLine();

        return new RootResult("половинного деления",
            "a=" + NumberFormat.Format(startA, 1) + ", b=" + NumberFormat.Format(startB, 1),
            root, root, steps);
    }

    private static string Format(double value, int digits)
    {
        return NumberFormat.Format(value, digits);
    }
}
