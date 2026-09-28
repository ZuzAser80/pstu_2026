using System;

// Метод Гаусса с выбором главных элементов (метод главных элементов).
// На каждом шаге выбирается наибольший по модулю элемент оставшегося блока,
// переносится на главную диагональ перестановкой строк и столбцов,
// после чего его столбец исключается из остальных строк
static class GaussSolver
{
    private const int Digits = 4;
    private const int MultiplierDigits = 6;
    private const int SolutionDigits = 8;

    public static double[]? Solve(LinearSystem system)
    {
        var size = system.Size;

        // Расширенная матрица системы: n строк и n + 1 столбцов
        var extended = new double[size, size + 1];
        for (var row = 0; row < size; row++)
        {
            for (var column = 0; column < size; column++)
            {
                extended[row, column] = system.GetCoefficient(row, column);
            }
            extended[row, size] = system.GetRightPart(row);
        }

        // При перестановке столбцов меняется нумерация неизвестных
        var variableOrder = new int[size];
        for (var i = 0; i < size; i++)
        {
            variableOrder[i] = i;
        }

        // Прямой ход
        for (var step = 0; step < size; step++)
        {
            FindMainElement(extended, step, out var mainRow, out var mainColumn, out var mainValue);

            // Деление на ноль исключено: если главного элемента нет,
            // система не имеет единственного решения
            if (Math.Abs(mainValue) < LinearSystem.Tiny)
            {
                ReportNoUniqueSolution(extended, step);
                return null;
            }

            Console.WriteLine("Шаг " + (step + 1) + ". Главный элемент a"
                + (mainRow + 1) + (mainColumn + 1) + " = "
                + NumberFormat.Format(mainValue, Digits));
            SwapRows(extended, mainRow, step);
            SwapColumns(extended, mainColumn, step);
            (variableOrder[step], variableOrder[mainColumn]) = (variableOrder[mainColumn], variableOrder[step]);
            if (mainColumn != step)
            {
                Console.WriteLine("   столбец " + (mainColumn + 1) + " перенесён на место столбца "
                    + (step + 1) + ", нумерация неизвестных меняется");
            }

            for (var row = step + 1; row < size; row++)
            {
                var multiplier = extended[row, step] / extended[step, step];
                extended[row, step] = 0;
                for (var column = step + 1; column <= size; column++)
                {
                    extended[row, column] -= multiplier * extended[step, column];
                }
                Console.WriteLine("   множитель m" + (row + 1) + " = "
                    + NumberFormat.Format(multiplier, MultiplierDigits));
            }

            Console.WriteLine("   приведённая система:");
            Console.WriteLine(NumberFormat.Block(extended, step, step, size, Digits));
            Console.WriteLine();
        }

        // Обратный ход
        var current = new double[size];
        for (var row = size - 1; row >= 0; row--)
        {
            if (Math.Abs(extended[row, row]) < LinearSystem.Tiny)
            {
                Console.WriteLine("Диагональный элемент равен нулю, система не имеет единственного решения.");
                Console.WriteLine("Деление на ноль не выполняется.");
                return null;
            }
            var value = extended[row, size];
            for (var column = row + 1; column < size; column++)
            {
                value -= extended[row, column] * current[column];
            }
            current[row] = value / extended[row, row];
            Console.WriteLine("   x" + (variableOrder[row] + 1) + " = "
                + NumberFormat.Format(current[row], SolutionDigits));
        }

        // Возвращаем решение в исходной нумерации неизвестных
        var solution = new double[size];
        for (var i = 0; i < size; i++)
        {
            solution[variableOrder[i]] = current[i];
        }
        return solution;
    }

    // Наибольший по модулю элемент блока, начинающегося с шага step
    private static void FindMainElement(double[,] extended, int step, out int mainRow, out int mainColumn, out double mainValue)
    {
        var size = extended.GetLength(0);
        mainRow = step;
        mainColumn = step;
        mainValue = 0;
        for (var row = step; row < size; row++)
        {
            for (var column = step; column < size; column++)
            {
                if (Math.Abs(extended[row, column]) <= Math.Abs(mainValue))
                {
                    continue;
                }
                mainValue = extended[row, column];
                mainRow = row;
                mainColumn = column;
            }
        }
    }

    private static void SwapRows(double[,] extended, int first, int second)
    {
        if (first == second)
        {
            return;
        }
        var width = extended.GetLength(1);
        for (var column = 0; column < width; column++)
        {
            var value = extended[first, column];
            extended[first, column] = extended[second, column];
            extended[second, column] = value;
        }
    }

    // Столбец свободных членов не переставляется
    private static void SwapColumns(double[,] extended, int first, int second)
    {
        if (first == second)
        {
            return;
        }
        var height = extended.GetLength(0);
        for (var row = 0; row < height; row++)
        {
            var value = extended[row, first];
            extended[row, first] = extended[row, second];
            extended[row, second] = value;
        }
    }

    // Все элементы столбца ниже диагонального нулевые, поэтому определитель
    // равен нулю. Система либо несовместна, либо имеет бесконечно много решений
    private static void ReportNoUniqueSolution(double[,] extended, int step)
    {
        var size = extended.GetLength(0);
        var isConsistent = true;
        for (var row = step; row < size; row++)
        {
            var isZero = true;
            for (var column = step; column < size; column++)
            {
                if (Math.Abs(extended[row, column]) <= LinearSystem.Tiny)
                {
                    continue;
                }
                isZero = false;
                break;
            }
            if (isZero && Math.Abs(extended[row, size]) > LinearSystem.Tiny)
            {
                isConsistent = false;
            }
        }

        Console.WriteLine("Главный элемент не найден: все элементы оставшегося блока равны нулю.");
        if (isConsistent)
        {
            Console.WriteLine("Определитель системы равен нулю, система имеет бесконечно много решений.");
        }
        else
        {
            Console.WriteLine("Определитель системы равен нулю и система несовместна, решения нет.");
        }
        Console.WriteLine("Деление на ноль не выполняется, аварийное завершение исключено.");
    }
}
