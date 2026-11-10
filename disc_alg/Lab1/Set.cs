using System;

public class Set : IEquatable<Set>
{
    private readonly int[] _elements;

    private Set(int[] elements)
    {
        _elements = elements;
    }

    public static Set Empty => new Set(Array.Empty<int>());

    public static Set Of(int[] raw)
    {
        return new Set(Deduplicate(raw));
    }

    public int Count => _elements.Length;

    public int[] Elements => (int[])_elements.Clone();

    public bool Contains(int value)
    {
        return Array.IndexOf(_elements, value) >= 0;
    }

    public Set Intersect(Set other)
    {
        return Filter(other.Contains);
    }

    public Set Union(Set other)
    {
        return Concatenate(this, other.Difference(this));
    }

    public Set Difference(Set other)
    {
        return Filter(value => !other.Contains(value));
    }

    public Set Xor(Set other)
    {
        return Difference(other).Union(other.Difference(this));
    }

    public Set Complement(Set universal)
    {
        return universal.Filter(value => !Contains(value));
    }

    public bool IsSubsetOf(Set other)
    {
        foreach (int value in _elements)
        {
            if (!other.Contains(value))
            {
                return false;
            }
        }
        return true;
    }

    public bool Equals(Set? other)
    {
        return other is not null
            && _elements.Length == other._elements.Length
            && IsSubsetOf(other);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Set);
    }

    public override int GetHashCode()
    {
        int hash = 0;
        foreach (int value in _elements)
        {
            hash += value;
        }
        return hash;
    }

    public override string ToString()
    {
        return "{" + string.Join(", ", _elements) + "}";
    }

    private Set Filter(Func<int, bool> keep)
    {
        int[] buffer = new int[_elements.Length];
        int count = 0;
        foreach (int value in _elements)
        {
            if (keep(value))
            {
                buffer[count++] = value;
            }
        }
        return new Set(Trim(buffer, count));
    }

    private static Set Concatenate(Set first, Set second)
    {
        int[] result = new int[first.Count + second.Count];
        Array.Copy(first._elements, 0, result, 0, first.Count);
        Array.Copy(second._elements, 0, result, first.Count, second.Count);
        return new Set(result);
    }

    private static int[] Deduplicate(int[] raw)
    {
        int[] buffer = new int[raw.Length];
        int count = 0;
        foreach (int value in raw)
        {
            if (Array.IndexOf(buffer, value, 0, count) < 0)
            {
                buffer[count++] = value;
            }
        }
        return Trim(buffer, count);
    }

    private static int[] Trim(int[] buffer, int count)
    {
        if (count == buffer.Length)
        {
            return buffer;
        }

        int[] result = new int[count];
        Array.Copy(buffer, 0, result, 0, count);
        return result;
    }
}
