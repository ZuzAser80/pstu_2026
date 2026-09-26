using System;

public class Set
{
    private readonly int[] _elements;

    private Set(int[] elements)
    {
        _elements = elements;
    }

    public static Set Empty => new Set(new int[0]);

    public static Set Of(int[] raw)
    {
        int[] temp = new int[raw.Length];
        int n = 0;
        for (int i = 0; i < raw.Length; i++)
        {
            if (!Contains(temp, raw[i], n))
            {
                temp[n++] = raw[i];
            }
        }
        int[] elements = new int[n];
        for (int i = 0; i < n; i++)
        {
            elements[i] = temp[i];
        }
        return new Set(elements);
    }

    public int Count => _elements.Length;

    public int[] Elements => _elements;

    public bool Contains(int value)
    {
        for (int i = 0; i < _elements.Length; i++)
        {
            if (_elements[i] == value) return true;
        }
        return false;
    }

    public Set Intersect(Set other)
    {
        int n = 0;
        for (int i = 0; i < _elements.Length; i++)
        {
            if (other.Contains(_elements[i])) n++;
        }
        int[] result = new int[n];
        int k = 0;
        for (int i = 0; i < _elements.Length; i++)
        {
            if (other.Contains(_elements[i])) result[k++] = _elements[i];
        }
        return new Set(result);
    }

    public Set Union(Set other)
    {
        int n = _elements.Length;
        for (int i = 0; i < other._elements.Length; i++)
        {
            if (!Contains(other._elements[i])) n++;
        }
        int[] result = new int[n];
        int k = 0;
        for (int i = 0; i < _elements.Length; i++)
        {
            result[k++] = _elements[i];
        }
        for (int i = 0; i < other._elements.Length; i++)
        {
            if (!Contains(other._elements[i])) result[k++] = other._elements[i];
        }
        return new Set(result);
    }

    public Set Difference(Set other)
    {
        int n = 0;
        for (int i = 0; i < _elements.Length; i++)
        {
            if (!other.Contains(_elements[i])) n++;
        }
        int[] result = new int[n];
        int k = 0;
        for (int i = 0; i < _elements.Length; i++)
        {
            if (!other.Contains(_elements[i])) result[k++] = _elements[i];
        }
        return new Set(result);
    }

    public Set Xor(Set other)
    {
        return Difference(other).Union(other.Difference(this));
    }

    public Set Complement(Set universal)
    {
        int n = 0;
        for (int i = 0; i < universal._elements.Length; i++)
        {
            if (!Contains(universal._elements[i])) n++;
        }
        int[] result = new int[n];
        int k = 0;
        for (int i = 0; i < universal._elements.Length; i++)
        {
            if (!Contains(universal._elements[i]))
            {
                result[k++] = universal._elements[i];
            }
        }
        return new Set(result);
    }

    public bool IsSubsetOf(Set other)
    {
        for (int i = 0; i < _elements.Length; i++)
        {
            if (!other.Contains(_elements[i]))
            {
                return false;
            }
        }
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Set other) return false;
        if (_elements.Length != other._elements.Length) return false;
        return IsSubsetOf(other);
    }

    public override int GetHashCode()
    {
        int h = 0;
        for (int i = 0; i < _elements.Length; i++)
        {
            h += _elements[i];
        }
        return h;
    }

    public override string ToString()
    {
        if (_elements.Length == 0) return "{}";
        string s = "{";
        for (int i = 0; i < _elements.Length; i++)
        {
            s += (i > 0 ? ", " : "") + _elements[i];
        }
        return s + "}";
    }

    private static bool Contains(int[] arr, int value, int length)
    {
        for (int i = 0; i < length; i++)
        {
            if (arr[i] == value) return true;
        }
        return false;
    }
}