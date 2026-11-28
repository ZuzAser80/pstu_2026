using System;
using System.IO;
using System.Text;

// Лабораторная работа №4. Численное интегрирование.
// Вариант 4: интеграл от 2.2 до 3.4 функции x^2/(x^2+1).
//
// Программа реализует четыре задания:
//   1) формулу трапеций с удвоением числа разбиений и уточнением по Рунге;
//   2) формулу Симпсона с тем же алгоритмом;
//   3) квадратуры Гаусса–Лежандра с 4 и 5 узлами;
//   4) сравнение результатов с точным значением интеграла.
//
// Исходные данные обязательно читаются из файла input.txt (см. InputData).
internal static class Program
{
    private const string InputFileName = "input.txt";

    private static int Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("==============================================================");
        Console.WriteLine(" Лабораторная работа №4. Численное интегрирование");
        Console.WriteLine(" Вариант 4");
        Console.WriteLine("==============================================================");
        Console.WriteLine();

        InputData data;
        try
        {
            data = InputData.Read(InputFileName);
        }
        catch (Exception exception) when (exception is InputDataException
            or IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine("Ошибка чтения исходных данных: " + exception.Message);
            Console.Error.WriteLine("Программа ожидает файл " + InputFileName
                + " в текущем каталоге.");
            return 1;
        }

        Console.WriteLine("Исходные данные прочитаны из " + InputFileName + ".");
        PrintInputData(data);

        double exact = Integrand.ExactIntegral(data.A, data.B);
        Console.WriteLine("Точное значение: I = (b - arctg b) - (a - arctg a) = "
            + NumberFormat.Format(exact, 12) + ".");
        Console.WriteLine();

        RefinementResult trapezoid;
        RefinementResult simpson;
        GaussResult gauss4;
        GaussResult gauss5;
        try
        {
            trapezoid = Quadrature.Trapezoid(data.A, data.B, data.Epsilon, data.InitialParts);
            simpson = Quadrature.Simpson(data.A, data.B, data.Epsilon, data.InitialParts);
            gauss4 = GaussQuadrature.Integrate(data.A, data.B, 4);
            gauss5 = GaussQuadrature.Integrate(data.A, data.B, 5);
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidOperationException or OverflowException)
        {
            Console.Error.WriteLine("Ошибка вычисления интеграла: " + exception.Message);
            return 1;
        }

        PrintRefinement(trapezoid, data.Epsilon, "Задание 1. Формула трапеций");
        PrintRefinement(simpson, data.Epsilon, "Задание 2. Формула Симпсона");
        PrintGauss(gauss4);
        PrintGauss(gauss5);
        PrintComparison(exact, trapezoid, simpson, gauss4, gauss5);
        PrintConclusion(exact, trapezoid, simpson, gauss4, gauss5);

