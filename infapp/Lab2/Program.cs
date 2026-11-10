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
// Данные читаются только из файла input.txt (см. ReadInput).
class Program
{
    private const double Epsilon = 1e-6;

    private static void Main()
    {
        Console.WriteLine("Лабораторная работа №2. Методы решения нелинейных уравнений");
        Console.WriteLine();

        var (a, b, coarseStep, fineStep) = ReadInput();
        Console.WriteLine("Уравнение: f(x) = " + Equation.Formula);
        Console.WriteLine();

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

    // Ввод данных только из файла input.txt (файл обязателен):
    //   строка 1: коэффициенты a3 a2 a1 a0;
    //   строка 2: границы отрезка поиска a b;
    //   строка 3: шаги грубого и точного сканирования (необязательно).
    // Пустые строки и всё, что начинается с '#', игнорируются.
    // В качестве десятичного разделителя допускаются точка и запятая.
    private static (double A, double B, double Coarse, double Fine) ReadInput()
    {
        const double defaultCoarse = 1;
        const double defaultFine = 0.1;
        const string fileName = "input.txt";

        if (!File.Exists(fileName))
        {
            Fail("файл " + fileName + " не найден");
        }

        var tokens = new List<string>();
        foreach (var rawLine in File.ReadAllLines(fileName))
        {
            var line = rawLine;
            var comment = line.IndexOf('#');
            if (comment >= 0)
            {
                line = line.Substring(0, comment);
            }

            tokens.AddRange(line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        }

        if (tokens.Count < 6)
        {
            Fail("нужно минимум 6 чисел (a3 a2 a1 a0 a b), найдено " + tokens.Count);
        }
        if (tokens.Count > 8)
        {
            Fail("нужно не больше 8 чисел (a3 a2 a1 a0 a b [coarse fine]), найдено "
                + tokens.Count);
        }

        var numbers = new double[tokens.Count];
        for (var i = 0; i < tokens.Count; i++)
        {
            if (!double.TryParse(tokens[i].Replace(',', '.'), NumberStyles.Float,
                    CultureInfo.InvariantCulture, out numbers[i]))
            {
                Fail("\"" + tokens[i] + "\" не является числом");
            }
        }

        var a = numbers[4];
        var b = numbers[5];
        var coarse = numbers.Length > 6 ? numbers[6] : defaultCoarse;
        var fine = numbers.Length > 7 ? numbers[7] : defaultFine;

        if (a >= b)
        {
            Fail("левый конец отрезка a должен быть меньше правого b");
        }
        if (coarse <= 0 || fine <= 0)
        {
            Fail("шаги сканирования должны быть положительными");
        }

        Equation.Set(numbers[0], numbers[1], numbers[2], numbers[3]);
        Console.WriteLine("Данные прочитаны из input.txt.");
        return (a, b, coarse, fine);
    }

    private static void Fail(string message)
    {
        Console.Error.WriteLine("input.txt: " + message + ".");
        Environment.Exit(1);
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
