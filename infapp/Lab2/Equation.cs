using System;
using System.Globalization;
using System.Text;

// Уравнение вида  a3*x^3 + a2*x^2 + a1*x + a0 = 0.
// По умолчанию используется уравнение варианта 4:
//     x^3 - 0.1 x^2 + 0.4 x - 1.5 = 0.
//
// Аналитическое отделение корней. Первая производная
//     f'(x) = 3*a3*x^2 + 2*a2*x + a1
// для варианта 4 имеет дискриминант D = 0.2^2 - 4*3*0.4 = -4.76 < 0, поэтому
// f'(x) > 0 при всех x: функция строго возрастает и уравнение имеет ровно
// один действительный корень. Вторая производная выпукла вниз при x > 0.
static class Equation
{
    private static double c3 = 1.0;
    private static double c2 = -0.1;
    private static double c1 = 0.4;
    private static double c0 = -1.5;

    public static void Set(double a3, double a2, double a1, double a0)
    {
        c3 = a3;
        c2 = a2;
        c1 = a1;
        c0 = a0;
    }

    // Значение функции f(x) по схеме Горнера
    public static double Value(double x)
    {
        return ((c3 * x + c2) * x + c1) * x + c0;
    }

    // Первая производная f'(x)
    public static double Derivative(double x)
    {
        return (3 * c3 * x + 2 * c2) * x + c1;
    }

    // Вторая производная f''(x)
    public static double SecondDerivative(double x)
    {
        return 6 * c3 * x + 2 * c2;
    }

    // Коэффициенты уравнения
    public static double[] Coefficients => new[] { c3, c2, c1, c0 };

    // Текстовая запись уравнения
    public static string Formula
    {
        get
        {
            var text = new StringBuilder();
            text.Append(Term(c3, "x^3", true));
            text.Append(Term(c2, "x^2", false));
            text.Append(Term(c1, "x", false));
            text.Append(Term(c0, "", false));
            return text.Append(" = 0").ToString();
        }
    }

    // Печатает одно слагаемое. Первое слагаемое печатается без знака '+',
    // единичные коэффициенты (1 и -1) при неизвестном опускаются.
    private static string Term(double value, string suffix, bool first)
    {
        var text = new StringBuilder();
        if (Math.Abs(value) < 1e-15)
        {
            return first ? "0" : string.Empty;
        }

        if (first)
        {
            if (value < 0)
            {
                text.Append('-');
            }
        }
        else
        {
            text.Append(value < 0 ? " - " : " + ");
        }

        var magnitude = Math.Abs(value);
        if (suffix.Length == 0 || Math.Abs(magnitude - 1) > 1e-15)
        {
            text.Append(Format(magnitude));
            if (suffix.Length > 0)
            {
                text.Append(' ');
            }
        }
        text.Append(suffix);
        return text.ToString();
    }

    private static string Format(double value)
    {
        return value.ToString("G", CultureInfo.InvariantCulture);
    }
}
