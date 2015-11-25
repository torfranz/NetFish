using System.Runtime.CompilerServices;

internal class StateInfoWrapper
{
    internal int current;

    internal StateInfo[] table;

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    internal StateInfoWrapper()
        : this(new StateInfo[_.MAX_PLY], 0)
    {
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    internal StateInfoWrapper(StateInfo[] table, int current)
    {
        this.table = table;
        this.current = current;

        for (var i = 0; i < table.Length; i++)
        {
            table[i] = new StateInfo();
        }
    }

    internal StateInfo this[int index] => this.table[index];

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    public static StateInfoWrapper operator ++(StateInfoWrapper p)
    {
        p.current += 1;
        if (p.current == p.table.Length)
        {
            p.current = 0;
        }
        return p;
    }
}