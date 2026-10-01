using System;
using System.Diagnostics.Contracts;
using LabsUtil;

class Program
{
    static readonly Random _random = new();
    const int MinValue = -100, MaxValue = 100;

    static void Main(string[] args)
    {
        System.Console.WriteLine("1. Двумерные массивы \n2. Рваные массивы");
        int choice = PstuUtil.TryReadT<int>("?: ", 1, 2);
        switch (choice)
        {
            case 1:
                System.Console.WriteLine("1. Ввести вручную \n2. Рандом \n3. Добавить столбец \n4. Выход");
                choice = PstuUtil.TryReadT<int>("?: ", 1, 2);
                int[,] currentArray = { };
                int n;
                switch (choice)
                {
                    case 1:
                        n = PstuUtil.TryReadT<int>("n: ");
                        currentArray = InputArray(new int[n, n]);
                        break;
                    case 2:
                        n = PstuUtil.TryReadT<int>("n: ");
                        currentArray = RandomArray(new int[PstuUtil.TryReadT<int>("n: "), PstuUtil.TryReadT<int>("m: ")]);
                        break;
                }
                System.Console.WriteLine("Before:");
                PrintArray(currentArray);
                var rectArrAfter = AddColumn(currentArray);
                System.Console.WriteLine("After:");
                PrintArray(rectArrAfter);
                break;
            case 2:
                System.Console.WriteLine("1. Ввести вручную \n2. Рандом \n3. Добавить столбец \n4. Выход");
                choice = PstuUtil.TryReadT<int>("?: ", 1, 3);
                int[][] currentJagged = { };
                switch (choice)
                {
                    case 1:
                        {
                            n = PstuUtil.TryReadT<int>("n: ");
                            var jagged = new int[n][];
                            for (int i = 0; i < n; i++)
                                jagged[i] = new int[PstuUtil.TryReadT<int>($"m[{i}]: ")];
                            currentJagged = InputArray(jagged);
                            break;
                        }
                    case 2:
                        {
                            n = PstuUtil.TryReadT<int>("n: ");
                            int minLen = PstuUtil.TryReadT<int>("min length: ");
                            int maxLen = PstuUtil.TryReadT<int>("max length: ");
                            var jagged = new int[n][];
                            for (int i = 0; i < n; i++)
                                jagged[i] = new int[_random.Next(minLen, maxLen + 1)];
                            currentJagged = RandomArray(jagged);
                            break;
                        }
                    case 3:
                        {
                            n = PstuUtil.TryReadT<int>("n: ");
                            var jagged = new int[n][];
                            for (int i = 0; i < n; i++)
                                jagged[i] = new int[PstuUtil.TryReadT<int>($"m[{i}]: ")];
                            currentJagged = InputArray(jagged);
                            break;
                        }
                }
                System.Console.WriteLine("Before:");
                PrintArray(currentJagged);
                if (choice == 3)
                {
                    var jaggedWithCol = AddColumn(currentJagged);
                    System.Console.WriteLine("After:");
                    PrintArray(jaggedWithCol);
                }
                else
                {
                    int k = PstuUtil.TryReadT<int>("k: ");
                    var jaggedAfter = RemoveAllKLines(currentJagged, k);
                    System.Console.WriteLine("After:");
                    PrintArray(jaggedAfter);
                }
                break;
        }
    }

    static int[][] InputArray(int[][] arr)
    {
        for (int i = 0; i < arr.Length; i++)
            for (int j = 0; j < arr[i].Length; j++)
                arr[i][j] = PstuUtil.TryReadT<int>($"a[{i},{j}]: ");
        return arr;
    }

    static int[][] RemoveAllKLines(int[][] arr, int k)
    {
        int[][] res = new int[0][];
        for (int i = 0; i < arr.Length; i++)
        {
            bool containsK = false;
            for (int j = 0; j < arr[i].Length; j++)
            {
                if (arr[i][j] == k)
                {
                    containsK = true;
                    break;
                }
            }
            if (!containsK)
            {
                Array.Resize(ref res, res.Length + 1);
                res[^1] = arr[i];
            }
        }
        return res;
    }

    static int[,] InputArray(int[,] arr)
    {
        for (int i = 0; i < arr.GetLength(0); i++)
            for (int j = 0; j < arr.GetLength(1); j++)
                arr[i, j] = PstuUtil.TryReadT<int>($"a[{i},{j}]: ");
        return arr;
    }

    static int[][] RandomArray(int[][] arr)
    {
        for (int i = 0; i < arr.Length; i++)
            for (int j = 0; j < arr[i].Length; j++)
                arr[i][j] = _random.Next(MinValue, MaxValue + 1);
        return arr;
    }

    static int[,] RandomArray(int[,] arr)
    {
        for (int i = 0; i < arr.GetLength(0); i++)
            for (int j = 0; j < arr.GetLength(1); j++)
                arr[i, j] = _random.Next(MinValue, MaxValue + 1);
        return arr;
    }

    static void PrintArray(int[,] arr)
    {
        for (int i = 0; i < arr.GetLength(0); i++)
        {
            for (int j = 0; j < arr.GetLength(1); j++)
                System.Console.Write(arr[i, j] + " ");
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

    static int[,] AddColumn(int[,] arr)
    {
        int n = arr.GetLength(0);
        int m = arr.GetLength(1);
        var result = new int[n, m + 1];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < m; j++)
                result[i, j] = arr[i, j];
        return result;
    }

    static int[][] AddColumn(int[][] arr)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            var newRow = new int[arr[i].Length + 1];
            for (int j = 0; j < arr[i].Length; j++)
                newRow[j] = arr[i][j];
            arr[i] = newRow;
        }
        return arr;
    }
}
