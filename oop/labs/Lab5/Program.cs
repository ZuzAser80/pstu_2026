using System;

class Program
{
    static readonly Random _random = new();
    const int MinValue = -100, MaxValue = 100;

    static void Main(string[] args)
    {
        System.Console.WriteLine("1. Двумерные массивы \n 2. Рваные массивы");
        int choice = ReadInput("?: ");
        switch (choice)
        {
            case 1:
                System.Console.WriteLine("1. Ввести вручную \n 2. Рандом \n 3. Выход");
                choice = ReadInput("?: ");
                switch (choice)
                {
                    case 1:
                        InputRectangularArray();
                        break;
                    case 2:
                        PrintArray(RandomRectangularArray());
                        break;
                }
                break;

            case 2:
                System.Console.WriteLine("1. Ввести вручную \n 2. Рандом \n 3. Выход");
                choice = ReadInput("?: ");
                switch (choice)
                {
                    case 1:
                        InputJaggedArray();
                        break;
                    case 2:
                        PrintArray(RandomJaggedArray());
                        break;
                }
                break;
        }



    }

    static int[][] InputJaggedArray()
    {
        int n = ReadInput("n: ");
        var result = new int[n][];
        for (int i = 0; i < n; i++)
        {
            int m = ReadInput("m: ");
            result[i] = new int[m];
            for (int j = 0; j < m; j++)
                result[i][j] = ReadInput($"a[{i},{j}]: ");
        }
        return result;
    }

    static int[][] RandomJaggedArray()
    {
        int n = ReadInput("n: ");
        int minLen = ReadInput("min length: ");
        int maxLen = ReadInput("max length: ");
        var result = new int[n][];
        for (int i = 0; i < n; i++)
        {
            int m = _random.Next(minLen, maxLen + 1);
            result[i] = new int[m];
            for (int j = 0; j < m; j++)
                result[i][j] = _random.Next(MinValue, MaxValue + 1);
        }
        return result;
    }

    static int[,] RandomRectangularArray()
    {
        int n = ReadInput("n: ");
        int m = ReadInput("m: ");
        var result = new int[n, m];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < m; j++)
                result[i, j] = _random.Next(MinValue, MaxValue + 1);
        return result;
    }

    static void PrintArray(int[,] arr)
    {
        for (int i = 0; i < arr.GetLength(0); i++)
        {
            for (int j = 0; j < arr.GetLength(1); j++) {
                System.Console.Write(arr[i, j] + " ");
            }
            System.Console.WriteLine();
        }
    }

    static void PrintArray(int[][] arr)
    {
        foreach (var row in arr)
        {
            foreach (int value in row)
                System.Console.Write(value + " ");
            System.Console.WriteLine();
        }
    }

    static int[,] InputRectangularArray()
    {
        int n = ReadInput("n: ");
        int m = ReadInput("m: ");
        var result = new int[n, m];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < m; j++)
                result[i, j] = ReadInput($"a[{i},{j}]: ");
        return result;
    }

    static int ReadInput(string prompt)
    {
        int result;
        bool isParsed;
        do
        {
            System.Console.WriteLine(prompt);
            isParsed = int.TryParse(System.Console.ReadLine(), out result);
            if (!isParsed)
                System.Console.WriteLine("Invalid number. Please enter an integer.");
        } while (!isParsed);
        return result;
    }


}

