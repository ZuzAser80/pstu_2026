using System;
using System.Text;

// Метод Зейделя. При вычислении (k+1)-го приближения xi используются уже
// вычисленные (k+1)-ые приближения x1, ..., x(i-1), поэтому неизвестные
// хранятся в одном одномерном массиве. Разность между приближениями
// накапливается в скалярной переменной до перезаписи элемента
static class ZeidelSolver
{
    private const int TransitionDigits = 4;
    private const int SolutionDigits = 8;
    private const int MaxIterations = 100000;

    public static double[]? Solve(LinearSystem system, double epsilon)
    {
        var size = system.Size;
        for (var row = 0; row < size; row++)
        {
            if (Math.Abs(system.GetCoefficient(row, row)) > LinearSystem.Tiny)
            {
                continue;
            }
            Console.WriteLine("Нулевой диагональный элемент a" + (row + 1) + (row + 1)
                + ", метод Зейделя неприменим.");
            return null;
        }

        // Матрица перехода и вектор свободных членов: x = Cx + d
        var matrixC = new double[size, size];
        var vectorD = new double[size];
        for (var row = 0; row < size; row++)
        {
            for (var column = 0; column < size; column++)
            {
                matrixC[row, column] = row == column
                    ? 0
                    : -system.GetCoefficient(row, column) / system.GetCoefficient(row, row);
            }
            vectorD[row] = system.GetRightPart(row) / system.GetCoefficient(row, row);
        }

        Console.WriteLine("Матрица перехода C и вектор свободных членов d:");
        Console.WriteLine(NumberFormat.Block(matrixC, 0, 0, size - 1, TransitionDigits));
        Console.WriteLine("d = " + NumberFormat.Cells(vectorD, TransitionDigits));
        Console.WriteLine();

        // Нулевое приближение принимаем равным вектору свободных членов
        var x = new double[size];
        Array.Copy(vectorD, x, size);

        Console.WriteLine("Итерации метода Зейделя с точностью " + NumberFormat.Format(epsilon, 5) + ":");
        Console.WriteLine(FormatHeader(size));
        Console.WriteLine(FormatIteration(0, x, 0, false));

        for (var iteration = 1; iteration <= MaxIterations; iteration++)
        {
            var maxDelta = 0.0;
            for (var row = 0; row < size; row++)
            {
                var next = vectorD[row];
                for (var column = 0; column < size; column++)
                {
                    if (column == row)
                    {
                        continue;
                    }
                    next += matrixC[row, column] * x[column];
                }
                maxDelta = Math.Max(maxDelta, Math.Abs(next - x[row]));
                x[row] = next;
            }
            Console.WriteLine(FormatIteration(iteration, x, maxDelta, true));
            if (maxDelta > epsilon)
            {
                continue;
            }
            Console.WriteLine("Точность достигнута за " + iteration + " итераций.");
            return x;
        }

        Console.WriteLine("Метод не сошёлся за " + MaxIterations + " итераций.");
        return null;
    }

    private static string FormatHeader(int size)
    {
        var text = new StringBuilder("   k");
        for (var i = 0; i < size; i++)
        {
            text.Append("          x").Append(i + 1);
        }
        return text.Append("     max|dx|").ToString();
    }

    private static string FormatIteration(int iteration, double[] x, double maxDelta, bool hasDelta)
    {
        var text = new StringBuilder(iteration.ToString().PadLeft(4));
        text.Append(' ').Append(NumberFormat.Cells(x, SolutionDigits));
        if (hasDelta)
        {
            text.Append(' ').Append(NumberFormat.Cell(maxDelta, 10));
        }
        return text.ToString();
    }
}
