using System;
using System.IO;
using System.Text;

// Лабораторная работа №5. Методы решения дифференциальных уравнений.
// Вариант 4: 3*y*y' = 2*x, y(-2) = -3, отрезок [-2; -1].
//
// так как 4 mod 5 = 4, используется метод Рунге-Кутта 4-го порядка
// с шагами h = 0.1 и h = 0.05; погрешность оценивается по правилу Рунге.
//
// Исходные данные обязательно читаются из файла input.txt (см. InputData).
internal static class Program
{
    private const string InputFileName = "input.txt";

    private static int Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("==============================================================");
        Console.WriteLine(" Лабораторная работа №5. Дифференциальные уравнения");
        Console.WriteLine(" Вариант 4: 3*y*y' = 2*x, y(-2) = -3");
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

        OdeSolution coarse;
        OdeSolution fine;
        try
        {
            coarse = RungeKuttaSolver.Solve(data.X0, data.Y0, data.CoarseStep, data.CoarseSteps);
            fine = RungeKuttaSolver.Solve(data.X0, data.Y0, data.FineStep, data.FineSteps);
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidOperationException or OverflowException)
        {
            Console.Error.WriteLine("Ошибка вычисления решения: " + exception.Message);
            return 1;
        }

        PrintStepDetails(coarse, "Первый шаг метода РК4, h = 0.1");
        PrintStepDetails(fine, "Первый шаг метода РК4, h = 0.05");
        PrintSolution(coarse, "Решение на сетке с h = 0.1");
        PrintSolution(fine, "Решение на сетке с h = 0.05");

        var runge = RungeKuttaSolver.Refine(coarse, fine);
        PrintRunge(runge);
        PrintSummary(coarse, fine, runge);

