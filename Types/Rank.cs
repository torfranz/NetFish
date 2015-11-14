using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

internal class Rank
{
    internal const int RANK_1_C = 0;

    internal const int RANK_2_C = 1;

    internal const int RANK_3_C = 2;

    internal const int RANK_4_C = 3;

    internal const int RANK_5_C = 4;

    internal const int RANK_6_C = 5;

    internal const int RANK_7_C = 6;

    internal const int RANK_8_C = 7;

    internal const int RANK_NB_C = 8;

    internal static Rank RANK_1 = new Rank(RANK_1_C);

    internal static Rank RANK_2 = new Rank(RANK_2_C);

    internal static Rank RANK_3 = new Rank(RANK_3_C);

    internal static Rank RANK_4 = new Rank(RANK_4_C);

    internal static Rank RANK_5 = new Rank(RANK_5_C);

    internal static Rank RANK_6 = new Rank(RANK_6_C);

    internal static Rank RANK_7 = new Rank(RANK_7_C);

    internal static Rank RANK_8 = new Rank(RANK_8_C);

    private static Rank RANK_NB = new Rank(RANK_NB_C);

    private int Value;

    #region constructors

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Rank Create(int value)
    {
        switch (value)
        {
            case RANK_1_C: return RANK_1;
            case RANK_2_C: return RANK_2;
            case RANK_3_C: return RANK_3;
            case RANK_4_C: return RANK_4;
            case RANK_5_C: return RANK_5;
            case RANK_6_C: return RANK_6;
            case RANK_7_C: return RANK_7;
            case RANK_8_C: return RANK_8;
            case RANK_NB_C: return RANK_NB;
            default: throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private Rank(int value)
    {
        Value = value;
        Debug.Assert(this.Value >= RANK_1_C && this.Value <= RANK_NB_C);
    }

    #endregion

    #region base operators

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Rank operator +(Rank v1, int v2)
    {
        return Rank.Create(v1.Value + v2);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Rank operator -(Rank v1, Rank v2)
    {
        return Rank.Create(v1.Value - v2.Value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static explicit operator int(Rank r)
    {
        return r.Value;
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
        return Rank.Create(r.Value ^ (c.ValueMe * 7));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Rank relative_rank(Color c, Square s)
    {
        return relative_rank(c, Square.rank_of(s));
    }
}