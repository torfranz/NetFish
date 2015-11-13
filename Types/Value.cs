using System.Runtime.CompilerServices;

internal struct Value
{
    internal static Value VALUE_ZERO = new Value(0);

    internal static Value VALUE_DRAW = new Value(0);

    internal static Value VALUE_KNOWN_WIN = new Value(10000);

    internal static Value VALUE_MATE = new Value(32000);

    internal static Value VALUE_INFINITE = new Value(32001);

    internal static Value VALUE_NONE = new Value(32002);

    internal static Value VALUE_MATE_IN_MAX_PLY = new Value(VALUE_MATE - 2*_.MAX_PLY);

    internal static Value VALUE_MATED_IN_MAX_PLY = new Value(-VALUE_MATE + 2*_.MAX_PLY);

    internal static Value PawnValueMg = new Value(198);

    internal static Value PawnValueEg = new Value(258);

    internal static Value KnightValueMg = new Value(817);

    internal static Value KnightValueEg = new Value(846);

    internal static Value BishopValueMg = new Value(836);

    internal static Value BishopValueEg = new Value(857);

    internal static Value RookValueMg = new Value(1270);

    internal static Value RookValueEg = new Value(1281);

    internal static Value QueenValueMg = new Value(2521);

    internal static Value QueenValueEg = new Value(2558);

    internal static Value MidgameLimit = new Value(15581);

    internal static Value EndgameLimit = new Value(3998);

    internal static Value[][] PieceValue =
    {
        new[]
        {
            VALUE_ZERO, PawnValueMg, KnightValueMg, BishopValueMg, RookValueMg,
            QueenValueMg, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO,
            VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO,
            VALUE_ZERO
        },
        new[]
        {
            VALUE_ZERO, PawnValueEg, KnightValueEg, BishopValueEg, RookValueEg,
            QueenValueEg, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO,
            VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO,
            VALUE_ZERO
        }
    };

    private int value
    {
#if FORCEINLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        get;
    }

    #region constructors

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static implicit operator bool(Value b)
    {
        return b.value != 0;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Value(int value)
    {
        this.value = value;
    }

    #endregion

    #region base operators

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Value operator +(Value v1, Value v2)
    {
        return new Value(v1.value + v2.value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Value operator +(Value v1, int v2)
    {
        return new Value(v1.value + v2);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Value operator -(Value v1, Value v2)
    {
        return new Value(v1.value - v2.value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Value operator -(Value v1, int v2)
    {
        return new Value(v1.value - v2);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Value operator -(Value v1)
    {
        return new Value(-v1.value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Value operator *(int v1, Value v2)
    {
        return new Value(v1*v2.value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Value operator *(Value v1, int v2)
    {
        return new Value(v1.value*v2);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static implicit operator int(Value v)
    {
        return v.value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator ==(Value v1, Value v2)
    {
        return v1.value == v2.value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator !=(Value v1, Value v2)
    {
        return v1.value != v2.value;
    }

    #endregion

    #region extended operators

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static int operator /(Value v1, Value v2)
    {
        return v1.value/v2.value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Value operator /(Value v1, int v2)
    {
        return new Value(v1.value/v2);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public override string ToString()
    {
        return $"{value}";
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Value mate_in(int ply)
    {
        return new Value(VALUE_MATE - ply);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Value mated_in(int ply)
    {
        return new Value(-VALUE_MATE + ply);
    }

    #endregion
}