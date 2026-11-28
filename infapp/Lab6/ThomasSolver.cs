using System;

// Коэффициенты разностного уравнения (2.8) во внутреннем узле x:
//     a_i y_{i-1} - b_i y_i + y_{i+1} = d_i,
// где a_i = (1 - 0.5 p h) / (1 + 0.5 p h),
//     b_i = (2 - q h^2) / (1 + 0.5 p h),
//     d_i = h^2 f / (1 + 0.5 p h).
internal readonly struct DifferenceRow
{
    public double X { get; }
    public double Lower { get; }
    public double Main { get; }
    public double RightSide { get; }

    public DifferenceRow(double x, double lower, double main, double rightSide)
    {
        X = x;
        Lower = lower;
        Main = main;
        RightSide = rightSide;
    }
}

// Метод прогонки для системы (2.11).
//
// Ограничение задания: не более трёх одномерных массивов. Их роли:
//   p, q  — прогоночные коэффициенты (передаются вызывающей стороной);
//   y     — решение (необязательный массив; если он не передан, значения
//           решения передаются через callback и третий массив не выделяется).
// Коэффициенты a_i, b_i, d_i вычисляются скалярами по мере обхода сетки.
internal static class ThomasSolver
{
    public static DifferenceRow CoefficientsAt(double x, double h)
    {
        double p = BoundaryProblem.P(x);
        double q = BoundaryProblem.Q(x);
        double f = BoundaryProblem.F(x);
        double denominator = 1.0 + 0.5 * p * h;
        if (Math.Abs(denominator) < 1e-15)
        {
            throw new InvalidOperationException(
                "знаменатель разностных коэффициентов обратился в нуль при x = " + x + ".");
        }

        return new DifferenceRow(
            x,
            (1.0 - 0.5 * p * h) / denominator,
            (2.0 - q * h * h) / denominator,
            h * h * f / denominator);
    }

    // Прямой и обратный ход. Массивы p и q должны иметь длину n + 1.
    // Если solution задан, решение сохраняется в нём; иначе для каждого
    // узла вызывается report(i, y_i) (обход идёт справа налево).
    public static void Solve(
        double left, double h, int n,
        double[] p, double[] q, double[]? solution,
        Action<int, double>? report)
    {
        if (n < 2)
        {
            throw new ArgumentException("число отрезков должно быть не меньше 2.", nameof(n));
        }

        if (p.Length < n + 1 || q.Length < n + 1)
        {
            throw new ArgumentException(
                "массивы прогоночных коэффициентов должны иметь длину не менее n + 1.");
        }

        if (solution != null && solution.Length != n + 1)
        {
            throw new ArgumentException("массив решения должен иметь длину n + 1.", nameof(solution));
        }

        // Левое краевое условие: (alpha_0 - alpha_1/h) y_0 + (alpha_1/h) y_1 = A.
        double leftDenominator = BoundaryProblem.Alpha0 * h - BoundaryProblem.Alpha1;
        if (Math.Abs(leftDenominator) < 1e-15)
        {
            throw new InvalidOperationException("левое краевое условие вырождено.");
        }

        p[0] = -BoundaryProblem.Alpha1 / leftDenominator;
        q[0] = BoundaryProblem.LeftValue * h / leftDenominator;

        // Прямой ход.
        for (int i = 1; i < n; i++)
        {
            var row = CoefficientsAt(left + i * h, h);
            double denominator = row.Lower * p[i - 1] - row.Main;
            if (Math.Abs(denominator) < 1e-15)
            {
                throw new InvalidOperationException(
                    "нулевой знаменатель прогонки в узле " + i + ".");
            }

            p[i] = -1.0 / denominator;
            q[i] = (row.RightSide - row.Lower * q[i - 1]) / denominator;
        }

        // Обратный ход: сначала y_n из правого краевого условия.
        double rightDenominator = BoundaryProblem.Beta0 * h
            + BoundaryProblem.Beta1 * (1.0 - p[n - 1]);
        if (Math.Abs(rightDenominator) < 1e-15)
        {
            throw new InvalidOperationException("правое краевое условие вырождено.");
        }

        double next = (BoundaryProblem.RightValue * h + BoundaryProblem.Beta1 * q[n - 1])
            / rightDenominator;

        if (solution != null)
        {
            solution[n] = next;
        }

        report?.Invoke(n, next);

        for (int i = n - 1; i >= 0; i--)
        {
            next = p[i] * next + q[i];
            if (solution != null)
            {
                solution[i] = next;
            }

            report?.Invoke(i, next);
        }
    }
}
