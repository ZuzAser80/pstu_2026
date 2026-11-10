using System;

// Кубический сплайн дефекта 1, построенный по значениям второй производной
// в узлах (раздел 2 методических указаний).
//
// На отрезке [x_i, x_{i+1}]:
//     S_i(x) = a_i + b_i (x - x_i) + c_i (x - x_i)^2 + d_i (x - x_i)^3,
// где
//     a_i = y_i,  c_i = m_i / 2,  d_i = (m_{i+1} - m_i) / (6 h_i),
//     b_i = (y_{i+1} - y_i) / h_i - h_i (2 m_i + m_{i+1}) / 6,
// а m_i = S''(x_i) находятся из трёхдиагональной системы (2.7)
// с естественными граничными условиями m_1 = m_n = 0.
internal sealed class CubicSpline
{
    public double[] Nodes { get; }
    public double[] Values { get; }
    public double[] H { get; }
    public double[] SecondDerivatives { get; }
    public double[] A { get; }
    public double[] B { get; }
    public double[] C { get; }
    public double[] D { get; }
    public double[] SystemLower { get; }
    public double[] SystemDiagonal { get; }
    public double[] SystemUpper { get; }
    public double[] SystemRightSide { get; }
    public TridiagonalSolution SystemSolution { get; }

    public CubicSpline(double[] x, double[] y)
    {
        if (x.Length != y.Length)
        {
            throw new ArgumentException("Массивы x и y должны иметь одинаковую длину.");
        }

        if (x.Length < 2)
        {
            throw new ArgumentException("Для построения сплайна нужно не менее двух узлов.");
        }

        Nodes = (double[])x.Clone();
        Values = (double[])y.Clone();

        int n = Nodes.Length;
        H = new double[n - 1];
        for (int i = 0; i < n - 1; i++)
        {
            H[i] = Nodes[i + 1] - Nodes[i];
            if (H[i] <= 0)
            {
                throw new ArgumentException("Узлы должны быть строго возрастающими.");
            }
        }

        // Система для вторых производных m_i:
        //   m_1 = 0,
        //   h_{i-1} m_{i-1} + 2 (h_{i-1} + h_i) m_i + h_i m_{i+1}
        //       = 6 ((y_{i+1} - y_i) / h_i - (y_i - y_{i-1}) / h_{i-1}),
        //   m_n = 0.
        var lower = new double[n];
        var diagonal = new double[n];
        var upper = new double[n];
        var rightSide = new double[n];

        diagonal[0] = 1;
        rightSide[0] = 0;

        for (int i = 1; i < n - 1; i++)
        {
            lower[i] = H[i - 1];
            diagonal[i] = 2 * (H[i - 1] + H[i]);
            upper[i] = H[i];
            rightSide[i] = 6 * ((Values[i + 1] - Values[i]) / H[i]
                - (Values[i] - Values[i - 1]) / H[i - 1]);
        }

        diagonal[n - 1] = 1;
        rightSide[n - 1] = 0;

        SystemLower = lower;
        SystemDiagonal = diagonal;
        SystemUpper = upper;
        SystemRightSide = rightSide;

        SystemSolution = ThomasSolver.Solve(lower, diagonal, upper, rightSide);
        SecondDerivatives = SystemSolution.Solution;

        A = new double[n - 1];
        B = new double[n - 1];
        C = new double[n - 1];
        D = new double[n - 1];

        for (int i = 0; i < n - 1; i++)
        {
            A[i] = Values[i];
            C[i] = SecondDerivatives[i] / 2;
            D[i] = (SecondDerivatives[i + 1] - SecondDerivatives[i]) / (6 * H[i]);
            B[i] = (Values[i + 1] - Values[i]) / H[i]
                - H[i] * (2 * SecondDerivatives[i] + SecondDerivatives[i + 1]) / 6;
        }
    }

    // Номер отрезка [x_i, x_{i+1}], содержащего точку x.
    public int FindSegment(double x)
    {
        int n = Nodes.Length;
        if (!double.IsFinite(x) || x < Nodes[0] || x > Nodes[n - 1])
        {
            throw new ArgumentOutOfRangeException(nameof(x),
                "Точка интерполяции лежит вне отрезка [" + Nodes[0] + "; " + Nodes[n - 1] + "].");
        }

        int left = 0;
        int right = n - 1;
        while (right - left > 1)
        {
            int middle = (left + right) / 2;
            if (Nodes[middle] <= x)
            {
                left = middle;
            }
            else
            {
                right = middle;
            }
        }

        return left;
    }

    // Значение сплайна в точке x (схема Горнера).
    public double Evaluate(double x)
    {
        int i = FindSegment(x);
        double dx = x - Nodes[i];
        return A[i] + dx * (B[i] + dx * (C[i] + dx * D[i]));
    }

    // Максимальное отклонение сплайна от табличных значений в узлах.
    public double MaxNodeDeviation()
    {
        double maximum = 0;
        for (int i = 0; i < Nodes.Length; i++)
        {
            maximum = Math.Max(maximum, Math.Abs(Evaluate(Nodes[i]) - Values[i]));
        }

        return maximum;
    }
}
