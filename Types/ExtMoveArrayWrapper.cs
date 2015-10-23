public struct ExtMoveArrayWrapper
{
    public ExtMove[] table;

    public int current;

    public ExtMoveArrayWrapper(ExtMove[] table)
        : this(table, 0)
    {
    }

    public ExtMoveArrayWrapper(ExtMove[] table, int current)
    {
        this.table = table;
        this.current = current;
    }

    public void set(ExtMove[] table)
    {
        this.table = table;
        current = 0;
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

    public void setCurrentMove(Move m)
    {
        table[current] = new ExtMove(m, table[current].Value);
    }

    public Move getCurrentMove()
    {
        return table[current].Move;
    }

    public ExtMove this[int index]
    {
        get { return table[index]; }
    }
}