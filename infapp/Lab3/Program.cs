using System;
using System.IO;
using System.Text;

// Лабораторная работа №3. Сплайн-интерполяция.
// Вариант 4, таблица 4: x = 0.180..0.205, X = 0.1838.
//
// Программа строит естественный кубический сплайн дефекта 1 по таблично
// заданной функции. Вторые производные в узлах находятся методом прогонки,
// после чего вычисляются коэффициенты многочленов на каждом отрезке.
//
// Исходные данные обязательно читаются из файла input.txt (см. InputData).
internal static class Program
{
    private const string InputFileName = "input.txt";

    private static int Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("==============================================================");
        Console.WriteLine(" Лабораторная работа №3. Сплайн-интерполяция");
        Console.WriteLine(" Вариант 4 (таблица 4)");
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
        Console.WriteLine();

        PrintInputTable(data);

        CubicSpline spline;
        try
        {
            spline = new CubicSpline(data.X, data.Y);
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidOperationException)
        {
            Console.Error.WriteLine("Ошибка построения сплайна: " + exception.Message);
            return 1;
        }

        PrintTridiagonalSystem(spline);
        PrintThomasSweep(spline);
        PrintSecondDerivatives(spline);
        PrintCoefficients(spline);
        PrintResult(spline, data.TargetX);
        PrintVerification(spline);

