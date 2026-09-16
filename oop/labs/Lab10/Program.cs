using Lab10;

class Program
{
    static void Main(string[] args)
    {
        var random = new Random();

        Console.WriteLine("=== Task 2-4: обрабатываем массив разных классов иерархии ===\n");

        var array = new Organisation[8];
        for (int i = 0; i < array.Length; i++)
        {
            switch (random.Next(0, 4))
            {
                case 0: array[i] = new Dock(); break;
                case 1: array[i] = new Factory(); break;
                case 2: array[i] = new InsuranceCompany(); break;
                default: array[i] = new Library(); break;
            }
            array[i].RandomInit();
        }

        Console.WriteLine("Исходный массив (RandomInit):");
        PrintArray(array);

        // Task 2: сортировка по IComparable (AnnualMoneyEarned, возрастание)
        Array.Sort(array);
        Console.WriteLine("\nTask 2: Array.Sort() по IComparable (AnnualMoneyEarned):");
        PrintArray(array);

        // Task 3: сортировка по другому критерию через IComparer (по имени)
        Array.Sort(array, new NameComparer());
        Console.WriteLine("\nTask 3: Array.Sort(comparer) по NameComparer (по Name):");
        PrintArray(array);

        // Task 3: поиск элемента по тому же критерию (Array.BinarySearch + IComparer)
        var target = array[3];
        int index = Array.BinarySearch(array, target, new NameComparer());
        Console.WriteLine($"\nTask 3: поиск через Array.BinarySearch(comparer), элемент '{target.Name}' найден на индексе {index}");

        // Task 4: бинарный поиск
        Console.WriteLine("\nTask 4: бинарный поиск по имени:");
        string existingName = array[3].Name;
        int found = BinarySearch(array, existingName, new NameComparer());
        Console.WriteLine($"Ищем существующее имя '{existingName}' -> индекс {found}");

        string missingName = Guid.NewGuid().ToString();
        int notFound = BinarySearch(array, missingName, new NameComparer());
        Console.WriteLine($"Ищем отсутствующее имя '{missingName}' -> индекс {notFound} (битовое дополнение позиции вставки)");

        Console.WriteLine("\n=== Task 6: массив типа IInit ===\n");

        var initArray = new IInit[5];
        initArray[0] = new Dock();
        initArray[1] = new Library();
        initArray[2] = new Factory();
        initArray[3] = new InsuranceCompany();
        initArray[4] = new InitClass();

        Console.WriteLine("Просмотр массива IInit до инициализации:");
        PrintIInitArray(initArray);

        Console.WriteLine("\nRandomInit() для всех элементов массива IInit:");
        foreach (var item in initArray)
        {
            item.RandomInit();
        }
        PrintIInitArray(initArray);

        Console.WriteLine("\nInit() для всех элементов массива IInit (ввод с клавиатуры):");
        foreach (var item in initArray)
        {
            item.Init();
        }
        PrintIInitArray(initArray);

        Console.WriteLine("\n=== Task 7: Clone (ICloneable) vs ShallowCopy ===\n");

        var dock = new Dock("Санкт-Петербург", "МойДок", 500_000,
            ShipTypeEnum.CARGO, 12, 40_000);
        Console.WriteLine("Оригинал:");
        dock.Show();

        var clone = (Dock)dock.Clone();
        Console.WriteLine("Клон (Clone()):");
        clone.Show();

        var shallow = (Dock)dock.ShallowCopy();
        Console.WriteLine("Поверхностная копия (ShallowCopy / MemberwiseClone):");
        shallow.Show();

        Console.WriteLine($"\nКлон равен оригиналу (учтены поля Dock): {clone.Equals(dock)}");
        Console.WriteLine($"ShallowCopy равен оригиналу: {shallow.Equals(dock)}");
        Console.WriteLine($"Клон — отдельный объект (не та же ссылка): {!ReferenceEquals(clone, dock)}");
        Console.WriteLine("Все поля — значения/строки, поэтому клон и поверхностная копия совпадают по содержимому.");
        Console.WriteLine("Разница видна при ссылочных полях: ShallowCopy() делит ссылки (MemberwiseClone), Clone() копирует их.");
    }

    static void PrintArray(Organisation[] array)
    {
        foreach (var item in array)
        {
            item.Show();
        }
    }

    static void PrintIInitArray(IInit[] array)
    {
        foreach (var item in array)
        {
            if (item is Organisation org)
            {
                org.Show();
            }
            else if (item is InitClass initClass)
            {
                initClass.Show();
            }
        }
    }

    static int BinarySearch(Organisation[] array, string name, NameComparer comparer)
    {
        int left = 0;
        int right = array.Length - 1;
        while (left <= right)
        {
            int mid = left + (right - left) / 2;
            int cmp = comparer.Compare(array[mid], new Organisation("", name, 0));
            if (cmp == 0)
            {
                return mid;
            }
            if (cmp < 0)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }
        return ~left;
    }
}