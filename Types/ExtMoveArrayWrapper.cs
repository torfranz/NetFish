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

        var temporaries = new List<ExtMove>(end.current - begin.current);
        var nextGoodLocation = 0;

        for (var idx = begin.current; idx < end.current; idx++)
        {
            // add items where value is > Value.VALUE_ZERO to front
            if (begin[idx].Value > Value.VALUE_ZERO)
            {
                temporaries.Insert(nextGoodLocation++, begin[idx]);
            }
            else
            {
                // otherwise put to end
                temporaries.Add(begin[idx]);
            }
        }

        // put back reordered items to original array locations
        for (var idx = begin.current; idx < end.current; idx++)
        {
            begin[idx] = temporaries[idx - begin.current];
        }

        return new ExtMoveArrayWrapper(begin.table, begin.current + nextGoodLocation);
    }

    // Our insertion sort, which is guaranteed to be stable, as it should be
    public static void insertion_sort(ExtMoveArrayWrapper begin, ExtMoveArrayWrapper end)
    {
        Debug.Assert(begin.table == end.table);
        Debug.Assert(begin.current <= end.current);

        for (var counter = begin.current; counter < end.current - 1; counter++)
        {
            var index = counter + 1;
            while (index > begin.current)
            {
                if (begin.table[index - 1] < begin.table[index])
                {
                    var temp = begin.table[index - 1];
                    begin.table[index - 1] = begin.table[index];
                    begin.table[index] = temp;
                }
                index--;
            }
        }
    }
}