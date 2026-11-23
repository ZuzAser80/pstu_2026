using System;
using System.Collections.Generic;

// Правая часть дифференциального уравнения варианта 4:
//     3 y y' = 2 x   =>   y' = f(x, y) = 2 x / (3 y),   y(-2) = -3.
// Точное решение получается разделением переменных:
//     (3/2) y^2 = x^2 + C,  C = 19/2,  y = -sqrt((2 x^2 + 19) / 3)
// (отрицательная ветвь выбрана по начальному условию y(-2) = -3).
internal static class OdeProblem
{
    public static double RightHandSide(double x, double y)
    {
        if (y == 0.0)
        {
            throw new InvalidOperationException(
                "правая часть f(x, y) = 2x / (3y) не определена при y = 0");
        }

        return 2.0 * x / (3.0 * y);
    }

    public static double ExactSolution(double x)
    {
        return -Math.Sqrt((2.0 * x * x + 19.0) / 3.0);
    }
}

// Одна точка сетки вместе с коэффициентами РК4, которые привели в неё.
internal sealed class IntegrationStep
{
    public double X { get; }
    public double Y { get; }
    public double K0 { get; }
    public double K1 { get; }
    public double K2 { get; }
    public double K3 { get; }
    public double Delta { get; }
    public double ExactValue { get; }
    public double Error { get; }

    public IntegrationStep(double x, double y, double k0, double k1, double k2,
        double k3, double delta)
    {
        X = x;
        Y = y;
        K0 = k0;
        K1 = k1;
        K2 = k2;
        K3 = k3;
        Delta = delta;
        ExactValue = OdeProblem.ExactSolution(x);
        Error = Math.Abs(y - ExactValue);
    }
}

// Решение на равномерной сетке с шагом h; Steps[0] — начальная точка.
internal sealed class OdeSolution
{
    public double StepSize { get; }
    public IReadOnlyList<IntegrationStep> Steps { get; }

    public OdeSolution(double stepSize, IReadOnlyList<IntegrationStep> steps)
    {
        StepSize = stepSize;
        Steps = steps;
    }
}

// Строка сравнения двух сеток: грубая h, мелкая h/2 и уточнение по Рунге.
internal sealed class RungeRow
{
    public double X { get; }
    public double CoarseValue { get; }
    public double FineValue { get; }
    public double Difference { get; }
    public double CorrectedValue { get; }
    public double ExactValue { get; }
    public double Error { get; }

    public RungeRow(double x, double coarseValue, double fineValue)
    {
        X = x;
        CoarseValue = coarseValue;
        FineValue = fineValue;
        Difference = fineValue - coarseValue;
        // k = 4 для метода РК4: 2^k - 1 = 15.
        CorrectedValue = fineValue + Difference / 15.0;
        ExactValue = OdeProblem.ExactSolution(x);
        Error = Math.Abs(CorrectedValue - ExactValue);
    }
}

// Метод Рунге-Кутта четвёртого порядка и оценка погрешности по правилу Рунге.
internal static class RungeKuttaSolver
{
    public static OdeSolution Solve(double x0, double y0, double h, int stepCount)
    {
        if (h <= 0 || !double.IsFinite(h))
        {
            throw new ArgumentException("шаг h должен быть положительным конечным числом.", nameof(h));
        }

        if (stepCount < 1)
        {
            throw new ArgumentException("число шагов должно быть положительным.", nameof(stepCount));
        }

        var steps = new List<IntegrationStep>(stepCount + 1)
        {
            new IntegrationStep(x0, y0, 0, 0, 0, 0, 0),
        };

        double x = x0;
        double y = y0;
        for (int i = 0; i < stepCount; i++)
        {
            double k0 = h * OdeProblem.RightHandSide(x, y);
            double k1 = h * OdeProblem.RightHandSide(x + h / 2.0, y + k0 / 2.0);
            double k2 = h * OdeProblem.RightHandSide(x + h / 2.0, y + k1 / 2.0);
            double k3 = h * OdeProblem.RightHandSide(x + h, y + k2);
            double delta = (k0 + 2.0 * k1 + 2.0 * k2 + k3) / 6.0;

            if (!double.IsFinite(delta))
            {
                throw new InvalidOperationException(
                    "вычисление правой части дало неконечное значение на шаге " + (i + 1) + ".");
            }

            y += delta;
            x = x0 + (i + 1) * h;
            steps.Add(new IntegrationStep(x, y, k0, k1, k2, k3, delta));
        }

        return new OdeSolution(h, steps);
    }

    // Сравнение решений с шагом h и h/2 в общих узлах сетки.
    public static IReadOnlyList<RungeRow> Refine(OdeSolution coarse, OdeSolution fine)
    {
        if (Math.Abs(fine.StepSize - coarse.StepSize / 2.0) > 1e-12 * coarse.StepSize)
        {
            throw new ArgumentException("мелкий шаг должен быть равен половине грубого.");
        }

        int coarseCount = coarse.Steps.Count;
        if (fine.Steps.Count != 2 * coarseCount - 1)
        {
            throw new ArgumentException("сетки не согласованы: число узлов должно отличаться вдвое.");
        }

        var rows = new List<RungeRow>(coarseCount);
        for (int i = 0; i < coarseCount; i++)
        {
            var coarseStep = coarse.Steps[i];
            var fineStep = fine.Steps[2 * i];
            if (Math.Abs(coarseStep.X - fineStep.X) > 1e-10)
            {
                throw new InvalidOperationException("узлы грубой и мелкой сеток не совпадают.");
            }

            rows.Add(new RungeRow(coarseStep.X, coarseStep.Y, fineStep.Y));
        }

        return rows;
    }
}
