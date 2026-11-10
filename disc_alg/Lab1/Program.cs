using System;
using LabsUtil;

class Program
{
    private enum InputMode
    {
        Manual = 1,
        Random = 2
    }

    private enum Operation
    {
        Intersection = 1,
        Union = 2,
        Difference = 3,
        SymmetricDifference = 4
    }

    private readonly string[] _names = { "A", "B", "C", "D", "E" };
    private readonly Set?[] _sets;
    private readonly Random _rnd = new Random();
    private Set? _universal;

    private Program()
    {
        _sets = new Set?[_names.Length];
    }

    private static void Main()
    {
        new Program().Run();
    }

    private void Run()
    {
        while (true)
        {
            PrintSets();
            PrintMainMenu();
            int choice = ReadChoice(1, 7, "choose action: ");
            if (choice == 7)
            {
                return;
            }
            Dispatch(choice);
        }
    }

    private void Dispatch(int choice)
    {
        switch (choice)
        {
            case 1:
                SetUniverse();
                break;
            case 2:
                EditSet();
                break;
            case 3:
                RunOperation();
                break;
            case 4:
                ShowComplement();
                break;
            case 5:
                CheckMembership();
                break;
            case 6:
                CheckSubset();
                break;
        }
    }

    private void SetUniverse()
    {
        _universal = ReadSet("U", null);
    }

    private void EditSet()
    {
        if (!EnsureUniverse())
        {
            return;
        }

        int index = PickIndex("choose the set to fill: ");
        _sets[index] = ReadSet(_names[index], _universal);
    }

    private void RunOperation()
    {
        if (!TryPickTwoSets(out Set? first, out Set? second))
        {
            return;
        }

        Operation operation = ReadOperation();
        Set result = Apply(first!, second!, operation);
        Console.WriteLine("result = " + result);
    }

    private void ShowComplement()
    {
        if (!EnsureUniverse())
        {
            return;
        }

        int index = PickIndex("choose the set to complement: ");
        if (!TryGetSet(index, out Set? set))
        {
            PrintSetNotSet();
            return;
        }

        Set complement = set!.Complement(_universal!);
        Console.WriteLine(_names[index] + " = " + complement);
    }

    private void CheckMembership()
    {
        int index = PickIndex("choose the set: ");
        if (!TryGetSet(index, out Set? set))
        {
            PrintSetNotSet();
            return;
        }

        int value = PstuUtil.TryReadT<int>("input element: ");
        bool contains = set!.Contains(value);
        Console.WriteLine(value + " in " + _names[index] + " : " + (contains ? "yes" : "no"));
    }

    private void CheckSubset()
    {
        int firstIndex = PickIndex("choose the subset candidate (first): ");
        int secondIndex = PickIndex("choose the super set (second): ");
        if (!TryGetSet(firstIndex, out Set? first) || !TryGetSet(secondIndex, out Set? second))
        {
            Console.WriteLine("both sets must be set first");
            return;
        }

        bool isSubset = first!.IsSubsetOf(second!);
        Console.WriteLine(_names[firstIndex] + " subset of " + _names[secondIndex] + " : " + (isSubset ? "yes" : "no"));
    }

    private bool EnsureUniverse()
    {
        if (_universal != null)
        {
            return true;
        }

        Console.WriteLine("set universal set U first");
        return false;
    }

    private bool TryPickTwoSets(out Set? first, out Set? second)
    {
        int firstIndex = PickIndex("choose first set: ");
        int secondIndex = PickIndex("choose second set: ");
        first = _sets[firstIndex];
        second = _sets[secondIndex];
        if (first != null && second != null)
        {
            return true;
        }

        Console.WriteLine("both sets must be set first");
        return false;
    }

    private bool TryGetSet(int index, out Set? set)
    {
        set = _sets[index];
        return set != null;
    }

    private static Set Apply(Set first, Set second, Operation operation)
    {
        switch (operation)
        {
            case Operation.Intersection:
                return first.Intersect(second);
            case Operation.Union:
                return first.Union(second);
            case Operation.Difference:
                return first.Difference(second);
            default:
                return first.Xor(second);
        }
    }

    private static Operation ReadOperation()
    {
        PrintOperationMenu();
        int choice = ReadChoice(1, 4, "choose operation: ");
        return (Operation)choice;
    }

