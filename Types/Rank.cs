using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#if PRIMITIVE
using RankType = System.Int32;
#else
internal class RankType
{
    private int Value;

#region constructors

    internal RankType(int value)
    {
        Value = value;
        Debug.Assert(this.Value >= 0 && this.Value <= 7);
    }

#endregion

#region base operators
    public static RankType operator +(RankType v1, int v2)
    {
        return Rank.Create(v1.Value + v2);
    }

    public static RankType operator -(RankType v1, RankType v2)
    {
        return Rank.Create(v1.Value - v2.Value);
    }

    public static implicit operator int (RankType r)
    {
        return r.Value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

#endregion

}
#endif

internal static class Rank
{

#if PRIMITIVE
    internal const int RANK_1 = 0;
    internal const int RANK_2 = 1;
    internal const int RANK_3 = 2;
    internal const int RANK_4 = 3;
    internal const int RANK_5 = 4;
    internal const int RANK_6 = 5;
    internal const int RANK_7 = 6;
    internal const int RANK_8 = 7;

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static RankType Create(int value)
    {
        return value;
    }

#else
    internal static RankType RANK_1 = new RankType(0);

    internal static RankType RANK_2 = new RankType(1);

    internal static RankType RANK_3 = new RankType(2);

    internal static RankType RANK_4 = new RankType(3);

    internal static RankType RANK_5 = new RankType(4);

    internal static RankType RANK_6 = new RankType(5);

    internal static RankType RANK_7 = new RankType(6);

    internal static RankType RANK_8 = new RankType(7);

    public static RankType Create(int value)
    {
        switch (value)
        {
            case 0:
                return RANK_1;
            case 1:
                return RANK_2;
            case 2:
                return RANK_3;
            case 3:
                return RANK_4;
            case 4:
                return RANK_5;
            case 5:
                return RANK_6;
            case 6:
                return RANK_7;
            case 7:
                return RANK_8;
            default:
                throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
#endif

    internal const int RANK_NB = 8;
    internal static RankType[] AllFiles = { RANK_1, RANK_2, RANK_3, RANK_4, RANK_5, RANK_6, RANK_7, RANK_8 };

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static RankType relative_rank(Color c, RankType r)
    {
        return Rank.Create(r ^ (c.ValueMe * 7));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static RankType relative_rank(Color c, Square s)
    {
        return relative_rank(c, Square.rank_of(s));
    }
}