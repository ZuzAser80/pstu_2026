using System;
using System.Globalization;

// Лабораторная работа №1. Методы решения систем линейных алгебраических уравнений.
// Задание 1 — метод Гаусса с выбором главных элементов.
// Задание 2 (чётный вариант) — метод Зейделя с точностью 10^-5
class Program
{
    private const double Epsilon = 1e-5;

    private static void Main(string[] args)
    {
        Console.WriteLine("Лабораторная работа №1. Методы решения систем линейных алгебраических уравнений");
        Console.WriteLine("Вариант 4");
        Console.WriteLine();

        var system = ReadSystem();
        if (system == null)
        {
            return;
        }
        Console.WriteLine();
        system.Print("Исходная система:");
        Console.WriteLine();

        Console.WriteLine("ЗАДАНИЕ 1. Решение методом Гаусса с выбором главных элементов");
        Console.WriteLine();
        var gaussSolution = GaussSolver.Solve(system);
        if (gaussSolution == null)
        {
            Console.WriteLine("Метод Гаусса не дал единственного решения.");
        }
        else
        {
            PrintSolution("Решение методом Гаусса:", gaussSolution);
            system.PrintResidual("Проверка подстановкой в исходную систему:", gaussSolution);
        }
        Console.WriteLine();

        Console.WriteLine("ЗАДАНИЕ 2. Решение методом Зейделя с точностью 10^-5");
        Console.WriteLine();
        var iterationSystem = IterationForm.Build(system);
        double[]? zeidelSolution = null;
        if (iterationSystem != null)
        {
            Console.WriteLine();
            zeidelSolution = ZeidelSolver.Solve(iterationSystem, Epsilon);
            if (zeidelSolution == null)
            {
                Console.WriteLine("Метод Зейделя не сошёлся.");
            }
            else
            {
                PrintSolution("Решение методом Зейделя:", zeidelSolution);
                system.PrintResidual("Проверка подстановкой в исходную систему:", zeidelSolution);
            }
        }
        Console.WriteLine();

        if (gaussSolution == null || zeidelSolution == null)
        {
            return;
        }
        var difference = 0.0;
        for (var i = 0; i < system.Size; i++)
        {
            difference = Math.Max(difference, Math.Abs(gaussSolution[i] - zeidelSolution[i]));
        }
        Console.WriteLine("Сравнение методов: максимальное расхождение решений = "
            + NumberFormat.Format(difference, 10));
    }

    // Чтение системы из файла input.txt в текущей папке.
    // Каждая непустая строка файла — одно уравнение: n коэффициентов и свободный член,
    // записанные через пробел, например "9.1 5.6 7.8 9.8".
    // Возвращает null, если файл отсутствует или не разобран.
    private static LinearSystem? ReadSystem()
    {
        const string fileName = "input.txt";
        if (!File.Exists(fileName))
        {
            Console.WriteLine("Файл " + fileName + " не найден в текущей папке.");
            Console.WriteLine("Программа читает систему из файла: создайте " + fileName
                + " с коэффициентами и свободными членами по одному уравнению в строке.");
            return null;
        }

        var rows = new List<double[]>();
        var lines = File.ReadAllLines(fileName);
        for (var i = 0; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }
            var row = ParseRow(lines[i], i + 1);
            if (row == null)
            {
                return null;
            }
            rows.Add(row);
        }

        if (rows.Count == 0)
        {
            Console.WriteLine("Файл " + fileName + " пуст: система не задана.");
            return null;
        }

        // Система из n уравнений: каждая строка должна содержать n коэффициентов
        // и свободный член
        var expected = rows.Count + 1;
        for (var row = 0; row < rows.Count; row++)
        {
            if (rows[row].Length != expected)
            {
                Console.WriteLine("Строка " + (row + 1) + " файла " + fileName + " содержит "
                    + rows[row].Length + " чисел, а система из " + rows.Count
                    + " уравнений требует " + expected + " (n коэффициентов и свободный член).");
                return null;
            }
        }
        return LinearSystem.FromRows(rows.ToArray());
    }

    // Разбор строки файла в расширенную строку системы, null при ошибке
    private static double[]? ParseRow(string line, int lineNumber)
    {
        var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var row = new double[parts.Length];
        for (var i = 0; i < parts.Length; i++)
        {
            if (!double.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out row[i]))
            {
                Console.WriteLine("Строка " + lineNumber + " файла input.txt: "
                    + "\"" + parts[i] + "\" не является числом.");
                return null;
            }
        }
        return row;
    }

    private static void PrintSolution(string title, double[] solution)
    {
        Console.WriteLine();
        Console.WriteLine(title);
        Console.WriteLine("   " + NumberFormat.Cells(solution, 8));
    }
}
