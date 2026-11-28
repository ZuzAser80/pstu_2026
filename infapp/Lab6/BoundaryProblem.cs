using System;

// Краевая задача варианта 4:
//     y'' - x y' + 2 y = x + 1,   y(0,9) - 0,5 y'(0,9) = 2,   y(1,2) = 1.
//
// В обозначениях методички (2.1)-(2.3):
//     y'' + p(x) y' + q(x) y = f(x),
//     alpha_0 y(a) + alpha_1 y'(a) = A,
//     beta_0  y(b) + beta_1  y'(b) = B.
internal static class BoundaryProblem
{
    public static double P(double x)
    {
        return -x;
    }

    public static double Q(double x)
    {
        return 2.0;
    }

    public static double F(double x)
    {
        return x + 1.0;
    }

    public const double Alpha0 = 1.0;
    public const double Alpha1 = -0.5;
    public const double LeftValue = 2.0;

    public const double Beta0 = 1.0;
    public const double Beta1 = 0.0;
    public const double RightValue = 1.0;
}
