using System.Collections.Generic;
using System.Diagnostics;

public class ExtMoveArrayWrapper
{
    public int current;

    public ExtMove[] table;

    public ExtMoveArrayWrapper(ExtMoveArrayWrapper wrapper)
        : this(wrapper.table, wrapper.current)
    {
    }

    public ExtMoveArrayWrapper(ExtMove[] table)
        : this(table, 0)
    {
    }

    public ExtMoveArrayWrapper(ExtMove[] table, int current)
    {
        this.table = table;
        this.current = current;
    }

    public ExtMove this[int index]
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

    public void set(ExtMove[] table)
    {
        this.table = table;
        this.current = 0;
    }

    public static ExtMoveArrayWrapper operator +(ExtMoveArrayWrapper p, int value)
    {
        p.current += value;
        return p;
    }

    public static bool operator ==(ExtMoveArrayWrapper p1, ExtMoveArrayWrapper p2)
    {
        return p1.table == p2.table && p1.current == p2.current;
    }

    public static bool operator !=(ExtMoveArrayWrapper p1, ExtMoveArrayWrapper p2)
    {
        return p1.table != p2.table || p1.current != p2.current;
    }

    public static ExtMoveArrayWrapper operator ++(ExtMoveArrayWrapper p)
    {
        p.current += 1;
        return p;
    }

    public static ExtMoveArrayWrapper operator --(ExtMoveArrayWrapper p)
    {
        p.current -= 1;
        return p;
    }

    public void Add(Move m)
    {
        this.table[this.current] = new ExtMove(m, this.table[this.current].Value);
        this.current++;
    }

    public void setCurrentMove(Move m)
    {
        this.table[this.current] = new ExtMove(m, this.table[this.current].Value);
    }

    public Move getCurrentMove()
    {
        return this.table[this.current].Move;
    }

    public static ExtMoveArrayWrapper Partition(ExtMoveArrayWrapper begin, ExtMoveArrayWrapper end)
    {
        Debug.Assert(begin.table == end.table);
        Debug.Assert(begin.current < end.current);

        var _First = begin.current;
        var _Last = end.current;

        for (; ; ++_First)
        {   // find any out-of-order pair
            for (; _First != _Last && (begin[_First].Value > Value.VALUE_ZERO); ++_First)
                ;   // skip in-place elements at beginning
            if (_First == _Last)
                break;  // done

            for (; _First != --_Last && !(begin[_Last].Value > Value.VALUE_ZERO);)
                ;   // skip in-place elements at end
            if (_First == _Last)
                break;  // done

            var tempValue = begin[_First];
            begin[_First] = begin[_Last];
            begin[_Last] = tempValue;
            
        }
        return new ExtMoveArrayWrapper(begin.table, _First);
        
    }

    // Our insertion sort, which is guaranteed to be stable, as it should be
    public static void insertion_sort(ExtMoveArrayWrapper begin, ExtMoveArrayWrapper end)
    {
        Debug.Assert(begin.table == end.table);
        Debug.Assert(begin.current <= end.current);

        int q;
        for (var p = begin.current + 1; p < end.current; ++p)
        {
            var tmp = begin[p];
            for (q = p; q != begin.current && begin[q - 1].Value < tmp.Value; --q)
            {
                begin[q] = begin [q - 1];
            }

            begin[q] = tmp;
        }
        
    }
}