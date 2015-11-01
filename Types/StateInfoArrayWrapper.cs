public class StateInfoWrapper
{
    public StateInfo[] table;

    public int current;

    public StateInfoWrapper(StateInfo[] table)
        : this(table, 0)
    {
    }

    public StateInfoWrapper(StateInfo[] table, int current)
    {
        this.table = table;
        this.current = current;

        for (var i = 0; i < 102; i++)
        {
            table[i] = new StateInfo();
        }
    }

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

    public StateInfo this[int index] => table[index];
}