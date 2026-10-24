using System;

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
        Console.WriteLine("f'(x) = 3x^2 - 0.2x + 0.4, D = 0.04 - 4.8 = -4.76 < 0 => f'(x) > 0,");
        Console.WriteLine("функция строго возрастает, значит корень единственный.");
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

    // Ищет отрезок смены знака функции на [from, to] с заданным шагом,
    // печатая таблицу знаков. Возвращает отрезок [a, b], где f(a)*f(b) < 0.
    private static (double A, double B) FindSignChange(double from, double to, double step)
    {
        double previousX = from;
        double previousValue = Equation.Value(from);
        Console.WriteLine(Row(previousX, previousValue));

        for (var x = from + step; x <= to + 1e-9; x += step)
        {
            var value = Equation.Value(x);
            Console.WriteLine(Row(x, value));
            if (Math.Sign(value) != Math.Sign(previousValue))
            {
                return (previousX, x);
            }
            previousX = x;
            previousValue = value;
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
}
