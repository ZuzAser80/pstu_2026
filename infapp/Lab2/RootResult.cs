// Результат работы одного метода уточнения корня.
// Используется для построения сводной таблицы в конце программы.
class RootResult
{
    public RootResult(string method, string bounds, double x0, double root, int steps)
    {
        Method = method;
        Bounds = bounds;
        X0 = x0;
        Root = root;
        Steps = steps;
    }

    // Название метода
    public string Method { get; }

    // Задание отрезка и начального приближения в виде строки
    public string Bounds { get; }

    // Начальное приближение
    public double X0 { get; }

    // Найденный корень
    public double Root { get; }

    // Количество выполненных шагов
    public int Steps { get; }

    // Невязка: значение функции в найденном корне (проверка подстановкой)
    public double Residual => Equation.Value(Root);
}
