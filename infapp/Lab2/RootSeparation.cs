using System;
using System.Globalization;
using System.Text;

// Отделение корней уравнения. Сначала корни отделяются аналитически
// (по знаку производной устанавливается число корней), затем отрезок
// сужается по таблице знаков функции f(x) до длины не больше заданной.
static class RootSeparation
{
    // Печатает исследование функции и возвращает отрезок [a, b],
    // содержащий единственный корень. Сначала выполняется грубый проход
    // с шагом coarseStep, затем найденный отрезок сужается с шагом fineStep.
    public static (double A, double B) Separate(double from, double to,
        double coarseStep, double fineStep)
    {
        Console.WriteLine("ОТДЕЛЕНИЕ КОРНЕЙ");
        Console.WriteLine("Уравнение: f(x) = " + Equation.Formula);
        PrintDerivativeAnalysis();
        Console.WriteLine();

        Console.WriteLine("Грубая таблица знаков функции:");
        var (a, b) = FindSignChange(from, to, coarseStep);
        Console.WriteLine();

        Console.WriteLine("Уточнение отрезка с шагом " + NumberFormat.Format(fineStep, 1)
            + ":");
        var refined = FindSignChange(a, b, fineStep);
        Console.WriteLine();
        return refined;
    }

    // Анализ первой производной: её знак показывает, монотонна ли функция
    // и сколько у уравнения действительных корней.
    private static void PrintDerivativeAnalysis()
    {
        var coefficients = Equation.Coefficients;
        var a = 3 * coefficients[0];
        var b = 2 * coefficients[1];
        var c = coefficients[2];

        Console.WriteLine("f'(x) = " + Polynomial(a, b, c));

        if (a == 0)
        {
            Console.WriteLine("Производная не является квадратичной; "
                + "корни отделяются по таблице знаков.");
            return;
        }

        var discriminant = b * b - 4 * a * c;
        Console.WriteLine("D = " + G(discriminant) + ".");
        if (discriminant < 0)
        {
            Console.WriteLine(a > 0
                ? "f'(x) > 0 при всех x => функция строго возрастает, корень единственный."
                : "f'(x) < 0 при всех x => функция строго убывает, корень единственный.");
        }
        else
        {
            Console.WriteLine("f'(x) обращается в нуль; "
                + "корни отделяются по таблице знаков.");
        }
    }

    // Ищет отрезок смены знака функции на [from, to] с заданным шагом,
    // печатая таблицу знаков. Возвращает отрезок [a, b], где f(a)*f(b) <= 0.
    // Точки вычисляются как from + i*step, чтобы не накапливать погрешность
    // и не пропустить точный нуль функции в узле сетки.
    private static (double A, double B) FindSignChange(double from, double to, double step)
    {
        double previousX = from;
        double previousValue = Equation.Value(from);
        Console.WriteLine(Row(previousX, previousValue));

        var count = (int)Math.Floor((to - from) / step + 1e-9);
        for (var i = 1; i <= count; i++)
        {
            var x = from + i * step;
            var value = Equation.Value(x);
            Console.WriteLine(Row(x, value));
            if (Math.Sign(value) != Math.Sign(previousValue))
            {
                return (previousX, x);
            }
            previousX = x;
            previousValue = value;
        }

        // Последняя точка отрезка, если шаг не укладывается в него целиком.
        if (previousX < to - 1e-12)
        {
            var value = Equation.Value(to);
            Console.WriteLine(Row(to, value));
            if (Math.Sign(value) != Math.Sign(previousValue))
            {
                return (previousX, to);
            }
        }

        throw new InvalidOperationException(
            "На отрезке [" + from + ", " + to + "] смена знака не найдена.");
    }

    // Строка таблицы знаков: x и f(x) со знаком
    private static string Row(double x, double value)
    {
        var sign = value > 0 ? "+" : value < 0 ? "-" : "0";
        return "   x = " + NumberFormat.Cell(x, 2)
            + "   f(x) = " + NumberFormat.Cell(value, 4)
            + "   знак: " + sign;
    }

    // Многочлен a*x^2 + b*x + c в удобочитаемом виде
    private static string Polynomial(double a, double b, double c)
    {
        var text = new StringBuilder();
        text.Append(G(a)).Append("x^2");
        text.Append(Term(b, "x"));
        text.Append(Term(c, string.Empty));
        return text.ToString();
    }

    private static string Term(double value, string suffix)
    {
        if (value == 0)
        {
            return string.Empty;
        }

        return (value < 0 ? " - " : " + ") + G(Math.Abs(value)) + suffix;
    }

    private static string G(double value)
    {
        return value.ToString("G10", CultureInfo.InvariantCulture);
    }
}
