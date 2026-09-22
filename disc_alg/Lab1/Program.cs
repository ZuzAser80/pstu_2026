using System;
using LabsUtil;

class Program
{

    static Random _rnd = new Random();

    static void Main(string[] args)
    {
        string[] names = { "A", "B", "C", "D", "E" };
        Set? universal = null;
        Set?[] sets = new Set?[names.Length];

        bool running = true;
        while (running)
        {
            PrintSets(names, sets, universal);
            Console.WriteLine();
            Console.WriteLine("1 : set universal set U");
            Console.WriteLine("2 : set a set (A-E)");
            Console.WriteLine("3 : set operation on two sets");
            Console.WriteLine("4 : complement of a set (within U)");
            Console.WriteLine("5 : element membership check");
            Console.WriteLine("6 : subset check");
            Console.WriteLine("7 : exit");

            int choice = ReadChoice(1, 7, "choose action: ");
            switch (choice)
            {
                case 1:
                    universal = ReadSet("U", null);
                    break;
                case 2:
                    if (universal == null)
                    {
                        Console.WriteLine("set universal set U first");
                        break;
                    }
                    int fill = PickIndex(names, "choose the set to fill: ");
                    sets[fill] = ReadSet(names[fill], universal);
                    break;
                case 3:
                    {
                        int i1 = PickIndex(names, "choose first set: ");
                        int i2 = PickIndex(names, "choose second set: ");
                        Set? a = sets[i1];
                        Set? b = sets[i2];
                        if (a == null || b == null)
                        {
                            Console.WriteLine("both sets must be set first");
                            break;
                        }
                        Console.WriteLine("1 : intersection");
                        Console.WriteLine("2 : union");
                        Console.WriteLine("3 : difference (first \\ second)");
                        Console.WriteLine("4 : symmetric difference");
                        int op = ReadChoice(1, 4, "choose operation: ");
                        Set result;
                        switch (op)
                        {
                            case 1: result = a.Intersect(b); break;
                            case 2: result = a.Union(b); break;
                            case 3: result = a.Difference(b); break;
                            default: result = a.Xor(b); break;
                        }
                        Console.WriteLine("result = " + result);
                        break;
                    }
                case 4:
                    if (universal == null)
                    {
                        Console.WriteLine("set universal set U first");
                        break;
                    }
                    {
                        int idx = PickIndex(names, "choose the set to complement: ");
                        Set? a = sets[idx];
                        if (a == null)
                        {
                            Console.WriteLine("choose a set that is set first");
                            break;
                        }
                        Console.WriteLine(names[idx] + " = " + a.Complement(universal));
                        break;
                    }
                case 5:
                    {
                        int idx = PickIndex(names, "choose the set: ");
                        Set? a = sets[idx];
                        if (a == null)
                        {
                            Console.WriteLine("choose a set that is set first");
                            break;
                        }
                        int value = PstuUtil.TryReadT<int>("input element: ");
                        Console.WriteLine(value + " in " + names[idx] + " : " + (a.Contains(value) ? "yes" : "no"));
                        break;
                    }
                case 6:
                    {
                        int i1 = PickIndex(names, "choose the subset candidate (first): ");
                        int i2 = PickIndex(names, "choose the super set (second): ");
                        Set? a = sets[i1];
                        Set? b = sets[i2];
                        if (a == null || b == null)
                        {
                            Console.WriteLine("both sets must be set first");
                            break;
                        }
                        Console.WriteLine(names[i1] + " subset of " + names[i2] + " : " + (a.IsSubsetOf(b) ? "yes" : "no"));
                        break;
                    }
                case 7:
                    running = false;
                    break;
            }
        }
    }

    static void PrintSets(string[] names, Set?[] sets, Set? universal)
    {
        Console.WriteLine();
        Console.WriteLine("--- sets ---");
        Console.WriteLine("U = " + (universal == null ? "not set" : universal.ToString()));
        for (int i = 0; i < sets.Length; i++)
        {
            Console.WriteLine(names[i] + " = " + (sets[i] == null ? "not set" : sets[i].ToString()));
        }
        Console.WriteLine("------------");
    }

    static int PickIndex(string[] names, string prompt)
    {
        for (int i = 0; i < names.Length; i++)
        {
            Console.WriteLine((i + 1) + " : set " + names[i]);
        }
        return ReadChoice(1, names.Length, prompt) - 1;
    }

    static int ReadChoice(int min, int max, string prompt)
    {
        int value;
        bool ok;
        do
        {
            value = PstuUtil.TryReadT<int>(prompt);
            ok = value >= min && value <= max;
            if (!ok)
            {
                Console.WriteLine("wrong input, choose between " + min + " and " + max);
            }
        } while (!ok);
        return value;
    }

    static Set ReadSet(string name, Set? universal)
    {
        int size = PstuUtil.TryReadT<int>("enter number of elements for " + name + ": ");
        if (size < 0)
        {
            Console.WriteLine("size can't be negative");
            size = 0;
        }
        int choice = ReadChoice(1, 2, "1 : manual input\n2 : random");
        int[] values = new int[size];
        if (choice == 1)
        {
            ReadManual(values, universal);
        }
        else
        {
            ReadRandom(values, universal);
        }
        return Set.Of(values);
    }

    static void ReadManual(int[] values, Set? universal)
    {
        for (int i = 0; i < values.Length; i++)
        {
            int value = PstuUtil.TryReadT<int>("input element " + (i + 1) + " : ");
            if (universal != null && !universal.Contains(value))
            {
                Console.WriteLine("element doesn't belong to U, input again");
                i--;
                continue;
            }
            values[i] = value;
        }
    }

    static void ReadRandom(int[] values, Set? universal)
    {
        int[] pool = universal == null ? null : universal.Elements;
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = (pool == null || pool.Length == 0)
                ? _rnd.Next(100000)
                : pool[_rnd.Next(pool.Length)];
        }
    }

}