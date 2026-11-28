using System;
using System.IO;
using System.Text;

// Лабораторная работа №6. Краевая задача для ОДУ 2-го порядка, метод прогонки.
// Вариант 4: y'' - x*y' + 2*y = x + 1,
//            y(0,9) - 0,5*y'(0,9) = 2, y(1,2) = 1.
//
// Согласно заданию в программе используется не более трёх одномерных массивов:
//   p, q — прогоночные коэффициенты (переиспользуются для двух сеток),
//   y — решение на мелкой сетке h/2. Решение на грубой сетке сохраняется
// в неиспользуемой при обратном ходе части массива p (элементы p[n..2n]).
//
// Исходные данные обязательно читаются из файла input.txt (см. InputData).
internal static class Program
{
    private const string InputFileName = "input.txt";

    private static int Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("==============================================================");
        Console.WriteLine(" Лабораторная работа №6. Краевая задача, метод прогонки");
        Console.WriteLine(" Вариант 4: y'' - x*y' + 2*y = x + 1");
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

        PrintInputData(data);

        int n = data.Intervals;
        // Ровно три одномерных массива на всю программу: p, q и y.
        // Сначала решается задача на мелкой сетке h/2 — её решение целиком
        // помещается в y (длина 2n+1). Затем переиспользуемые массивы p и q
        // служат для грубой сетки; её значения сохраняются в p[n..2n] —
        // эти элементы не участвуют в обратном ходе грубой сетки.
        double[] p = new double[2 * n + 1];
        double[] q = new double[2 * n + 1];
        double[] y = new double[2 * n + 1];

        try
        {
            ThomasSolver.Solve(data.Left, data.Step / 2.0, 2 * n, p, q, y, null);
            ThomasSolver.Solve(data.Left, data.Step, n, p, q, null,
                (index, value) => p[n + index] = value);
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidOperationException or OverflowException)
        {
            Console.Error.WriteLine("Ошибка решения системы: " + exception.Message);
            return 1;
        }

        PrintCoefficients(data);
        PrintSweepCoefficients(data.Left, n, data.Step, p, q);
        PrintSolution(data, p, n);
        PrintChecks(data, p, n);
        PrintComparison(data, p, y, n);

