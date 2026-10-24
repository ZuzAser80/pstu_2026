using System;
using System.Globalization;

// Форматирование чисел для вывода в консоль
static class NumberFormat
{
    private const int CellWidth = 12;

    public static string Format(double value, int digits)
    {
        return value.ToString("F" + digits, CultureInfo.InvariantCulture);
    }

    // Число, выровненное по правому краю в поле фиксированной ширины
    public static string Cell(double value, int digits)
    {
        return Format(value, digits).PadLeft(CellWidth);
    }
}
