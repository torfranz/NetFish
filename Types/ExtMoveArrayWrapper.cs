using System.Diagnostics;

#if PRIMITIVE
using MoveT = System.Int32;
#endif

internal class ExtMoveArrayWrapper
{
    internal int current;

    internal ExtMove[] table;

    internal ExtMoveArrayWrapper(ExtMoveArrayWrapper wrapper)
        : this(wrapper.table, wrapper.current)
    {
    }

    internal ExtMoveArrayWrapper(ExtMove[] table)
        : this(table, 0)
    {
    }

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
        p.current += 1;
        return p;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    public static ExtMoveArrayWrapper operator --(ExtMoveArrayWrapper p)
    {
        p.current -= 1;
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

        var _First = begin.current;
        var _Last = end.current;

        for (;; ++_First)
        {
            // find any out-of-order pair
            for (; _First != _Last && (begin[_First].Value > Value.VALUE_ZERO); ++_First)
            {
            }
            if (_First == _Last)
            {
                break; // done
            }

            for (; _First != --_Last && !(begin[_Last].Value > Value.VALUE_ZERO);)
            {
            }
            if (_First == _Last)
            {
                break; // done
            }

            var tempValue = begin[_First];
            begin[_First] = begin[_Last];
            begin[_Last] = tempValue;
        }
        return new ExtMoveArrayWrapper(begin.table, _First);
    }

    // Our insertion sort, which is guaranteed to be stable, as it should be
    internal static void insertion_sort(ExtMoveArrayWrapper begin, ExtMoveArrayWrapper end)
    {
        Debug.Assert(begin.table == end.table);
        Debug.Assert(begin.current <= end.current);

        for (var p = begin.current + 1; p < end.current; ++p)
        {
            var tmp = begin[p];
            int q;
            for (q = p; q != begin.current && begin[q - 1].Value < tmp.Value; --q)
            {
                begin[q] = begin[q - 1];
            }

            begin[q] = tmp;
        }
    }
}