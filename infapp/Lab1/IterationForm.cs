using System;
using System.Text;

// Приведение системы к виду, удобному для итераций, и проверка условия сходимости.
// Система Ax = b приводится к виду x = Cx + d, где C = I - D^-1·A, d = D^-1·A^-1·b
// (на практике d_i = b_i / a_ii). Для метода Зейделя достаточно, чтобы
// ||C|| < 1 по какой-либо норме матрицы. Проверяются нормы, подчинённые
// октаэдрической (||C||_1 = max по столбцам) и кубической (||C||_∞ = max по строкам)
// нормам векторов
static class IterationForm
{
    // Коэффициенты подбираемых комбинаций уравнений
    private const int MaxCombinationCoefficient = 2;

    // При большей размерности перебор комбинаций слишком велик
    private const int MaxSizeForCombinationSearch = 5;

    private const int Digits = 4;

    // Возвращает систему, пригодную для итераций, либо null,
    // если привести систему не удалось
    public static LinearSystem? Build(LinearSystem source)
    {
        Console.WriteLine("Проверка условия сходимости для исходной системы.");
        if (IsConvergent(source))
        {
            Console.WriteLine("Исходная система уже пригодна для итераций, преобразования не требуются.");
            return source;
        }

        Console.WriteLine("Исходная система не удовлетворяет достаточному условию сходимости.");
        Console.WriteLine("Подбираем эквивалентную систему линейными комбинациями уравнений.");
        Console.WriteLine();

        var prepared = TryCombineRows(source) ?? TryLabb(source);
        if (prepared == null)
        {
            Console.WriteLine("Привести систему к виду, пригодному для итераций, не удалось.");
            return null;
        }

        Console.WriteLine();
        prepared.Print("Полученная система:");
        Console.WriteLine();
        if (!IsConvergent(prepared))
        {
            return null;
        }
        return prepared;
    }

    // Достаточное условие сходимости: ||C||_1 < 1 или ||C||_∞ < 1
    private static bool IsConvergent(LinearSystem system)
    {
        var size = system.Size;
        for (var row = 0; row < size; row++)
        {
            if (Math.Abs(system.GetCoefficient(row, row)) > LinearSystem.Tiny)
            {
                continue;
            }
            Console.WriteLine("   нулевой диагональный элемент a" + (row + 1) + (row + 1)
                + ", разложение x = Cx + d невозможно");
            return false;
        }

        // Диагональные элементы матрицы перехода равны нулю, поэтому в сумму
        // входят только внедиагональные элементы столбца
        var octahedral = 0.0;
        for (var column = 0; column < size; column++)
        {
            var sum = 0.0;
            for (var row = 0; row < size; row++)
            {
                if (column == row)
                {
                    continue;
                }
                sum += Math.Abs(system.GetCoefficient(row, column)) / Math.Abs(system.GetCoefficient(row, row));
            }
            octahedral = Math.Max(octahedral, sum);
        }

        var cubic = 0.0;
        for (var row = 0; row < size; row++)
        {
            var sum = 0.0;
            for (var column = 0; column < size; column++)
            {
                if (column == row)
                {
                    continue;
                }
                sum += Math.Abs(system.GetCoefficient(row, column)) / Math.Abs(system.GetCoefficient(row, row));
            }
            cubic = Math.Max(cubic, sum);
        }

        Console.WriteLine("   норма матрицы, подчинённая октаэдрической: ||C||_1 = "
            + NumberFormat.Format(octahedral, Digits));
        Console.WriteLine("   норма матрицы, подчинённая кубической:       ||C||_∞ = "
            + NumberFormat.Format(cubic, Digits));
        if (octahedral < 1 || cubic < 1)
        {
            Console.WriteLine("   достаточное условие сходимости выполнено, метод сходится");
            return true;
        }
        Console.WriteLine("   достаточное условие сходимости не выполнено ни по одной из норм");
        return false;
    }

