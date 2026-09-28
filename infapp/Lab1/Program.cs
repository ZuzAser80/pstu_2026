using System;
using LabsUtil;

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

    // Универсальный ввод данных: система варианта 4 или произвольная система
    private static LinearSystem ReadSystem()
    {
        Console.WriteLine("1 : система варианта 4");
        Console.WriteLine("2 : ввести свою систему");
        var choice = PstuUtil.TryReadT<int>("выберите исходную систему: ");
        if (choice == 1)
        {
            return LinearSystem.CreateVariant4();
        }

        var size = PstuUtil.TryReadT<int>("число уравнений n: ", 1, 6);
        var coefficients = new double[size, size];
        var rightParts = new double[size];
        for (var row = 0; row < size; row++)
        {
            for (var column = 0; column < size; column++)
            {
                coefficients[row, column] = PstuUtil.TryReadT<double>("a" + (row + 1) + (column + 1) + " = ");
            }
            rightParts[row] = PstuUtil.TryReadT<double>("b" + (row + 1) + " = ");
        }
        return new LinearSystem(coefficients, rightParts);
    }

    private static void PrintSolution(string title, double[] solution)
    {
        Console.WriteLine();
        Console.WriteLine(title);
        Console.WriteLine("   " + NumberFormat.Cells(solution, 8));
    }
}
