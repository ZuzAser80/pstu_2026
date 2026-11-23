using LabsUtil;

namespace Lab2
{
    class Program
    {
        private static void Main()
        {
            while (true)
            {
                Table table = new Table();
                table.Fill();
                table.Print();

                Console.WriteLine();
                Console.WriteLine("СДНФ: " + NormalForms.Dnf(table));
                Console.WriteLine("СКНФ: " + NormalForms.Cnf(table));
                Console.WriteLine("МДНФ: " + NormalForms.Mdnf(table));

                if (!WantsAnotherTable())
                {
                    return;
                }
            }
        }

        private static bool WantsAnotherTable()
        {
            int choice = PstuUtil.TryReadT<int>("1 : new table\n2 : exit", 1, 2);
            return choice == 1;
        }
    }
}
