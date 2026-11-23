using System.Numerics;

namespace Lab2
{
    static class NormalForms
    {
        private readonly record struct Implicant(int Mask, int Value)
        {
            public bool Covers(int minterm)
            {
                return (minterm & ~Mask) == (Value & ~Mask);
            }

            public int LiteralCount(int variableCount)
            {
                return variableCount - BitOperations.PopCount((uint)Mask);
            }
        }

        public static string Dnf(Table table)
        {
            List<string> terms = new List<string>();
            for (int row = table.RowCount - 1; row >= 0; row--)
            {
                if (table.ValueAt(row))
                {
                    terms.Add(Minterm(row, table));
                }
            }
            return Join(terms, "0");
        }

        public static string Cnf(Table table)
        {
            List<string> terms = new List<string>();
            for (int row = table.RowCount - 1; row >= 0; row--)
            {
                if (!table.ValueAt(row))
                {
                    terms.Add("(" + Maxterm(row, table) + ")");
                }
            }
            return terms.Count == 0 ? "1" : string.Join(" ", terms);
        }

        public static string Mdnf(Table table)
        {
            int n = table.VariableCount;
            List<int> minterms = new List<int>();
            for (int row = 0; row < table.RowCount; row++)
            {
                if (table.ValueAt(row))
                {
                    minterms.Add(row);
                }
            }

            if (minterms.Count == 0)
            {
                return "0";
            }
            if (minterms.Count == table.RowCount)
            {
                return "1";
            }

            List<Implicant> primes = PrimeImplicants(n, minterms);
            List<Implicant> essential = EssentialPrimes(primes, minterms, out List<int> uncovered);
            List<Implicant> cover = new List<Implicant>(essential);

            if (uncovered.Count > 0)
            {
                List<Implicant> candidates = primes
                    .Where(p => !essential.Contains(p) && uncovered.Any(p.Covers))
                    .ToList();
                cover.AddRange(MinimalCover(candidates, uncovered.ToArray(), n));
            }

            cover.Sort(CompareImplicants);
            return Join(cover.Select(p => FormatImplicant(p, table)).ToList(), "0");
        }

        private static string Minterm(int row, Table table)
        {
            List<string> literals = new List<string>();
            for (int i = 0; i < table.VariableCount; i++)
            {
                string name = table.VariableNames[i];
                literals.Add(IsBitSet(row, i, table.VariableCount) ? name : "!" + name);
            }
            return string.Concat(literals);
        }

        private static string Maxterm(int row, Table table)
        {
            List<string> literals = new List<string>();
            for (int i = 0; i < table.VariableCount; i++)
            {
                string name = table.VariableNames[i];
                literals.Add(IsBitSet(row, i, table.VariableCount) ? "!" + name : name);
            }
            return string.Join("v", literals);
        }

        private static bool IsBitSet(int row, int index, int variableCount)
        {
            return ((row >> (variableCount - 1 - index)) & 1) != 0;
        }

        private static string FormatImplicant(Implicant implicant, Table table)
        {
            List<string> literals = new List<string>();
            for (int i = 0; i < table.VariableCount; i++)
            {
                int bit = 1 << (table.VariableCount - 1 - i);
                if ((implicant.Mask & bit) != 0)
                {
                    continue;
                }
                string name = table.VariableNames[i];
                literals.Add((implicant.Value & bit) != 0 ? name : "!" + name);
            }
            return literals.Count == 0 ? "1" : string.Concat(literals);
        }

        private static string Join(List<string> terms, string emptyValue)
        {
            return terms.Count == 0 ? emptyValue : string.Join(" v ", terms);
        }

