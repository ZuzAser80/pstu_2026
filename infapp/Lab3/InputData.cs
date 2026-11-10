using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

// Данные задачи: таблица x_i, y_i и точка X, в которой нужно вычислить сплайн.
internal sealed class InputData
{
    public double[] X { get; }
    public double[] Y { get; }
    public double TargetX { get; }

    public InputData(double[] x, double[] y, double targetX)
    {
        X = x;
        Y = y;
        TargetX = targetX;
    }

    // Формат файла input.txt:
    //   строка 1: n — количество узлов (n >= 2);
    //   строки 2..n+1: пары x_i y_i (x_i строго возрастают);
    //   строка n+2: X — точка интерполяции.
    // Пустые строки и всё, что начинается с '#', игнорируются.
    // В качестве десятичного разделителя допускаются и точка, и запятая.
    public static InputData Read(string path)
    {
        if (!File.Exists(path))
        {
            throw new InputDataException("файл " + path + " не найден");
        }

        var tokens = new List<string>();
        foreach (var rawLine in File.ReadAllLines(path))
        {
            var line = rawLine;
            int comment = line.IndexOf('#');
            if (comment >= 0)
            {
                line = line.Substring(0, comment);
            }

            foreach (var token in line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            {
                tokens.Add(token);
            }
        }

        if (tokens.Count == 0)
        {
            throw new InputDataException("файл " + path + " пуст");
        }

        int position = 0;
        int n = ReadInt(tokens, ref position, "количество узлов n");

        if (n < 2)
        {
            throw new InputDataException("количество узлов n должно быть не меньше 2");
        }

        int expectedCount = 1 + 2 * n + 1;
        if (tokens.Count != expectedCount)
        {
            throw new InputDataException(
                "ожидалось " + expectedCount + " чисел (n, " + n
                + " пар x y и X), найдено " + tokens.Count);
        }

        var x = new double[n];
        var y = new double[n];
        for (int i = 0; i < n; i++)
        {
            x[i] = ReadDouble(tokens, ref position, "x" + (i + 1));
            y[i] = ReadDouble(tokens, ref position, "y" + (i + 1));
        }

        double targetX = ReadDouble(tokens, ref position, "X");

        for (int i = 0; i < n; i++)
        {
            if (!double.IsFinite(x[i]) || !double.IsFinite(y[i]))
            {
                throw new InputDataException("все значения x_i и y_i должны быть конечными числами");
            }
        }

        if (!double.IsFinite(targetX))
        {
            throw new InputDataException("X должно быть конечным числом");
        }

        for (int i = 1; i < n; i++)
        {
            if (x[i] <= x[i - 1])
            {
                throw new InputDataException(
                    "узлы должны строго возрастать: x" + (i + 1) + " <= x" + i);
            }
        }

        if (targetX < x[0] || targetX > x[n - 1])
        {
            throw new InputDataException(
                "X = " + NumberFormat.Format(targetX, 6)
                + " лежит вне отрезка [" + NumberFormat.Format(x[0], 6)
                + "; " + NumberFormat.Format(x[n - 1], 6) + "]");
        }

        return new InputData(x, y, targetX);
    }

    private static int ReadInt(List<string> tokens, ref int position, string name)
    {
        string token = tokens[position];
        if (!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
        {
            throw new InputDataException(
                "не удалось прочитать " + name + " из \"" + token + "\"");
        }

        position++;
        return value;
    }

    private static double ReadDouble(List<string> tokens, ref int position, string name)
    {
        string token = tokens[position];
        string normalized = token.Replace(',', '.');
        if (!double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            throw new InputDataException(
                "не удалось прочитать " + name + " из \"" + token + "\"");
        }

        position++;
        return value;
    }
}

internal sealed class InputDataException : Exception
{
    public InputDataException(string message) : base(message)
    {
    }
}
