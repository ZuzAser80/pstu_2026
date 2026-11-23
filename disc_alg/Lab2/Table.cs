using LabsUtil;

namespace Lab2
{
    class Table
    {
        private enum InputMode
        {
            Manual = 1,
            Random = 2
        }

        private static readonly string[] VariableLabels = { "x", "y", "z", "t", "u" };

        private readonly Random _rnd = new Random();
        private int[] _values = Array.Empty<int>();

        public int VariableCount { get; private set; }

        public int RowCount => _values.Length;

        public IReadOnlyList<string> VariableNames => VariableLabels.Take(VariableCount).ToArray();

        public void Fill()
        {
            VariableCount = PstuUtil.TryReadT<int>("input number of variables (1-5): ", 1, VariableLabels.Length);
            _values = new int[1 << VariableCount];

            InputMode mode = ReadInputMode();
            if (mode == InputMode.Manual)
            {
                FillManual();
            }
            else
            {
                FillRandom();
            }
        }

        public bool ValueAt(int row)
        {
            return _values[row] != 0;
        }

        public void Print()
        {
            Console.WriteLine();
            Console.WriteLine("--- truth table ---");
            Console.WriteLine(string.Join(" ", VariableNames) + " | f");
            for (int row = 0; row < RowCount; row++)
            {
                Console.WriteLine(FormatAssignment(row) + " | " + _values[row]);
            }
            Console.WriteLine("-------------------");
        }

        private string FormatAssignment(int row)
        {
            string[] bits = new string[VariableCount];
            for (int i = 0; i < VariableCount; i++)
            {
                bits[i] = IsBitSet(row, i) ? "1" : "0";
            }
            return string.Join(" ", bits);
        }

        private bool IsBitSet(int row, int index)
        {
            return ((row >> (VariableCount - 1 - index)) & 1) != 0;
        }

        private static InputMode ReadInputMode()
        {
            int choice = PstuUtil.TryReadT<int>("1 : manual input\n2 : random input", 1, 2);
            return (InputMode)choice;
        }

        private void FillManual()
        {
            for (int row = 0; row < RowCount; row++)
            {
                _values[row] = PstuUtil.TryReadT<int>("input value for " + FormatAssignment(row) + ": ", 0, 1);
            }
        }

        private void FillRandom()
        {
            for (int row = 0; row < RowCount; row++)
            {
                _values[row] = _rnd.Next(2);
            }
        }
    }
}
