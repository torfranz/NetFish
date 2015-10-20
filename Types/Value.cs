using System.Runtime.CompilerServices;

public struct Value
{
    public static Value VALUE_ZERO = new Value(0);

    public static Value VALUE_DRAW = new Value(0);

    public static Value VALUE_KNOWN_WIN = new Value(10000);

    public static Value VALUE_MATE = new Value(32000);

    public static Value VALUE_INFINITE = new Value(32001);

    public static Value VALUE_NONE = new Value(32002);

    public static Value VALUE_MATE_IN_MAX_PLY = new Value(VALUE_MATE - 2 * _.MAX_PLY);

    public static Value VALUE_MATED_IN_MAX_PLY = new Value(-VALUE_MATE + 2 * _.MAX_PLY);

    public static Value PawnValueMg = new Value(198);

    public static Value PawnValueEg = new Value(258);

    public static Value KnightValueMg = new Value(817);

    public static Value KnightValueEg = new Value(846);

    public static Value BishopValueMg = new Value(836);

    public static Value BishopValueEg = new Value(857);

    public static Value RookValueMg = new Value(1270);

    public static Value RookValueEg = new Value(1281);

    public static Value QueenValueMg = new Value(2521);

    public static Value QueenValueEg = new Value(2558);

    public static Value MidgameLimit = new Value(15581);

    public static Value EndgameLimit = new Value(3998);

    public static Value[][] PieceValue = {
        new[]{ VALUE_ZERO, PawnValueMg, KnightValueMg, BishopValueMg, RookValueMg, QueenValueMg, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO},
        new[]{ VALUE_ZERO, PawnValueEg, KnightValueEg, BishopValueEg, RookValueEg, QueenValueEg, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO, VALUE_ZERO } };

    private int value { get; }

    #region constructors

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator int(Value v)
    {
        return v.value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Value v1, Value v2)
    {
        return v1.value == v2.value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Value v1, Value v2)
    {
        return v1.value != v2.value;
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