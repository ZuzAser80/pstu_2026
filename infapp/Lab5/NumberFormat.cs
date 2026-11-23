using System;
using System.Globalization;

// Форматирование чисел при выводе таблиц в консоль.
// Используется инвариантная культура: разделитель целой и дробной части — точка.
internal static class NumberFormat
{
    public const int CellWidth = 18;

    public static string Format(double value, int digits)
    {
        return value.ToString("F" + digits, CultureInfo.InvariantCulture);
    }

    // Число, выровненное по правому краю в поле фиксированной ширины.
    public static string Cell(double value, int digits)
    {
        return Format(value, digits).PadLeft(CellWidth);
    }

    // Целое число, выровненное по правому краю в том же поле.
    public static string Cell(int value)
    {
        return value.ToString(CultureInfo.InvariantCulture).PadLeft(CellWidth);
    }

    // Число в экспоненциальной форме, например 1.227E-012.
    public static string Scientific(double value, int digits)
    {
        return value.ToString("E" + digits, CultureInfo.InvariantCulture);
    }

    // Заголовок столбца, выровненный по правому краю так же, как значения.
    public static string Header(string text)
    {
        if (text.Length >= CellWidth)
        {
            return text;
        }

        return text.PadLeft(CellWidth);
    }
}
