﻿using System.Runtime.CompilerServices;

internal class StackArrayWrapper
{
    internal int current;

    internal Stack[] table;

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal StackArrayWrapper(Stack[] table)
        : this(table, 0)
    {
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal StackArrayWrapper(Stack[] table, int current)
    {
        this.table = table;
        this.current = current;
    }

    internal Stack this[int index] => table[index];

    
}