        private static List<Implicant> PrimeImplicants(int n, List<int> minterms)
        {
            HashSet<Implicant> current = new HashSet<Implicant>();
            foreach (int minterm in minterms)
            {
                current.Add(new Implicant(0, minterm));
            }

            HashSet<Implicant> primes = new HashSet<Implicant>();
            while (current.Count > 0)
            {
                List<Implicant> items = current.OrderBy(p => p.Mask).ThenBy(p => p.Value).ToList();
                HashSet<Implicant> next = new HashSet<Implicant>();
                HashSet<Implicant> used = new HashSet<Implicant>();

                for (int i = 0; i < items.Count; i++)
                {
                    for (int j = i + 1; j < items.Count; j++)
                    {
                        Implicant first = items[i];
                        Implicant second = items[j];
                        if (first.Mask != second.Mask)
                        {
                            continue;
                        }

                        int diff = first.Value ^ second.Value;
                        if (diff == 0 || (diff & (diff - 1)) != 0 || (diff & first.Mask) != 0)
                        {
                            continue;
                        }

                        int mask = first.Mask | diff;
                        next.Add(new Implicant(mask, first.Value & ~mask));
                        used.Add(first);
                        used.Add(second);
                    }
                }

                foreach (Implicant item in current)
                {
                    if (!used.Contains(item))
                    {
                        primes.Add(item);
                    }
                }
                current = next;
            }

            return primes.OrderBy(p => p.Mask).ThenBy(p => p.Value).ToList();
        }

        private static List<Implicant> EssentialPrimes(
            List<Implicant> primes,
            List<int> minterms,
            out List<int> uncovered)
        {
            List<Implicant> essential = new List<Implicant>();
            foreach (int minterm in minterms)
            {
                List<Implicant> covering = primes.Where(p => p.Covers(minterm)).ToList();
                if (covering.Count == 1 && !essential.Contains(covering[0]))
                {
                    essential.Add(covering[0]);
                }
            }

            uncovered = minterms.Where(m => !essential.Any(p => p.Covers(m))).ToList();
            return essential;
        }

        private static List<Implicant> MinimalCover(List<Implicant> candidates, int[] uncovered, int n)
        {
            List<Implicant>? best = null;
            List<Implicant> current = new List<Implicant>();

            void Search(int[] remaining)
            {
                if (remaining.Length == 0)
                {
                    if (best == null || IsBetter(current, best, n))
                    {
                        best = new List<Implicant>(current);
                    }
                    return;
                }
                if (best != null && current.Count >= best.Count)
                {
                    return;
                }

                int first = remaining[0];
                foreach (Implicant prime in candidates)
                {
                    if (!prime.Covers(first) || current.Contains(prime))
                    {
                        continue;
                    }

                    int[] next = remaining.Where(m => !prime.Covers(m)).ToArray();
                    current.Add(prime);
                    Search(next);
                    current.RemoveAt(current.Count - 1);
                }
            }

            Search(uncovered);
            return best ?? new List<Implicant>();
        }

        private static bool IsBetter(List<Implicant> candidate, List<Implicant> best, int n)
        {
            if (candidate.Count != best.Count)
            {
                return candidate.Count < best.Count;
            }

            int candidateLiterals = candidate.Sum(p => p.LiteralCount(n));
            int bestLiterals = best.Sum(p => p.LiteralCount(n));
            if (candidateLiterals != bestLiterals)
            {
                return candidateLiterals < bestLiterals;
            }

            List<Implicant> left = candidate.OrderBy(p => p.Mask).ThenBy(p => p.Value).ToList();
            List<Implicant> right = best.OrderBy(p => p.Mask).ThenBy(p => p.Value).ToList();
            for (int i = 0; i < left.Count; i++)
            {
                int comparison = CompareImplicants(left[i], right[i]);
                if (comparison != 0)
                {
                    return comparison < 0;
                }
            }
            return false;
        }

        private static int CompareImplicants(Implicant first, Implicant second)
        {
            int comparison = first.Mask.CompareTo(second.Mask);
            return comparison != 0 ? comparison : first.Value.CompareTo(second.Value);
        }
    }
}
