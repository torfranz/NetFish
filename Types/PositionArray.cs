using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PositionArray
{
    private ExtMove[] table;

    public int current = 0;

    public PositionArray(ExtMove[] table)
        : this(table, 0)
    {
    }

    public PositionArray(ExtMove[] table, int current)
    {
        this.table = table;
        this.current = current;
    }

    public void set(ExtMove[] table)
    {
        this.table = table;
        this.current = 0;
    }

    public static PositionArray operator +(PositionArray p, int value)
    {
        p.current += value;
        return p;
    }

    public static bool operator ==(PositionArray p1, PositionArray p2)
    {
        return p1.table == p2.table && p1.current == p2.current;
    }

    public static bool operator !=(PositionArray p1, PositionArray p2)
    {
        return p1.table != p2.table || p1.current != p2.current;
    }

    public static PositionArray operator ++(PositionArray p)
    {
        p.current += 1;
        return p;
    }

    public static PositionArray operator --(PositionArray p)
    {
        p.current -= 1;
        return p;
    }

    public ExtMove Current
    {
        get
        {
            return table[current];
        }
    }

    public ExtMove this[int index]
    {
        get
        {
            return table[index];
        }
    }
}

