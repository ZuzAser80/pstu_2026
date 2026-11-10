using System;

// Метод половинного деления (дихотомии). Отрезок [a, b], на концах которого
// f(a)*f(b) <= 0, делится пополам; выбирается та половина, где знаки функции
// на концах различны. Метод сходится всегда и приближается к корню с двух
// сторон: левые концы a_n образуют неубывающую, а правые b_n — невозрастающую
// последовательность. Условие останова: b - a < eps.
// Если f(a), f(b) или f(c) обращается в нуль, соответствующий конец или
// середина сразу принимается за точный корень, и счёт прекращается.
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
            + "; f(a)*f(b) <= 0 - корень отделён.");
        Console.WriteLine();
        Console.WriteLine(string.Format("{0,4}{1,14}{2,14}{3,14}{4,15}{5,14}",
            "n", "a", "b", "c=(a+b)/2", "f(c)", "b-a"));

        var fa = Equation.Value(a);
        var fb = Equation.Value(b);

        // Корень уже совпал с концом отрезка - ответ известен сразу.
        if (fa == 0)
        {
            return Finish(a, 0, startA, startB);
        }
        if (fb == 0)
        {
            return Finish(b, 0, startA, startB);
        }

        var steps = 0;
        double c;
        do
        {
            c = 0.5 * (a + b);
            var fc = Equation.Value(c);
            steps++;
            Console.WriteLine(string.Format("{0,4}{1,14}{2,14}{3,14}{4,15}{5,14}",
                steps, Format(a, 8), Format(b, 8), Format(c, 8),
                Format(fc, 8), Format(b - a, 8)));

            // Середина отрезка совпала с корнем - дальше делить не нужно.
            if (fc == 0)
            {
                return Finish(c, steps, startA, startB);
            }

            if (fa * fc < 0)
            {
                b = c;
                fb = fc;
            }
            else
            {
                a = c;
                fa = fc;
            }
        } while (b - a >= epsilon || Equation.Value(c) >= epsilon);

        return Finish(0.5 * (a + b), steps, startA, startB);
    }

    private static RootResult Finish(double root, int steps, double startA, double startB)
    {
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
