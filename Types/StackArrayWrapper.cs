public class StackArrayWrapper
{
    public int current;

    public Stack[] table;

    public StackArrayWrapper(Stack[] table)
        : this(table, 0)
    {
    }

    public StackArrayWrapper(Stack[] table, int current)
    {
        this.table = table;
        this.current = current;
    }

    public Stack this[int index] => table[index];

    public void set(Stack[] table)
    {
        this.table = table;
        current = 0;
    }

    public static StackArrayWrapper operator ++(StackArrayWrapper p)
    {
        p.current += 1;
        return p;
    }

    public static StackArrayWrapper operator --(StackArrayWrapper p)
    {
        p.current -= 1;
        return p;
    }
}