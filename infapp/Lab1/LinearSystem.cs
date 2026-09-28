using System;
using System.Text;

// Система линейных алгебраических уравнений Ax = b:
// коэффициенты хранятся в двумерном массиве, свободные члены — в одномерном
class LinearSystem
{
    // Число, меньше которого коэффициент считается нулём (защита от деления на ноль)
    public const double Tiny = 1e-12;

    private const int Digits = 4;

    private readonly double[,] _coefficients;
    private readonly double[] _rightParts;

    public LinearSystem(double[,] coefficients, double[] rightParts)
    {
        _coefficients = coefficients;
        _rightParts = rightParts;
    }

    public int Size => _rightParts.Length;

    public double GetCoefficient(int row, int column) => _coefficients[row, column];

    public double GetRightPart(int row) => _rightParts[row];

    // Вариант 4 из лабораторной работы
    public static LinearSystem CreateVariant4()
    {
        var coefficients = new double[,]
        {
            { 9.1, 5.6, 7.8 },
            { 3.8, 5.1, 2.8 },
            { 4.1, 5.7, 1.2 }
        };
        var rightParts = new double[] { 9.8, 6.7, 5.8 };
        return new LinearSystem(coefficients, rightParts);
    }

    // Собирает систему из расширенных строк (n коэффициентов и свободный член)
    public static LinearSystem FromRows(double[][] extendedRows)
    {
        var size = extendedRows.Length;
        var coefficients = new double[size, size];
        var rightParts = new double[size];
        for (var row = 0; row < size; row++)
        {
            for (var column = 0; column < size; column++)
            {
                coefficients[row, column] = extendedRows[row][column];
            }
            rightParts[row] = extendedRows[row][size];
        }
        return new LinearSystem(coefficients, rightParts);
    }

    public double[] GetExtendedRow(int row)
    {
        var extendedRow = new double[Size + 1];
        for (var column = 0; column < Size; column++)
        {
            extendedRow[column] = _coefficients[row, column];
        }
        extendedRow[Size] = _rightParts[row];
        return extendedRow;
    }

    // Линейная комбинация уравнений системы с целыми коэффициентами:
    // новая строка = c1·уравнение1 + c2·уравнение2 + ... + cn·уравнениеn
    public double[] GetCombinedRow(int[] coefficients)
    {
        var extendedRow = new double[Size + 1];
        for (var k = 0; k < Size; k++)
        {
            var source = GetExtendedRow(k);
            for (var column = 0; column <= Size; column++)
            {
                extendedRow[column] += coefficients[k] * source[column];
            }
        }
        return extendedRow;
    }

    // Отношение суммы модулей внедиагональных элементов строки к её диагональному
    // элементу. Меньше единицы означает строгое диагональное преобладание
    public static double GetDominanceRatio(double[] extendedRow, int diagonalIndex)
    {
        var size = extendedRow.Length - 1;
        var diagonal = Math.Abs(extendedRow[diagonalIndex]);
        if (diagonal < Tiny)
        {
            return double.PositiveInfinity;
        }
        var offDiagonal = 0.0;
        for (var column = 0; column < size; column++)
        {
            if (column == diagonalIndex)
            {
                continue;
            }
            offDiagonal += Math.Abs(extendedRow[column]);
        }
        return offDiagonal / diagonal;
    }

    public bool IsStrictlyDominant()
    {
        for (var row = 0; row < Size; row++)
        {
            if (GetDominanceRatio(GetExtendedRow(row), row) >= 1)
            {
                return false;
            }
        }
        return true;
    }

    public void Print(string title)
    {
        Console.WriteLine(title);
        for (var row = 0; row < Size; row++)
        {
            Console.WriteLine("   " + FormatEquation(GetExtendedRow(row)));
        }
    }

    // Проверка результата подстановкой в исходную систему: печатает остатки r = b - Ax
    public void PrintResidual(string title, double[] solution)
    {
        Console.WriteLine();
        Console.WriteLine(title);
        var norm = 0.0;
        for (var row = 0; row < Size; row++)
        {
            var residual = _rightParts[row];
            for (var column = 0; column < Size; column++)
            {
                residual -= _coefficients[row, column] * solution[column];
            }
            norm = Math.Max(norm, Math.Abs(residual));
            Console.WriteLine("   r" + (row + 1) + " = " + NumberFormat.Format(residual, 10));
        }
        Console.WriteLine("   норма остатка = " + NumberFormat.Format(norm, 10));
    }

    private static string FormatEquation(double[] extendedRow)
    {
        var size = extendedRow.Length - 1;
        var text = new StringBuilder();
        for (var column = 0; column < size; column++)
        {
            if (column == 0)
            {
                text.Append(NumberFormat.Format(extendedRow[column], Digits));
            }
            else
            {
                text.Append(extendedRow[column] < 0 ? " - " : " + ");
                text.Append(NumberFormat.Format(Math.Abs(extendedRow[column]), Digits));
            }
            text.Append(" x").Append(column + 1);
        }
        text.Append(" = ").Append(NumberFormat.Format(extendedRow[size], Digits));
        return text.ToString();
    }
}
