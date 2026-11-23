using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

// Данные задачи Коши: отрезок [x0; x1], начальное значение y0
// и два шага интегрирования — грубый h1 и вдвое меньший h2.
internal sealed class InputData
{
    public double X0 { get; }
    public double X1 { get; }
    public double Y0 { get; }
    public double CoarseStep { get; }
    public double FineStep { get; }
    public int CoarseSteps { get; }
    public int FineSteps { get; }

    public InputData(double x0, double x1, double y0, double coarseStep, double fineStep)
    {
        X0 = x0;
        X1 = x1;
        Y0 = y0;
        CoarseStep = coarseStep;
        FineStep = fineStep;
        CoarseSteps = (int)Math.Round((x1 - x0) / coarseStep);
        FineSteps = (int)Math.Round((x1 - x0) / fineStep);
    }

    // Формат файла input.txt:
    //   x0 x1 y0 h1 h2,
    // где [x0; x1] — отрезок интегрирования, y0 = y(x0),
    // h1 — шаг грубой сетки, h2 = h1 / 2 — шаг мелкой сетки (для правила Рунге).
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

        if (tokens.Count != 5)
        {
            throw new InputDataException(
                "ожидалось 5 чисел (x0, x1, y0, h1, h2), найдено " + tokens.Count);
        }

        int position = 0;
        double x0 = ReadDouble(tokens, ref position, "x0");
        double x1 = ReadDouble(tokens, ref position, "x1");
        double y0 = ReadDouble(tokens, ref position, "y0");
        double coarseStep = ReadDouble(tokens, ref position, "h1");
        double fineStep = ReadDouble(tokens, ref position, "h2");

        if (!double.IsFinite(x0) || !double.IsFinite(x1) || !double.IsFinite(y0)
            || !double.IsFinite(coarseStep) || !double.IsFinite(fineStep))
        {
            throw new InputDataException("все параметры должны быть конечными числами");
        }

        if (x1 <= x0)
        {
            throw new InputDataException("должно выполняться x0 < x1");
        }

        if (coarseStep <= 0 || fineStep <= 0)
        {
            throw new InputDataException("шаги h1 и h2 должны быть положительными");
        }

        if (Math.Abs(fineStep - coarseStep / 2.0) > 1e-12 * coarseStep)
        {
            throw new InputDataException("для правила Рунге шаг h2 должен быть равен h1 / 2");
        }

        double length = x1 - x0;
        double coarseRatio = length / coarseStep;
        double fineRatio = length / fineStep;
        if (Math.Abs(coarseRatio - Math.Round(coarseRatio)) > 1e-8
            || Math.Abs(fineRatio - Math.Round(fineRatio)) > 1e-8)
        {
            throw new InputDataException("шаги h1 и h2 должны нацело укладываться в отрезок [x0; x1]");
        }

        return new InputData(x0, x1, y0, coarseStep, fineStep);
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
