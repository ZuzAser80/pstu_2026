using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

// Данные краевой задачи: отрезок [a; b] и шаг сетки h.
internal sealed class InputData
{
    public double Left { get; }
    public double Right { get; }
    public double Step { get; }
    public int Intervals { get; }

    public InputData(double left, double right, double step)
    {
        Left = left;
        Right = right;
        Step = step;
        Intervals = (int)Math.Round((right - left) / step);
    }

    // Формат файла input.txt:
    //   a b h,
    // где [a; b] — отрезок интегрирования, h — шаг сетки (в задании h = 0,05).
    // Пустые строки и всё, что начинается с '#', игнорируются.
    // В качестве десятичного разделителя допускаются и точка, и запятая.
    // Уравнение и краевые условия фиксированы вариантом 4 (см. BoundaryProblem).
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

        if (tokens.Count != 3)
        {
            throw new InputDataException(
                "ожидалось 3 числа (a, b, h), найдено " + tokens.Count);
        }

        int position = 0;
        double left = ReadDouble(tokens, ref position, "a");
        double right = ReadDouble(tokens, ref position, "b");
        double step = ReadDouble(tokens, ref position, "h");

        if (!double.IsFinite(left) || !double.IsFinite(right) || !double.IsFinite(step))
        {
            throw new InputDataException("a, b и h должны быть конечными числами");
        }

        if (right <= left)
        {
            throw new InputDataException("должно выполняться a < b");
        }

        if (step <= 0)
        {
            throw new InputDataException("шаг h должен быть положительным");
        }

        double ratio = (right - left) / step;
        if (Math.Abs(ratio - Math.Round(ratio)) > 1e-8)
        {
            throw new InputDataException("шаг h должен нацело укладываться в отрезок [a; b]");
        }

        if (Math.Round(ratio) < 2)
        {
            throw new InputDataException("число отрезков должно быть не меньше 2");
        }

        return new InputData(left, right, step);
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