        return 0;
    }

    private static void PrintInputData(InputData data)
    {
        Console.WriteLine("Параметры задачи:");
        Console.WriteLine(NumberFormat.Header("a")
            + NumberFormat.Header("b")
            + NumberFormat.Header("epsilon")
            + NumberFormat.Header("n0"));
        Console.WriteLine(new string('-', NumberFormat.CellWidth * 4));
        Console.WriteLine(NumberFormat.Cell(data.A, 6)
            + NumberFormat.Cell(data.B, 6)
            + NumberFormat.Cell(data.Epsilon, 8)
            + NumberFormat.Cell(data.InitialParts));
        Console.WriteLine();
        Console.WriteLine("Подынтегральная функция (вариант 4): f(x) = x^2 / (x^2 + 1).");
        Console.WriteLine();
    }

    private static void PrintRefinement(RefinementResult result, double epsilon, string title)
    {
        Console.WriteLine(title);
        Console.WriteLine("Шаг удвоения: n -> 2n; критерий остановки: |I_2n - I_n| < "
            + NumberFormat.Format(epsilon, 8) + ".");
        Console.WriteLine("Порядок остаточного члена m = " + result.Order
            + "; формула Рунге: I = I_2n + (I_2n - I_n) / (2^m - 1).");
        Console.WriteLine();

        Console.WriteLine(NumberFormat.Header("n")
            + NumberFormat.Header("h")
            + NumberFormat.Header("I_n")
            + NumberFormat.Header("|I_2n-I_n|")
            + NumberFormat.Header("I_Рунге"));
        Console.WriteLine(new string('-', NumberFormat.CellWidth * 5));

        foreach (var step in result.Steps)
        {
            Console.Write(NumberFormat.Cell(step.Parts));
            Console.Write(NumberFormat.Cell(step.Step, 6));
            Console.Write(NumberFormat.Cell(step.Integral, 10));
            Console.Write(NumberFormat.Cell(step.Difference, 8));
            Console.WriteLine(NumberFormat.Cell(step.Corrected, 10));
        }

        Console.WriteLine();
        Console.WriteLine("Условие выполнено при n = " + result.FinalParts + ".");
        Console.WriteLine("I_" + result.FinalParts + " = " + NumberFormat.Format(result.LastIntegral, 12)
            + "; уточнённое значение I = " + NumberFormat.Format(result.Value, 12) + ".");
        Console.WriteLine();
    }

    private static void PrintGauss(GaussResult result)
    {
        Console.WriteLine("Задание 3. Формула Гаусса–Лежандра, число узлов n = " + result.NodeCount + ".");
        Console.WriteLine("x_i = (a+b)/2 + (b-a)/2 * t_i = "
            + NumberFormat.Format(result.Center, 6) + " + "
            + NumberFormat.Format(result.HalfLength, 6) + " * t_i.");
        Console.WriteLine();

        Console.WriteLine(NumberFormat.Header("i")
            + NumberFormat.Header("C_i")
            + NumberFormat.Header("t_i")
            + NumberFormat.Header("x_i")
            + NumberFormat.Header("f(x_i)")
            + NumberFormat.Header("C_i f(x_i)"));
        Console.WriteLine(new string('-', NumberFormat.CellWidth * 6));

        for (int i = 0; i < result.Nodes.Count; i++)
        {
            var node = result.Nodes[i];
            Console.Write(NumberFormat.Cell(i + 1));
            Console.Write(NumberFormat.Cell(node.Weight, 10));
            Console.Write(NumberFormat.Cell(node.T, 10));
            Console.Write(NumberFormat.Cell(node.X, 10));
            Console.Write(NumberFormat.Cell(node.FunctionValue, 10));
            Console.WriteLine(NumberFormat.Cell(node.Contribution, 10));
        }

        Console.WriteLine();
        Console.WriteLine("Сумма C_i f(x_i) = " + NumberFormat.Format(result.Sum, 12) + ".");
        Console.WriteLine("I = (b-a)/2 * сумма = " + NumberFormat.Format(result.Value, 12) + ".");
        Console.WriteLine();
    }

    private static void PrintComparison(
        double exact, RefinementResult trapezoid, RefinementResult simpson,
        GaussResult gauss4, GaussResult gauss5)
    {
        Console.WriteLine("Задание 4. Сравнение результатов");
        Console.WriteLine("Точное значение I = " + NumberFormat.Format(exact, 12) + ".");
        Console.WriteLine();

        Console.WriteLine(NumberFormat.Header("метод")
            + NumberFormat.Header("n / узлы")
            + NumberFormat.Header("I")
            + NumberFormat.Header("|I-I_exact|")
            + NumberFormat.Header("отн. ошибка, %"));
        Console.WriteLine(new string('-', NumberFormat.CellWidth * 5));

        PrintComparisonRow("Трапеции + Рунге", trapezoid.FinalParts, trapezoid.Value, exact);
        PrintComparisonRow("Симпсон + Рунге", simpson.FinalParts, simpson.Value, exact);
        PrintComparisonRow("Гаусс–Лежандра", gauss4.NodeCount, gauss4.Value, exact);
        PrintComparisonRow("Гаусс–Лежандра", gauss5.NodeCount, gauss5.Value, exact);

        Console.WriteLine();
    }

    private static void PrintComparisonRow(string method, int parts, double value, double exact)
    {
        double error = Math.Abs(value - exact);
        double relative = error / Math.Abs(exact) * 100.0;

        Console.Write(NumberFormat.Header(method));
        Console.Write(NumberFormat.Cell(parts));
        Console.Write(NumberFormat.Cell(value, 12));
        Console.Write(NumberFormat.Scientific(error, 4).PadLeft(NumberFormat.CellWidth));
        Console.WriteLine(NumberFormat.Scientific(relative, 4).PadLeft(NumberFormat.CellWidth));
    }

    private static void PrintConclusion(
        double exact, RefinementResult trapezoid, RefinementResult simpson,
        GaussResult gauss4, GaussResult gauss5)
    {
        double errorTrapezoid = Math.Abs(trapezoid.Value - exact);
        double errorSimpson = Math.Abs(simpson.Value - exact);
        double errorGauss4 = Math.Abs(gauss4.Value - exact);
        double errorGauss5 = Math.Abs(gauss5.Value - exact);

        Console.WriteLine("Выводы:");
        Console.WriteLine("  Трапеции достигли точности при 2n = " + trapezoid.FinalParts
            + " разбиений, Симпсон — при 2n = " + simpson.FinalParts + ".");
        Console.WriteLine("  Отклонение от точного значения:");
        Console.WriteLine("    трапеции (Рунге): " + NumberFormat.Scientific(errorTrapezoid, 4));
        Console.WriteLine("    Симпсон (Рунге):  " + NumberFormat.Scientific(errorSimpson, 4));
        Console.WriteLine("    Гаусс–Лежандра, n = 4: " + NumberFormat.Scientific(errorGauss4, 4));
        Console.WriteLine("    Гаусс–Лежандра, n = 5: " + NumberFormat.Scientific(errorGauss5, 4));
        Console.WriteLine("  Наименьшую погрешность дала формула Гаусса–Лежандра с "
            + (errorGauss5 <= errorGauss4 ? 5 : 4) + " узлами.");
        Console.WriteLine();
    }
}
