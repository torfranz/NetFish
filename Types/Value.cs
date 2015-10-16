using System.Runtime.CompilerServices;

public struct Value
{
    public const int VALUE_ZERO = 0;

    public const int VALUE_DRAW = 0;

    public const int VALUE_KNOWN_WIN = 10000;

    public const int VALUE_MATE = 32000;

    public const int VALUE_INFINITE = 32001;

    public const int VALUE_NONE = 32002;

    public const int VALUE_MATE_IN_MAX_PLY = VALUE_MATE - 2 * _.MAX_PLY;

    public const int VALUE_MATED_IN_MAX_PLY = -VALUE_MATE + 2 * _.MAX_PLY;

    public const int PawnValueMg = 198, PawnValueEg = 258;

    public const int KnightValueMg = 817, KnightValueEg = 846;

    public const int BishopValueMg = 836, BishopValueEg = 857;

    public const int RookValueMg = 1270, RookValueEg = 1281;

    public const int QueenValueMg = 2521, QueenValueEg = 2558;

    public const int MidgameLimit = 15581, EndgameLimit = 3998;

    public int value { get; }

    #region constructors

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Value(Value value)
        : this(value.value)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Value(int value)
    {
        this.value = value;
    }

    #endregion

    #region base operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value operator +(Value v1, Value v2)
    {
        return new Value(v1.value + v2.value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value operator +(Value v1, int v2)
    {
        return new Value(v1.value + v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value operator +(int v1, Value v2)
    {
        return new Value(v1 + v2.value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value operator -(Value v1, Value v2)
    {
        return new Value(v1.value - v2.value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value operator -(Value v1, int v2)
    {
        return new Value(v1.value - v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value operator -(Value v1)
    {
        return new Value(-v1.value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value operator *(int v1, Value v2)
    {
        return new Value(v1 * v2.value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value operator *(Value v1, int v2)
    {
        return new Value(v1.value * v2);
    }

    #endregion

    #region extended operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator /(Value v1, Value v2)
    {
        return v1.value / v2.value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value operator /(Value v1, int v2)
    {
        return new Value(v1.value / v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value mate_in(int ply)
    {
        return new Value(VALUE_MATE - ply);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Value mated_in(int ply)
    {
        return new Value(-VALUE_MATE + ply);
    }

    #endregion
}