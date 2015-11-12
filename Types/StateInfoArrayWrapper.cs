﻿public class StateInfoWrapper
{
    public int current;

    public StateInfo[] table;

    public StateInfoWrapper()
        : this(new StateInfo[_.MAX_PLY], 0)
    {
    }

    public StateInfoWrapper(StateInfo[] table, int current)
    {
        this.table = table;
        this.current = current;

        for (var i = 0; i < table.Length; i++)
        {
            table[i] = new StateInfo();
        }
    }

    public StateInfo this[int index] => table[index];

    public static StateInfoWrapper operator ++(StateInfoWrapper p)
    {
        p.current += 1;
        if (p.current == p.table.Length)
        {
            p.current = 0;
        }
        return p;
    }

    public static StateInfoWrapper operator --(StateInfoWrapper p)
    {
        p.current -= 1;
        return p;
    }
}