    // Подбор линейных комбинаций уравнений с малыми целыми коэффициентами.
    // Для каждой строки перебираются комбинации и выбирается та, у которой
    // диагональный элемент преобладает над остальными. Если преобладание
    // достигнуто во всех строках, матрица невырождена и метод сходится
    private static LinearSystem? TryCombineRows(LinearSystem source)
    {
        var size = source.Size;
        if (size > MaxSizeForCombinationSearch)
        {
            Console.WriteLine("Размер системы велик для подбора комбинаций уравнений.");
            return null;
        }

        Console.WriteLine("Подбираем комбинации уравнений с коэффициентами от -"
            + MaxCombinationCoefficient + " до " + MaxCombinationCoefficient + ".");
        var newRows = new double[size][];
        for (var row = 0; row < size; row++)
        {
            if (!TryFindBestRow(source, row, out var combination, out var extendedRow))
            {
                Console.WriteLine("Для строки " + (row + 1)
                    + " не нашлось комбинации уравнений с преобладающей диагональю.");
                return null;
            }
            Console.WriteLine("   уравнение " + (row + 1) + " = " + DescribeCombination(combination));
            newRows[row] = extendedRow;
        }
        return LinearSystem.FromRows(newRows);
    }

    private static bool TryFindBestRow(LinearSystem source, int row, out int[] combination, out double[] extendedRow)
    {
        var size = source.Size;
        combination = new int[size];
        extendedRow = new double[size + 1];
        var candidate = new int[size];
        var baseValue = 2 * MaxCombinationCoefficient + 1;
        var variants = (int)Math.Pow(baseValue, size);
        var isFound = false;
        var bestRatio = 0.0;
        var bestNonZeroCount = 0;
        var bestMaxAbs = 0;

        for (var index = 0; index < variants; index++)
        {
            DecodeCoefficients(index, candidate, baseValue);
            if (IsZeroCombination(candidate))
            {
                continue;
            }
            MakeFirstCoefficientPositive(candidate);
            var combined = source.GetCombinedRow(candidate);
            var ratio = LinearSystem.GetDominanceRatio(combined, row);
            if (ratio >= 1)
            {
                continue;
            }
            var nonZeroCount = CountNonZero(candidate);
            var maxAbs = MaxAbs(candidate);
            if (isFound && !IsBetter(ratio, nonZeroCount, maxAbs, bestRatio, bestNonZeroCount, bestMaxAbs))
            {
                continue;
            }
            isFound = true;
            bestRatio = ratio;
            bestNonZeroCount = nonZeroCount;
            bestMaxAbs = maxAbs;
            Array.Copy(candidate, combination, size);
            Array.Copy(combined, extendedRow, size + 1);
        }
        return isFound;
    }

    // Метод Лабба: прямой ход метода Гаусса даёт треугольную систему,
    // добавление к её диагонали числа delta эквивалентно линейной комбинации
    // уравнений и всегда позволяет получить преобладание диагонали
    private static LinearSystem? TryLabb(LinearSystem source)
    {
        var size = source.Size;
        var upper = new double[size, size];
        var rightParts = new double[size];
        for (var row = 0; row < size; row++)
        {
            for (var column = 0; column < size; column++)
            {
                upper[row, column] = source.GetCoefficient(row, column);
            }
            rightParts[row] = source.GetRightPart(row);
        }

        for (var row = 1; row < size; row++)
        {
            for (var previous = 0; previous < row; previous++)
            {
                var factor = upper[row, previous] / upper[previous, previous];
                upper[row, previous] = 0;
                for (var column = previous + 1; column < size; column++)
                {
                    upper[row, column] -= factor * upper[previous, column];
                }
                rightParts[row] -= factor * rightParts[previous];
            }
            if (Math.Abs(upper[row, row]) < LinearSystem.Tiny)
            {
                Console.WriteLine("Метод Лабба неприменим: система вырождена.");
                return null;
            }
        }

        var maxRatio = 0.0;
        for (var row = 0; row < size; row++)
        {
            var extendedRow = new double[size + 1];
            for (var column = 0; column < size; column++)
            {
                extendedRow[column] = upper[row, column];
            }
            extendedRow[size] = rightParts[row];
            maxRatio = Math.Max(maxRatio, LinearSystem.GetDominanceRatio(extendedRow, row));
        }
        var delta = Math.Max(1e-6, maxRatio - 1 + 0.01);
        Console.WriteLine("Комбинации не найдены, применяем метод Лабба: U + delta·D, delta = "
            + NumberFormat.Format(delta, Digits));

        for (var row = 0; row < size; row++)
        {
            var factor = rightParts[row] / upper[row, row];
            for (var column = 0; column < size; column++)
            {
                if (column == row)
                {
                    upper[row, column] += delta * upper[row, column];
                }
            }
            rightParts[row] += delta * factor;
        }
        return LinearSystem.FromRows(GetRows(upper, rightParts));
    }

