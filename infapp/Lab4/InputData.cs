using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

// Данные задачи: пределы интегрирования, требуемая точность epsilon
// и начальное количество частей разбиения n0.
internal sealed class InputData
{
    public double A { get; }
    public double B { get; }
    public double Epsilon { get; }
    public int InitialParts { get; }

    public InputData(double a, double b, double epsilon, int initialParts)
    {
        A = a;
        B = b;
        Epsilon = epsilon;
        InitialParts = initialParts;
    }

    // Формат файла input.txt:
    //   строка 1: a b epsilon n0,
    // где a < b — пределы интегрирования, epsilon > 0 — требуемая точность,
    // n0 >= 2 — начальное число частей разбиения (далее оно удваивается).
    // Пустые строки и всё, что начинается с '#', игнорируются.
    // В качестве десятичного разделителя допускаются и точка, и запятая.
    // Подынтегральная функция фиксирована — это вариант 4:
    //   f(x) = x^2 / (x^2 + 1).
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

        if (tokens.Count != 4)
        {
            throw new InputDataException(
                "ожидалось 4 числа (a, b, epsilon, n0), найдено " + tokens.Count);
        }

        int position = 0;
        double a = ReadDouble(tokens, ref position, "a");
        double b = ReadDouble(tokens, ref position, "b");
        double epsilon = ReadDouble(tokens, ref position, "epsilon");
        int initialParts = ReadInt(tokens, ref position, "n0");

        if (!double.IsFinite(a) || !double.IsFinite(b) || !double.IsFinite(epsilon))
        {
            throw new InputDataException("a, b и epsilon должны быть конечными числами");
        }

        if (a >= b)
        {
            throw new InputDataException("должно выполняться a < b");
        }

        if (epsilon <= 0)
        {
            throw new InputDataException("epsilon должно быть положительным числом");
        }

        if (initialParts < 2)
        {
            throw new InputDataException("начальное число частей n0 должно быть не меньше 2");
        }

        return new InputData(a, b, epsilon, initialParts);
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
