using System.Diagnostics;
using System.Runtime.CompilerServices;
#if PRIMITIVE
using MoveT = System.Int32;
#endif

internal class ExtMoveArrayWrapper
{
    internal int current;

    internal ExtMove[] table;

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal ExtMoveArrayWrapper(ExtMoveArrayWrapper wrapper)
        : this(wrapper.table, wrapper.current)
    {
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal ExtMoveArrayWrapper(ExtMove[] table)
        : this(table, 0)
    {
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal ExtMoveArrayWrapper(ExtMove[] table, int current)
    {
        this.table = table;
        this.current = current;
    }

    internal ExtMove this[int index]
    {
        get
        {
            return this.table[index];
        }
        set
        {
            this.table[index] = value;
        }
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    internal void set(ExtMove[] table)
    {
        this.table = table;
        this.current = 0;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    public static ExtMoveArrayWrapper operator +(ExtMoveArrayWrapper p, int value)
    {
        p.current += value;
        return p;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    public static bool operator ==(ExtMoveArrayWrapper p1, ExtMoveArrayWrapper p2)
    {
        return p1.table == p2.table && p1.current == p2.current;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    public static bool operator !=(ExtMoveArrayWrapper p1, ExtMoveArrayWrapper p2)
    {
        return p1.table != p2.table || p1.current != p2.current;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    public static ExtMoveArrayWrapper operator ++(ExtMoveArrayWrapper p)
    {
        p.current++;
        return p;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    public static ExtMoveArrayWrapper operator --(ExtMoveArrayWrapper p)
    {
        p.current--;
        return p;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    internal void Add(MoveT m)
    {
        this.table[this.current] = new ExtMove(m, Value.VALUE_NONE);
        this.current++;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    internal void setCurrentMove(MoveT m)
    {
        this.table[this.current] = new ExtMove(m, Value.VALUE_NONE);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    internal MoveT getCurrentMove()
    {
        return this.table[this.current].Move;
    }

    internal static ExtMoveArrayWrapper Partition(ExtMoveArrayWrapper begin, ExtMoveArrayWrapper end)
    {
        Debug.Assert(begin.table == end.table);
        Debug.Assert(begin.current < end.current);

        var table = begin.table;
        var first = begin.current;
        var last = end.current;

        for (;; ++first)
        {
            // find any out-of-order pair
            for (; first != last && (table[first].Value > Value.VALUE_ZERO); ++first)
            {
            }
            if (first == last)
            {
                break; // done
            }

            for (; first != --last && !(table[last].Value > Value.VALUE_ZERO);)
            {
            }
            if (first == last)
            {
                break; // done
            }

            var tempValue = table[first];
            table[first] = table[last];
            table[last] = tempValue;
        }
        return new ExtMoveArrayWrapper(begin.table, first);
    }

    // Our insertion sort, which is guaranteed to be stable, as it should be
    internal static void insertion_sort(ExtMoveArrayWrapper begin, ExtMoveArrayWrapper end)
    {
        Debug.Assert(begin.table == end.table);
        Debug.Assert(begin.current <= end.current);

        var table = begin.table;
        for (var p = begin.current + 1; p < end.current; ++p)
        {
            var tmp = table[p];
            int q;
            for (q = p; q != begin.current && table[q - 1].Value < tmp.Value; --q)
            {
                table[q] = table[q - 1];
            }

            table[q] = tmp;
        }
    }
}