    private static double[][] GetRows(double[,] coefficients, double[] rightParts)
    {
        var size = rightParts.Length;
        var rows = new double[size][];
        for (var row = 0; row < size; row++)
        {
            var extendedRow = new double[size + 1];
            for (var column = 0; column < size; column++)
            {
                extendedRow[column] = coefficients[row, column];
            }
            extendedRow[size] = rightParts[row];
            rows[row] = extendedRow;
        }
        return rows;
    }

    private static void DecodeCoefficients(int index, int[] candidate, int baseValue)
    {
        for (var i = 0; i < candidate.Length; i++)
        {
            candidate[i] = index % baseValue - MaxCombinationCoefficient;
            index /= baseValue;
        }
    }

    // Комбинация, отличающаяся знаком, задаёт ту же систему,
    // поэтому оставляем первый ненулевой коэффициент положительным
    private static void MakeFirstCoefficientPositive(int[] candidate)
    {
        foreach (var coefficient in candidate)
        {
            if (coefficient == 0)
            {
                continue;
            }
            if (coefficient > 0)
            {
                return;
            }
            for (var i = 0; i < candidate.Length; i++)
            {
                candidate[i] = -candidate[i];
            }
            return;
        }
    }

    private static bool IsZeroCombination(int[] candidate)
    {
        foreach (var coefficient in candidate)
        {
            if (coefficient != 0)
            {
                return false;
            }
        }
        return true;
    }

    private static int CountNonZero(int[] candidate)
    {
        var count = 0;
        foreach (var coefficient in candidate)
        {
            if (coefficient != 0)
            {
                count++;
            }
        }
        return count;
    }

    private static int MaxAbs(int[] candidate)
    {
        var maxAbs = 0;
        foreach (var coefficient in candidate)
        {
            maxAbs = Math.Max(maxAbs, Math.Abs(coefficient));
        }
        return maxAbs;
    }

    // При равном отношении преобладания выбирается самая простая комбинация
    private static bool IsBetter(double ratio, int nonZeroCount, int maxAbs, double bestRatio, int bestNonZeroCount, int bestMaxAbs)
    {
        if (ratio < bestRatio - 1e-12)
        {
            return true;
        }
        if (ratio > bestRatio + 1e-12)
        {
            return false;
        }
        if (nonZeroCount != bestNonZeroCount)
        {
            return nonZeroCount < bestNonZeroCount;
        }
        return maxAbs < bestMaxAbs;
    }

    private static string DescribeCombination(int[] combination)
    {
        var text = new StringBuilder();
        for (var k = 0; k < combination.Length; k++)
        {
            if (combination[k] == 0)
            {
                continue;
            }
            var term = (Math.Abs(combination[k]) == 1 ? "" : Math.Abs(combination[k]) + "*") + "уравнение" + (k + 1);
            if (text.Length == 0)
            {
                text.Append(combination[k] < 0 ? "-" : "").Append(term);
                continue;
            }
            text.Append(combination[k] < 0 ? " - " : " + ").Append(term);
        }
        return text.ToString();
    }
}