        return 0;
    }

    private static void PrintInputData(InputData data)
    {
        Console.WriteLine();
        Console.WriteLine("Параметры задачи:");
        Console.WriteLine(NumberFormat.Header("x0")
            + NumberFormat.Header("x1")
            + NumberFormat.Header("y0")
            + NumberFormat.Header("h1")
            + NumberFormat.Header("h2"));
        Console.WriteLine(new string('-', NumberFormat.CellWidth * 5));
        Console.WriteLine(NumberFormat.Cell(data.X0, 6)
            + NumberFormat.Cell(data.X1, 6)
            + NumberFormat.Cell(data.Y0, 6)
            + NumberFormat.Cell(data.CoarseStep, 6)
            + NumberFormat.Cell(data.FineStep, 6));
        Console.WriteLine();
        Console.WriteLine("Уравнение: 3*y*y' = 2*x, или y' = f(x,y) = 2*x/(3*y).");
        Console.WriteLine("Точное решение: y(x) = -sqrt((2*x^2 + 19)/3).");
        Console.WriteLine("Число шагов: " + data.CoarseSteps + " при h1 и "
            + data.FineSteps + " при h2.");
        Console.WriteLine();
    }

    private static void PrintStepDetails(OdeSolution solution, string title)
    {
        var first = solution.Steps[1];
        double h = solution.StepSize;
        // Steps[1] хранит конец шага; коэффициенты K вычислялись от его начала.
        double xi = first.X - h;
        double yi = first.Y - first.Delta;

        Console.WriteLine(title);
        Console.WriteLine("  K0 = h f(x_i, y_i),  K1 = h f(x_i+h/2, y_i+K0/2),");
        Console.WriteLine("  K2 = h f(x_i+h/2, y_i+K1/2),  K3 = h f(x_i+h, y_i+K2).");
        Console.WriteLine(NumberFormat.Header("K")
            + NumberFormat.Header("x*")
            + NumberFormat.Header("y*")
            + NumberFormat.Header("значение"));
        Console.WriteLine(new string('-', NumberFormat.CellWidth * 4));
        PrintStepRow("K0", xi, yi, first.K0);
        PrintStepRow("K1", xi + h / 2.0, yi + first.K0 / 2.0, first.K1);
        PrintStepRow("K2", xi + h / 2.0, yi + first.K1 / 2.0, first.K2);
        PrintStepRow("K3", xi + h, yi + first.K2, first.K3);
        Console.WriteLine();
        Console.WriteLine("  Δy = (K0 + 2K1 + 2K2 + K3)/6 = "
            + NumberFormat.Format(first.Delta, 10) + ".");
        Console.WriteLine("  y1 = y0 + Δy = " + NumberFormat.Format(first.Y, 10)
            + "; точное значение " + NumberFormat.Format(first.ExactValue, 10) + ".");
        Console.WriteLine();
    }

    private static void PrintStepRow(string name, double x, double y, double value)
    {
        Console.Write(NumberFormat.Header(name));
        Console.Write(NumberFormat.Cell(x, 6));
        Console.Write(NumberFormat.Cell(y, 10));
        Console.WriteLine(NumberFormat.Cell(value, 10));
    }

    private static void PrintSolution(OdeSolution solution, string title)
    {
        Console.WriteLine(title);
        Console.WriteLine(NumberFormat.Header("i")
            + NumberFormat.Header("x_i")
            + NumberFormat.Header("y_i")
            + NumberFormat.Header("y(x_i)")
            + NumberFormat.Header("|y_i-y(x_i)|"));
        Console.WriteLine(new string('-', NumberFormat.CellWidth * 5));

        for (int i = 0; i < solution.Steps.Count; i++)
        {
            var step = solution.Steps[i];
            Console.Write(NumberFormat.Cell(i));
            Console.Write(NumberFormat.Cell(step.X, 6));
            Console.Write(NumberFormat.Cell(step.Y, 10));
            Console.Write(NumberFormat.Cell(step.ExactValue, 10));
            Console.WriteLine(NumberFormat.Scientific(step.Error, 3).PadLeft(NumberFormat.CellWidth));
        }

        Console.WriteLine();
    }

    private static void PrintRunge(System.Collections.Generic.IReadOnlyList<RungeRow> rows)
    {
        Console.WriteLine("Оценка погрешности по правилу Рунге (k = 4, знаменатель 2^4-1 = 15):");
        Console.WriteLine(NumberFormat.Header("i")
            + NumberFormat.Header("x_i")
            + NumberFormat.Header("y_h")
            + NumberFormat.Header("y_h/2")
            + NumberFormat.Header("y_h/2-y_h")
            + NumberFormat.Header("y_Рунге")
            + NumberFormat.Header("y(x_i)")
            + NumberFormat.Header("ошибка"));
        Console.WriteLine(new string('-', NumberFormat.CellWidth * 8));

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            Console.Write(NumberFormat.Cell(i));
            Console.Write(NumberFormat.Cell(row.X, 6));
            Console.Write(NumberFormat.Cell(row.CoarseValue, 10));
            Console.Write(NumberFormat.Cell(row.FineValue, 10));
            Console.Write(NumberFormat.Scientific(row.Difference, 3).PadLeft(NumberFormat.CellWidth));
            Console.Write(NumberFormat.Cell(row.CorrectedValue, 10));
            Console.Write(NumberFormat.Cell(row.ExactValue, 10));
            Console.WriteLine(NumberFormat.Scientific(row.Error, 3).PadLeft(NumberFormat.CellWidth));
        }

        Console.WriteLine();
    }

    private static void PrintSummary(OdeSolution coarse, OdeSolution fine,
        System.Collections.Generic.IReadOnlyList<RungeRow> rows)
    {
        var lastCoarse = coarse.Steps[coarse.Steps.Count - 1];
        var lastFine = fine.Steps[fine.Steps.Count - 1];
        var lastRow = rows[rows.Count - 1];

        Console.WriteLine("Результат в конце отрезка x = " + NumberFormat.Format(lastCoarse.X, 6) + ":");
        Console.WriteLine("  y_h   (h = 0.1)  = " + NumberFormat.Format(lastCoarse.Y, 12));
        Console.WriteLine("  y_h/2 (h = 0.05) = " + NumberFormat.Format(lastFine.Y, 12));
        Console.WriteLine("  y_Рунге          = " + NumberFormat.Format(lastRow.CorrectedValue, 12));
        Console.WriteLine("  y(-1) точное      = " + NumberFormat.Format(lastRow.ExactValue, 12));
        Console.WriteLine();
        Console.WriteLine("Погрешность при h = 0.1:  "
            + NumberFormat.Scientific(lastCoarse.Error, 4));
        Console.WriteLine("Погрешность при h = 0.05: "
            + NumberFormat.Scientific(lastFine.Error, 4));
        Console.WriteLine("Погрешность после Рунге: "
            + NumberFormat.Scientific(lastRow.Error, 4));
        if (lastFine.Error > 0)
        {
            Console.WriteLine("Отношение ошибок e_h / e_h/2 = "
                + NumberFormat.Format(lastCoarse.Error / lastFine.Error, 2)
                + " (теоретическое для РК4: 2^4 = 16).");
        }

        Console.WriteLine();
    }
}
