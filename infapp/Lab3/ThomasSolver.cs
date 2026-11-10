using System;

// Решение системы линейных уравнений с трёхдиагональной матрицей
// методом прогонки:
//     a_i x_{i-1} + b_i x_i + c_i x_{i+1} = d_i,  i = 1..n,
// причём a_1 = 0 и c_n = 0.
internal sealed class TridiagonalSolution
{
    public double[] Solution { get; }
    public double[] P { get; }
    public double[] Q { get; }
    public double MaxResidual { get; }

    public TridiagonalSolution(double[] solution, double[] p, double[] q, double maxResidual)
    {
        Solution = solution;
        P = p;
        Q = q;
        MaxResidual = maxResidual;
    }
}

internal static class ThomasSolver
{
    public static TridiagonalSolution Solve(
        double[] lower, double[] diagonal, double[] upper, double[] rightSide)
    {
        int n = diagonal.Length;

        if (n == 0)
        {
            throw new ArgumentException("Размер системы должен быть положительным.");
        }

        if (lower.Length != n || upper.Length != n || rightSide.Length != n)
        {
            throw new ArgumentException("Все массивы системы должны иметь одинаковую длину.");
        }

        var p = new double[n];
        var q = new double[n];

        // Прямой ход: прогоночные коэффициенты.
        if (Math.Abs(diagonal[0]) < 1e-15)
        {
            throw new InvalidOperationException("Нулевой ведущий элемент метода прогонки.");
        }

        p[0] = -upper[0] / diagonal[0];
        q[0] = rightSide[0] / diagonal[0];

        for (int i = 1; i < n; i++)
        {
            double denominator = diagonal[i] + lower[i] * p[i - 1];
            if (Math.Abs(denominator) < 1e-15)
            {
                throw new InvalidOperationException(
                    "Нулевой знаменатель метода прогонки на шаге " + (i + 1) + ".");
            }

            p[i] = -upper[i] / denominator;
            q[i] = (rightSide[i] - lower[i] * q[i - 1]) / denominator;
        }

        // Обратный ход.
        var solution = new double[n];
        solution[n - 1] = q[n - 1];
        for (int i = n - 2; i >= 0; i--)
        {
            solution[i] = p[i] * solution[i + 1] + q[i];
        }

        double maxResidual = 0;
        for (int i = 0; i < n; i++)
        {
            double left = diagonal[i] * solution[i];
            if (i > 0)
            {
                left += lower[i] * solution[i - 1];
            }

            if (i < n - 1)
            {
                left += upper[i] * solution[i + 1];
            }

            maxResidual = Math.Max(maxResidual, Math.Abs(left - rightSide[i]));
        }

        return new TridiagonalSolution(solution, p, q, maxResidual);
    }
}