    private void PrintSets()
    {
        Console.WriteLine();
        Console.WriteLine("--- sets ---");
        PrintSetLine("U", _universal);
        for (int i = 0; i < _names.Length; i++)
        {
            PrintSetLine(_names[i], _sets[i]);
        }
        Console.WriteLine("------------");
    }

    private static void PrintSetLine(string name, Set? set)
    {
        Console.WriteLine(name + " = " + FormatSet(set));
    }

    private static string FormatSet(Set? set)
    {
        return set == null ? "not set" : set.ToString();
    }

    private static void PrintMainMenu()
    {
        Console.WriteLine();
        Console.WriteLine("1 : set universal set U");
        Console.WriteLine("2 : set a set (A-E)");
        Console.WriteLine("3 : set operation on two sets");
        Console.WriteLine("4 : complement of a set (within U)");
        Console.WriteLine("5 : element membership check");
        Console.WriteLine("6 : subset check");
        Console.WriteLine("7 : exit");
    }

    private static void PrintOperationMenu()
    {
        Console.WriteLine("1 : intersection");
        Console.WriteLine("2 : union");
        Console.WriteLine("3 : difference (first \\ second)");
        Console.WriteLine("4 : symmetric difference");
    }

    private static void PrintSetNotSet()
    {
        Console.WriteLine("choose a set that is set first");
    }

    private int PickIndex(string prompt)
    {
        for (int i = 0; i < _names.Length; i++)
        {
            Console.WriteLine((i + 1) + " : set " + _names[i]);
        }
        return ReadChoice(1, _names.Length, prompt) - 1;
    }

    private static int ReadChoice(int min, int max, string prompt)
    {
        return PstuUtil.TryReadT<int>(prompt, min, max);
    }

    private Set ReadSet(string name, Set? universal)
    {
        int size = ReadSetSize(name);
        int[] values = ReadValues(size, universal);
        return Set.Of(values);
    }

    private static int ReadSetSize(string name)
    {
        int size = PstuUtil.TryReadT<int>("enter number of elements for " + name + ": ");
        if (size >= 0)
        {
            return size;
        }

        Console.WriteLine("size can't be negative");
        return 0;
    }

    private int[] ReadValues(int size, Set? universal)
    {
        InputMode mode = ReadInputMode();
        return mode == InputMode.Manual
            ? ReadManual(size, universal)
            : ReadRandom(size, universal);
    }

    private static InputMode ReadInputMode()
    {
        int choice = ReadChoice(1, 2, "1 : manual input\n2 : random");
        return (InputMode)choice;
    }

    private static int[] ReadManual(int size, Set? universal)
    {
        int[] values = new int[size];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = ReadElementInUniverse(universal, i + 1);
        }
        return values;
    }

    private static int ReadElementInUniverse(Set? universal, int ordinal)
    {
        while (true)
        {
            int value = PstuUtil.TryReadT<int>("input element " + ordinal + " : ");
            if (universal == null || universal.Contains(value))
            {
                return value;
            }
            Console.WriteLine("element doesn't belong to U, input again");
        }
    }

    private int[] ReadRandom(int size, Set? universal)
    {
        int[] values = new int[size];
        int[]? pool = universal?.Elements;
        if (pool == null || pool.Length == 0)
        {
            FillDistinctFromRange(values, _rnd);
            return values;
        }

        int distinctCount = Math.Min(size, pool.Length);
        FillDistinctFromPool(values, distinctCount, pool, _rnd);
        FillRemainingFromPool(values, distinctCount, pool, _rnd);
        return values;
    }

    private static void FillDistinctFromRange(int[] values, Random random)
    {
        int filled = 0;
        while (filled < values.Length)
        {
            int candidate = random.Next(100000);
            if (!ContainsAmong(values, filled, candidate))
            {
                values[filled++] = candidate;
            }
        }
    }

    private static void FillDistinctFromPool(int[] values, int count, int[] pool, Random random)
    {
        int filled = 0;
        while (filled < count)
        {
            int candidate = pool[random.Next(pool.Length)];
            if (!ContainsAmong(values, filled, candidate))
            {
                values[filled++] = candidate;
            }
        }
    }

    private static void FillRemainingFromPool(int[] values, int start, int[] pool, Random random)
    {
        for (int i = start; i < values.Length; i++)
        {
            values[i] = pool[random.Next(pool.Length)];
        }
    }

    private static bool ContainsAmong(int[] values, int count, int value)
    {
        return Array.IndexOf(values, value, 0, count) >= 0;
    }
}
