using System.Runtime.CompilerServices;

internal struct Rank
{
    internal const int RANK_1C = 0;

    internal const int RANK_2C = 1;

    internal const int RANK_3C = 2;

    internal const int RANK_4C = 3;

    internal const int RANK_5C = 4;

    internal const int RANK_6C = 5;

    internal const int RANK_7C = 6;

    internal const int RANK_8C = 7;

    internal static Rank RANK_1 = new Rank(RANK_1C);

    internal static Rank RANK_2 = new Rank(RANK_2C);

    internal static Rank RANK_3 = new Rank(RANK_3C);

    internal static Rank RANK_4 = new Rank(RANK_4C);

    internal static Rank RANK_5 = new Rank(RANK_5C);

    internal static Rank RANK_6 = new Rank(RANK_6C);

    internal static Rank RANK_7 = new Rank(RANK_7C);

    internal static Rank RANK_8 = new Rank(RANK_8C);

    internal static Rank RANK_NB = new Rank(8);

    internal int Value
    {
#if FORCEINLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        get;
#if FORCEINLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
       private set;
    }

    #region constructors

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Rank(uint value)
        : this((int) value)
    {
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Rank(int value)
    {
        Value = value;
        //Debug.Assert(this.Value >= -8 && this.Value <= 8);
    }

    #endregion

    #region base operators

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Rank operator +(Rank v1, int v2)
    {
        return new Rank(v1.Value + v2);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Rank operator +(int v1, Rank v2)
    {
        return new Rank(v1 + v2.Value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Rank operator -(Rank v1, Rank v2)
    {
        return new Rank(v1.Value - v2.Value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Rank operator -(Rank v1, int v2)
    {
        return new Rank(v1.Value - v2);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Rank operator *(int v1, Rank v2)
    {
        return new Rank(v1*v2.Value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static implicit operator int(Rank r)
    {
        return r.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator ==(Rank v1, Rank v2)
    {
        return v1.Value == v2.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator !=(Rank v1, Rank v2)
    {
        return v1.Value != v2.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Rank operator ++(Rank v1)
    {
        v1.Value += 1;
        return v1;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Rank operator --(Rank v1)

    {
        v1.Value -= 1;
        return v1;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public override string ToString()
    {
        return Value.ToString();
    }

    #endregion

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Rank relative_rank(Color c, Rank r)
    {
        return new Rank(r.Value ^ (c.Value * 7));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Rank relative_rank(Color c, Square s)
    {
        return relative_rank(c, Square.rank_of(s));
    }
}