        return 0;
    }

    private static void PrintInputData(InputData data)
    {
        Console.WriteLine("Исходные данные прочитаны из " + InputFileName + ".");
        Console.WriteLine();
        Console.WriteLine("Параметры задачи:");
        Console.WriteLine(NumberFormat.Header("a")
            + NumberFormat.Header("b")
            + NumberFormat.Header("h")
            + NumberFormat.Header("n"));
        Console.WriteLine(new string('-', NumberFormat.CellWidth * 4));
        Console.WriteLine(NumberFormat.Cell(data.Left, 6)
            + NumberFormat.Cell(data.Right, 6)
            + NumberFormat.Cell(data.Step, 6)
            + NumberFormat.Cell(data.Intervals));
        Console.WriteLine();
        Console.WriteLine("Уравнение: y'' - x*y' + 2*y = x + 1.");
        Console.WriteLine("Краевые условия: y(0.9) - 0.5*y'(0.9) = 2, y(1.2) = 1.");
        Console.WriteLine("Однородная сетка: x_i = a + i*h, i = 0.." + data.Intervals + ".");
        Console.WriteLine("Используется не более трёх одномерных массивов: p, q, y.");
        Console.WriteLine();
    }

    private static void PrintCoefficients(InputData data)
    {
        Console.WriteLine("Коэффициенты разностного уравнения a_i*y_{i-1} - b_i*y_i + y_{i+1} = d_i:");
        Console.WriteLine(NumberFormat.Header("i")
            + NumberFormat.Header("x_i")
            + NumberFormat.Header("a_i")
            + NumberFormat.Header("b_i")
            + NumberFormat.Header("d_i"));
        Console.WriteLine(new string('-', NumberFormat.CellWidth * 5));

        for (int i = 1; i < data.Intervals; i++)
        {
            var row = ThomasSolver.CoefficientsAt(data.Left + i * data.Step, data.Step);
            Console.Write(NumberFormat.Cell(i));
            Console.Write(NumberFormat.Cell(row.X, 6));
            Console.Write(NumberFormat.Cell(row.Lower, 8));
            Console.Write(NumberFormat.Cell(row.Main, 8));
            Console.WriteLine(NumberFormat.Cell(row.RightSide, 8));
        }

        Console.WriteLine();
    }

    private static void PrintSweepCoefficients(double left, int n, double h,
        double[] p, double[] q)
    {
        Console.WriteLine("Прямой ход: прогоночные коэффициенты y_i = p_i*y_{i+1} + q_i:");
        Console.WriteLine(NumberFormat.Header("i")
            + NumberFormat.Header("x_i")
            + NumberFormat.Header("p_i")
            + NumberFormat.Header("q_i"));
        Console.WriteLine(new string('-', NumberFormat.CellWidth * 4));

        for (int i = 0; i < n; i++)
        {
            Console.Write(NumberFormat.Cell(i));
            Console.Write(NumberFormat.Cell(left + i * h, 6));
            Console.Write(NumberFormat.Cell(p[i], 8));
            Console.WriteLine(NumberFormat.Cell(q[i], 8));
        }

        Console.WriteLine();
    }

    private static void PrintSolution(InputData data, double[] coarse, int n)
    {
        Console.WriteLine("Обратный ход: значения решения в узлах сетки:");
        Console.WriteLine(NumberFormat.Header("i")
            + NumberFormat.Header("x_i")
            + NumberFormat.Header("y_i"));
        Console.WriteLine(new string('-', NumberFormat.CellWidth * 3));

        double h = data.Step;
        for (int i = 0; i <= n; i++)
        {
            Console.Write(NumberFormat.Cell(i));
            Console.Write(NumberFormat.Cell(data.Left + i * h, 6));
            Console.WriteLine(NumberFormat.Cell(coarse[n + i], 10));
        }

        Console.WriteLine();
    }

    private static void PrintChecks(InputData data, double[] coarse, int n)
    {
        double h = data.Step;

        double leftResidual = BoundaryProblem.Alpha0 * coarse[n]
            + BoundaryProblem.Alpha1 * (coarse[n + 1] - coarse[n]) / h
            - BoundaryProblem.LeftValue;
        double rightResidual = BoundaryProblem.Beta0 * coarse[2 * n]
            + BoundaryProblem.Beta1 * (coarse[2 * n] - coarse[2 * n - 1]) / h
            - BoundaryProblem.RightValue;

        double maxResidual = 0;
        for (int i = 1; i < n; i++)
        {
            var row = ThomasSolver.CoefficientsAt(data.Left + i * h, h);
            double residual = row.Lower * coarse[n + i - 1] - row.Main * coarse[n + i]
                + coarse[n + i + 1] - row.RightSide;
            maxResidual = Math.Max(maxResidual, Math.Abs(residual));
        }

        Console.WriteLine("Проверка точности:");
        Console.WriteLine("  невязка левого краевого условия:  "
            + NumberFormat.Scientific(leftResidual, 3));
        Console.WriteLine("  невязка правого краевого условия: "
            + NumberFormat.Scientific(rightResidual, 3));
        Console.WriteLine("  максимальная невязка разностного уравнения: "
            + NumberFormat.Scientific(maxResidual, 3));
        Console.WriteLine();
    }

    private static void PrintComparison(InputData data, double[] coarse,
        double[] fine, int n)
    {
        Console.WriteLine("Сравнение решений на сетках h и h/2 в общих узлах:");
        Console.WriteLine(NumberFormat.Header("i")
            + NumberFormat.Header("x_i")
            + NumberFormat.Header("y_h")
            + NumberFormat.Header("y_h/2")
            + NumberFormat.Header("y_h/2 - y_h"));
        Console.WriteLine(new string('-', NumberFormat.CellWidth * 5));

        double maxDifference = 0;
        for (int i = 0; i <= n; i++)
        {
            double coarseValue = coarse[n + i];
            double fineValue = fine[2 * i];
            double difference = fineValue - coarseValue;
            maxDifference = Math.Max(maxDifference, Math.Abs(difference));
            Console.Write(NumberFormat.Cell(i));
            Console.Write(NumberFormat.Cell(data.Left + i * data.Step, 6));
            Console.Write(NumberFormat.Cell(coarseValue, 10));
            Console.Write(NumberFormat.Cell(fineValue, 10));
            Console.WriteLine(NumberFormat.Scientific(difference, 3).PadLeft(NumberFormat.CellWidth));
        }

        Console.WriteLine();
        Console.WriteLine("Максимальное отличие решений: "
            + NumberFormat.Scientific(maxDifference, 4) + ".");
        Console.WriteLine("Порядок аппроксимации внутри отрезка O(h^2), краевые условия O(h),");
        Console.WriteLine("поэтому при вдвое меньшем шаге решение уточняется примерно вдвое.");
        Console.WriteLine();
    }
}
