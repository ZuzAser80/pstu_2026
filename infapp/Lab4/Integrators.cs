using System;
using System.Collections.Generic;

// Подынтегральная функция варианта 4 и её точный первообразный.
//
//     f(x) = x^2 / (x^2 + 1),       F(x) = x - arctg(x),
//
// поэтому точное значение интеграла равно F(b) - F(a).
internal static class Integrand
{
    public static double Evaluate(double x)
    {
        double square = x * x;
        return square / (square + 1.0);
    }

    public static double ExactIntegral(double a, double b)
    {
        return (b - Math.Atan(b)) - (a - Math.Atan(a));
    }
}

// Один шаг удвоения числа parts разбиения.
// Integral — приближение на более грубой сетке n,
// Difference — модуль разности грубого и уточнённого Approximations,
// Corrected — уточнённое по Рунге значение.
internal sealed class RefinementStep
{
    public int Parts { get; }
    public double Step { get; }
    public double Integral { get; }
    public double Difference { get; }
    public double Corrected { get; }

    public RefinementStep(int parts, double step, double integral, double difference, double corrected)
    {
        Parts = parts;
        Step = step;
        Integral = integral;
        Difference = difference;
        Corrected = corrected;
    }
}

internal sealed class RefinementResult
{
    public string MethodName { get; }
    public int Order { get; }
    public IReadOnlyList<RefinementStep> Steps { get; }
    public int FinalParts { get; }
    public double LastIntegral { get; }
    public double Value { get; }

    public RefinementResult(
        string methodName, int order, IReadOnlyList<RefinementStep> steps,
        int finalParts, double lastIntegral, double value)
    {
        MethodName = methodName;
        Order = order;
        Steps = steps;
        FinalParts = finalParts;
        LastIntegral = lastIntegral;
        Value = value;
    }
}

// Формулы трапеций и Симпсона с автоматическим удвоением числа разбиений
// и уточнением результата по правилу Рунге.
internal static class Quadrature
{
    private const int MaxDoublings = 30;

    public static RefinementResult Trapezoid(double a, double b, double epsilon, int initialParts)
    {
        return Refine("Формула трапеций", 2, a, b, epsilon, initialParts, TrapezoidValue);
    }

    public static RefinementResult Simpson(double a, double b, double epsilon, int initialParts)
    {
        return Refine("Формула Симпсона", 4, a, b, epsilon, initialParts, SimpsonValue);
    }

    private static RefinementResult Refine(
        string methodName, int order, double a, double b, double epsilon,
        int initialParts, Func<double, double, int, double> rule)
    {
        var steps = new List<RefinementStep>();
        double rungeDivisor = Math.Pow(2.0, order) - 1.0;

        int parts = initialParts;
        double coarse = rule(a, b, parts);

        for (int iteration = 0; iteration < MaxDoublings; iteration++)
        {
            int fineParts = checked(parts * 2);
            double fine = rule(a, b, fineParts);
            double difference = Math.Abs(fine - coarse);
            double corrected = fine + (fine - coarse) / rungeDivisor;

            steps.Add(new RefinementStep(
                parts, (b - a) / parts, coarse, difference, corrected));

            if (difference < epsilon)
            {
                return new RefinementResult(
                    methodName, order, steps, fineParts, fine, corrected);
            }

            parts = fineParts;
            coarse = fine;
        }

        throw new InvalidOperationException(
            "За " + MaxDoublings + " удвоений не достигнута точность " + epsilon + ".");
    }

    // h * (1/2 f_0 + f_1 + ... + f_{n-1} + 1/2 f_n).
    private static double TrapezoidValue(double a, double b, int n)
    {
        if (n < 1)
        {
            throw new ArgumentException("Число разбиений должно быть положительным.");
        }

        double step = (b - a) / n;
        double sum = 0.5 * (Integrand.Evaluate(a) + Integrand.Evaluate(b));
        for (int i = 1; i < n; i++)
        {
            sum += Integrand.Evaluate(a + i * step);
        }

        return step * sum;
    }

    // h/3 * (f_0 + f_n + 2 * чётные + 4 * нечётные), n обязательно чётно.
    private static double SimpsonValue(double a, double b, int n)
    {
        if (n < 2 || n % 2 != 0)
        {
            throw new ArgumentException("Формула Симпсона требует чётного n >= 2.");
        }

        double step = (b - a) / n;
        double sum = Integrand.Evaluate(a) + Integrand.Evaluate(b);
        for (int i = 1; i < n; i++)
        {
            sum += (i % 2 == 0 ? 2.0 : 4.0) * Integrand.Evaluate(a + i * step);
        }

        return step * sum / 3.0;
    }
}

// Одна строка таблицы квадратуры Гаусса–Лежандра.
internal sealed class GaussNode
{
    public double Weight { get; }
    public double T { get; }
    public double X { get; }
    public double FunctionValue { get; }
    public double Contribution { get; }

    public GaussNode(double weight, double t, double x, double functionValue, double contribution)
    {
        Weight = weight;
        T = t;
        X = x;
        FunctionValue = functionValue;
        Contribution = contribution;
    }
}

internal sealed class GaussResult
{
    public int NodeCount { get; }
    public double Center { get; }
    public double HalfLength { get; }
    public IReadOnlyList<GaussNode> Nodes { get; }
    public double Sum { get; }
    public double Value { get; }

    public GaussResult(
        int nodeCount, double center, double halfLength,
        IReadOnlyList<GaussNode> nodes, double sum, double value)
    {
        NodeCount = nodeCount;
        Center = center;
        HalfLength = halfLength;
        Nodes = nodes;
        Sum = sum;
        Value = value;
    }
}

// Обычная квадратура Гаусса–Лежандра (вес p(x) = 1) с 4 и 5 узлами
// на отрезке [a, b]. Узлы t_i — нули многочлена Лежандра на [-1, 1].
internal static class GaussQuadrature
{
    private static readonly double[] Nodes4 =
    {
        -0.8611363116, -0.3399810436, 0.3399810436, 0.8611363116,
    };

    private static readonly double[] Weights4 =
    {
        0.3478548451, 0.6521451549, 0.6521451549, 0.3478548451,
    };

    private static readonly double[] Nodes5 =
    {
        -0.9061798459, -0.5384693101, 0.0, 0.5384693101, 0.9061798459,
    };

    private static readonly double[] Weights5 =
    {
        0.2369268851, 0.4786286705, 0.5688888889, 0.4786286705, 0.2369268851,
    };

    public static GaussResult Integrate(double a, double b, int nodeCount)
    {
        double[] nodes;
        double[] weights;
        switch (nodeCount)
        {
            case 4:
                nodes = Nodes4;
                weights = Weights4;
                break;
            case 5:
                nodes = Nodes5;
                weights = Weights5;
                break;
            default:
                throw new ArgumentException("Поддерживаются квадратуры Гаусса–Лежандра с 4 или 5 узлами.");
        }

        double center = (a + b) / 2.0;
        double halfLength = (b - a) / 2.0;
        var table = new List<GaussNode>();
        double sum = 0.0;

        for (int i = 0; i < nodes.Length; i++)
        {
            double x = center + halfLength * nodes[i];
            double value = Integrand.Evaluate(x);
            double contribution = weights[i] * value;
            sum += contribution;
            table.Add(new GaussNode(weights[i], nodes[i], x, value, contribution));
        }

        return new GaussResult(nodeCount, center, halfLength, table, sum, halfLength * sum);
    }
}
