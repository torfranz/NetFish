
#if PRIMITIVE
using ValueT = System.Int32;
#else
internal struct ValueT
{
    private readonly int value;

    #region constructors

    internal ValueT(int value)
    {
        this.value = value;
    }

    #endregion

    #region base operators

    public static ValueT operator +(ValueT v1, ValueT v2)
    {
        return Value.Create(v1.value + v2.value);
    }

    public static ValueT operator +(ValueT v1, int v2)
    {
        return Value.Create(v1.value + v2);
    }

    public static ValueT operator -(ValueT v1, ValueT v2)
    {
        return Value.Create(v1.value - v2.value);
    }

    public static ValueT operator -(ValueT v1, int v2)
    {
        return Value.Create(v1.value - v2);
    }

    public static ValueT operator -(ValueT v1)
    {
        return Value.Create(-v1.value);
    }

    public static ValueT operator *(int v1, ValueT v2)
    {
        return Value.Create(v1 * v2.value);
    }

    public static ValueT operator *(ValueT v1, int v2)
    {
        return Value.Create(v1.value * v2);
    }

    public static implicit operator int(ValueT v)
    {
        return v.value;
    }

    public static bool operator ==(ValueT v1, ValueT v2)
    {
        return v1.value == v2.value;
    }

    public static bool operator !=(ValueT v1, ValueT v2)
    {
        return v1.value != v2.value;
    }

    #endregion

    #region extended operators

    public static int operator /(ValueT v1, ValueT v2)
    {
        return v1.value / v2.value;
    }

    public static ValueT operator /(ValueT v1, int v2)
    {
        return Value.Create(v1.value / v2);
    }

    public override string ToString()
    {
        return $"{this.value}";
    }

    #endregion
}
#endif

internal static class Value
{
#if PRIMITIVE
    internal const ValueT VALUE_ZERO = 0;

    internal const ValueT VALUE_DRAW = 0;

    internal const ValueT VALUE_KNOWN_WIN = 10000;

    internal const ValueT VALUE_MATE = 32000;

    internal const ValueT VALUE_INFINITE = 32001;

    internal const ValueT VALUE_NONE = 32002;

    internal const ValueT VALUE_MATE_IN_MAX_PLY = VALUE_MATE - 2*_.MAX_PLY;

    internal const ValueT VALUE_MATED_IN_MAX_PLY = -VALUE_MATE + 2*_.MAX_PLY;

    internal const ValueT PawnValueMg = 198;

    internal const ValueT PawnValueEg = 258;

    internal const ValueT KnightValueMg = 817;

    internal const ValueT KnightValueEg = 846;

    internal const ValueT BishopValueMg = 836;

    internal const ValueT BishopValueEg = 857;

    internal const ValueT RookValueMg = 1270;

    internal const ValueT RookValueEg = 1281;

    internal const ValueT QueenValueMg = 2521;

    internal const ValueT QueenValueEg = 2558;

    internal const ValueT MidgameLimit = 15581;

    internal const ValueT EndgameLimit = 3998;

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static ValueT Create(int value)
    {
        return value;
    }

#else
    internal static ValueT VALUE_ZERO = Create(0);

    internal static ValueT VALUE_DRAW = Create(0);

    internal static ValueT VALUE_KNOWN_WIN = Create(10000);

    internal static ValueT VALUE_MATE = Create(32000);

    internal static ValueT VALUE_INFINITE = Create(32001);

    internal static ValueT VALUE_NONE = Create(32002);

    internal static ValueT VALUE_MATE_IN_MAX_PLY = Create(VALUE_MATE - 2 * _.MAX_PLY);

    internal static ValueT VALUE_MATED_IN_MAX_PLY = Create(-VALUE_MATE + 2 * _.MAX_PLY);

    internal static ValueT PawnValueMg = Create(198);

    internal static ValueT PawnValueEg = Create(258);

    internal static ValueT KnightValueMg = Create(817);

    internal static ValueT KnightValueEg = Create(846);

    internal static ValueT BishopValueMg = Create(836);

    internal static ValueT BishopValueEg = Create(857);

    internal static ValueT RookValueMg = Create(1270);

    internal static ValueT RookValueEg = Create(1281);

    internal static ValueT QueenValueMg = Create(2521);

    internal static ValueT QueenValueEg = Create(2558);

    internal static ValueT MidgameLimit = Create(15581);

    internal static ValueT EndgameLimit = Create(3998);

    internal static ValueT Create(int value)
    {
        return new ValueT(value);
    }
#endif

    internal static ValueT[][] PieceValue =
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

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    internal static ValueT mate_in(int ply)
    {
        return Create(VALUE_MATE - ply);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    internal static ValueT mated_in(int ply)
    {
        return Create(-VALUE_MATE + ply);
    }
}