        return 0;
    }

    private static void PrintInputTable(InputData data)
    {
        int n = data.X.Length;
        Console.WriteLine("Таблица значений функции:");
        Console.WriteLine(NumberFormat.Header("i")
            + NumberFormat.Header("x_i")
            + NumberFormat.Header("y_i")
            + NumberFormat.Header("h_i"));
        Console.WriteLine(new string('-', NumberFormat.CellWidth * 4));

        for (int i = 0; i < n; i++)
        {
            double h = i < n - 1 ? data.X[i + 1] - data.X[i] : double.NaN;
            Console.Write(NumberFormat.Cell(i + 1));
            Console.Write(NumberFormat.Cell(data.X[i], 6));
            Console.Write(NumberFormat.Cell(data.Y[i], 6));
            Console.WriteLine(i < n - 1
                ? NumberFormat.Cell(h, 6)
                : new string(' ', NumberFormat.CellWidth));
        }

        Console.WriteLine();
    }

    private static void PrintTridiagonalSystem(CubicSpline spline)
    {
        int n = spline.SecondDerivatives.Length;
        Console.WriteLine("Система для вторых производных m_i:");
        Console.WriteLine(NumberFormat.Header("i")
            + NumberFormat.Header("a_i")
            + NumberFormat.Header("b_i")
            + NumberFormat.Header("c_i")
            + NumberFormat.Header("d_i"));
        Console.WriteLine(new string('-', NumberFormat.CellWidth * 5));

        for (int i = 0; i < n; i++)
        {
            Console.Write(NumberFormat.Cell(i + 1));
            Console.Write(NumberFormat.Cell(spline.SystemLower[i], 6));
            Console.Write(NumberFormat.Cell(spline.SystemDiagonal[i], 6));
            Console.Write(NumberFormat.Cell(spline.SystemUpper[i], 6));
            Console.WriteLine(NumberFormat.Cell(spline.SystemRightSide[i], 8));
        }

        Console.WriteLine("Граничные условия: m_1 = 0, m_n = 0.");
        Console.WriteLine();
    }

    private static void PrintThomasSweep(CubicSpline spline)
    {
        int n = spline.SystemSolution.P.Length;
        Console.WriteLine("Прямой ход метода прогонки:");
        Console.WriteLine(NumberFormat.Header("i")
            + NumberFormat.Header("p_i")
            + NumberFormat.Header("q_i"));
        Console.WriteLine(new string('-', NumberFormat.CellWidth * 3));

        for (int i = 0; i < n; i++)
        {
            Console.Write(NumberFormat.Cell(i + 1));
            Console.Write(NumberFormat.Cell(spline.SystemSolution.P[i], 10));
            Console.WriteLine(NumberFormat.Cell(spline.SystemSolution.Q[i], 10));
        }

        Console.WriteLine();
    }

    private static void PrintSecondDerivatives(CubicSpline spline)
    {
        int n = spline.SecondDerivatives.Length;
        Console.WriteLine("Вторые производные в узлах (m_i = S''(x_i)):");
        Console.WriteLine(NumberFormat.Header("i")
            + NumberFormat.Header("x_i")
            + NumberFormat.Header("m_i"));
        Console.WriteLine(new string('-', NumberFormat.CellWidth * 3));

        for (int i = 0; i < n; i++)
        {
            Console.Write(NumberFormat.Cell(i + 1));
            Console.Write(NumberFormat.Cell(spline.Nodes[i], 6));
            Console.WriteLine(NumberFormat.Cell(spline.SecondDerivatives[i], 10));
        }

        Console.WriteLine("Невязка системы: "
            + NumberFormat.Scientific(spline.SystemSolution.MaxResidual, 6) + ".");
        Console.WriteLine();
    }

    private static void PrintCoefficients(CubicSpline spline)
    {
        int segments = spline.A.Length;
        Console.WriteLine("Коэффициенты сплайна на отрезках [x_i; x_{i+1}]:");
        Console.WriteLine(NumberFormat.Header("i")
            + NumberFormat.Header("a_i")
            + NumberFormat.Header("b_i")
            + NumberFormat.Header("c_i")
            + NumberFormat.Header("d_i"));
        Console.WriteLine(new string('-', NumberFormat.CellWidth * 5));

        for (int i = 0; i < segments; i++)
        {
            Console.Write(NumberFormat.Cell(i + 1));
            Console.Write(NumberFormat.Cell(spline.A[i], 8));
            Console.Write(NumberFormat.Cell(spline.B[i], 8));
            Console.Write(NumberFormat.Cell(spline.C[i], 8));
            Console.WriteLine(NumberFormat.Cell(spline.D[i], 8));
        }

        Console.WriteLine();
    }

    private static void PrintResult(CubicSpline spline, double targetX)
    {
        int segment = spline.FindSegment(targetX);
        double result = spline.Evaluate(targetX);

        Console.WriteLine("Результат интерполяции:");
        Console.WriteLine("  X = " + NumberFormat.Format(targetX, 6));
        Console.WriteLine("  X принадлежит отрезку [" + NumberFormat.Format(spline.Nodes[segment], 6)
            + "; " + NumberFormat.Format(spline.Nodes[segment + 1], 6) + "],"
            + " номер отрезка " + (segment + 1) + ".");
        Console.WriteLine("  S(X) = " + NumberFormat.Format(result, 10));
        Console.WriteLine();
    }

    private static void PrintVerification(CubicSpline spline)
    {
        Console.WriteLine("Проверка значений сплайна в узлах:");
        Console.WriteLine(NumberFormat.Header("i")
            + NumberFormat.Header("x_i")
            + NumberFormat.Header("y_i")
            + NumberFormat.Header("S(x_i)")
            + NumberFormat.Header("|S-y|"));
        Console.WriteLine(new string('-', NumberFormat.CellWidth * 5));

        for (int i = 0; i < spline.Nodes.Length; i++)
        {
            double value = spline.Evaluate(spline.Nodes[i]);
            Console.Write(NumberFormat.Cell(i + 1));
            Console.Write(NumberFormat.Cell(spline.Nodes[i], 6));
            Console.Write(NumberFormat.Cell(spline.Values[i], 6));
            Console.Write(NumberFormat.Cell(value, 8));
            Console.WriteLine(NumberFormat.Cell(Math.Abs(value - spline.Values[i]), 3));
        }

        Console.WriteLine();
        Console.WriteLine("Максимальное отклонение в узлах: "
            + NumberFormat.Format(spline.MaxNodeDeviation(), 3) + ".");
    }
}
