using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

// Лабораторная работа №2. Методы решения нелинейных уравнений.
// Вариант 4: f(x) = x^3 - 0.1 x^2 + 0.4 x - 1.5 = 0.
// 1) корень отделяется аналитически;
// 2)-6) уточняется методами половинного деления, хорд, касательных,
// комбинированным методом хорд и касательных и методом итераций
// с точностью eps = 10^-6.
//
// Данные можно задать в файле input.txt (см. ReadInput), иначе берётся
// уравнение варианта 4 на отрезке [1; 1.1].
class Program
{
    private const double Epsilon = 1e-6;

    private static void Main()
    {
        Console.WriteLine("Лабораторная работа №2. Методы решения нелинейных уравнений");
        Console.WriteLine("Вариант 4. Уравнение: f(x) = " + Equation.Formula);
        Console.WriteLine();

        var (a, b, coarseStep, fineStep) = ReadInput();

        var (rootA, rootB) = RootSeparation.Separate(a, b, coarseStep, fineStep);
        Console.WriteLine("Корень отделён на отрезке [" + NumberFormat.Format(rootA, 2)
            + "; " + NumberFormat.Format(rootB, 2) + "].");
        Console.WriteLine();

        var results = new List<RootResult>
        {
            BisectionSolver.Solve(rootA, rootB, Epsilon),
            ChordSolver.Solve(rootA, rootB, Epsilon),
            NewtonSolver.Solve(rootA, rootB, Epsilon),
            CombinedSolver.Solve(rootA, rootB, Epsilon),
            IterationSolver.Solve(rootA, rootB, Epsilon),
        };

        PrintSummary(results);
    }

    // Универсальный ввод данных. Файл input.txt (необязательный) содержит:
    //   строка 1: коэффициенты a3 a2 a1 a0;
    //   строка 2: границы отрезка поиска a b;
    //   строка 3: шаги грубого и точного сканирования (необязательно).
    // Если файла нет или он не разобран, используются параметры варианта 4.
    private static (double A, double B, double Coarse, double Fine) ReadInput()
    {
        const double defaultCoarse = 1;
        const double defaultFine = 0.1;
        const string fileName = "input.txt";

        if (!File.Exists(fileName))
        {
            Console.WriteLine("Файл input.txt не найден, используется уравнение варианта 4.");
            Console.WriteLine();
            return (-2, 3, defaultCoarse, defaultFine);
        }

        var parts = new List<double>();
        foreach (var line in File.ReadAllLines(fileName))
        {
            var tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                if (!double.TryParse(token.Replace(',', '.'), NumberStyles.Float,
                        CultureInfo.InvariantCulture, out var value))
                {
                    Console.WriteLine("input.txt: \"" + token + "\" не является числом, "
                        + "используются параметры варианта 4.");
                    Console.WriteLine();
                    return (-2, 3, defaultCoarse, defaultFine);
                }
                parts.Add(value);
            }
        }

        if (parts.Count < 6)
        {
            Console.WriteLine("input.txt: нужно минимум 6 чисел (a3 a2 a1 a0 a b), "
                + "используются параметры варианта 4.");
            Console.WriteLine();
            return (-2, 3, defaultCoarse, defaultFine);
        }

        Equation.Set(parts[0], parts[1], parts[2], parts[3]);
        var coarse = parts.Count > 6 ? parts[6] : defaultCoarse;
        var fine = parts.Count > 7 ? parts[7] : defaultFine;
        Console.WriteLine("Данные прочитаны из input.txt: f(x) = " + Equation.Formula);
        Console.WriteLine();
        return (parts[4], parts[5], coarse, fine);
    }

    // Сводная таблица: метод, задание отрезка и начального приближения,
    // найденный корень, количество шагов, проверка подстановкой.
    private static void PrintSummary(List<RootResult> results)
    {
        Console.WriteLine("СВОДНАЯ ТАБЛИЦА РЕЗУЛЬТАТОВ (eps = 10^-6)");
        Console.WriteLine(string.Format("{0,-28}{1,-26}{2,18}{3,8}",
            "Метод", "a, b, x0", "x", "Шагов"));
        foreach (var result in results)
        {
            Console.WriteLine(string.Format("{0,-28}{1,-26}{2,18}{3,8}",
                result.Method, result.Bounds, NumberFormat.Format(result.Root, 8),
                result.Steps));
        }

        Console.WriteLine();
        Console.WriteLine("Проверка подстановкой: значения f(x) в найденных корнях:");
        foreach (var result in results)
        {
            Console.WriteLine("   " + result.Method.PadRight(28) + " f(x) = "
                + NumberFormat.Format(result.Residual, 12));
        }
    }
}
