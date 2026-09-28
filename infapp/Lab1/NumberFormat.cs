using System;
using System.Globalization;

// Форматирование чисел, строк и блоков матриц для вывода в консоль
static class NumberFormat
{
    private const int CellWidth = 11;

    public static string Format(double value, int digits)
    {
        return value.ToString("F" + digits, CultureInfo.CurrentCulture);
    }

    public static string Cell(double value, int digits)
    {
        return Format(value, digits).PadLeft(CellWidth);
    }

    public static string Cells(double[] values, int digits)
    {
        var cells = new string[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            cells[i] = Cell(values[i], digits);
        }
        return string.Join(" ", cells);
    }

    // Печатает прямоугольный блок матрицы: строки от firstRow,
    // столбцы от firstColumn до lastColumn включительно
    public static string Block(double[,] matrix, int firstRow, int firstColumn, int lastColumn, int digits)
    {
        var lines = new string[matrix.GetLength(0) - firstRow];
        for (var row = firstRow; row < matrix.GetLength(0); row++)
        {
            var cells = new string[lastColumn - firstColumn + 1];
            for (var column = firstColumn; column <= lastColumn; column++)
            {
                cells[column - firstColumn] = Cell(matrix[row, column], digits);
            }
            lines[row - firstRow] = string.Join(" ", cells);
        }
        return string.Join(Environment.NewLine, lines);
    }
}
