using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct PositionArray
{
    public ExtMove[] table;

    public int last;

    public PositionArray(ExtMove[] table)
        : this(table, 0)
    {
        this.last = last;
    }

    public PositionArray(ExtMove[] table, int current)
    {
        this.table = table;
        this.last = current;
    }

    public void set(ExtMove[] table)
    {
        this.table = table;
        this.last = 0;
    }

    public static PositionArray operator +(PositionArray p, int value)
    {
        p.last += value;
        return p;
    }

    public static bool operator ==(PositionArray p1, PositionArray p2)
    {
        return p1.table == p2.table && p1.last == p2.last;
    }

    public static bool operator !=(PositionArray p1, PositionArray p2)
    {
        return p1.table != p2.table || p1.last != p2.last;
    }

    public static PositionArray operator ++(PositionArray p)
    {
        p.last += 1;
        return p;
    }

    public static PositionArray operator --(PositionArray p)
    {
        p.last -= 1;
        return p;
    }

    public void setCurrentMove(Move m)
    {
        table[last - 1] = new ExtMove(m, table[last].Value);
    }

    public Move getCurrentMove()
    {
        return table[last - 1].Move;
    }

    public ExtMove this[int index]
    {
        get
        {
            return table[index];
        }
    }
}

