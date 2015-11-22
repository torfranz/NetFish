using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#if PRIMITIVE
using RankT = System.Int32;
using ColorT = System.Int32;
using SquareT = System.Int32;
#else
internal class RankT
{
    private int Value;

#region constructors

    internal RankT(int value)
    {
        Value = value;
        Debug.Assert(this.Value >= 0 && this.Value <= 7);
    }

    #endregion

    #region base operators
    
    public static RankT operator +(RankT v1, int v2)
    {
        return Rank.Create(v1.Value + v2);
    }

    public static RankT operator -(RankT v1, RankT v2)
    {
        return Rank.Create(v1.Value - v2.Value);
    }

    public static implicit operator int (RankT r)
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
    internal const RankT RANK_1 = 0;
    internal const RankT RANK_2 = 1;
    internal const RankT RANK_3 = 2;
    internal const RankT RANK_4 = 3;
    internal const RankT RANK_5 = 4;
    internal const RankT RANK_6 = 5;
    internal const RankT RANK_7 = 6;
    internal const RankT RANK_8 = 7;

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static RankT Create(int value)
    {
        return value;
    }

#else
    internal static RankT RANK_1 = new RankT(0);

    internal static RankT RANK_2 = new RankT(1);

    internal static RankT RANK_3 = new RankT(2);

    internal static RankT RANK_4 = new RankT(3);

    internal static RankT RANK_5 = new RankT(4);

    internal static RankT RANK_6 = new RankT(5);

    internal static RankT RANK_7 = new RankT(6);

    internal static RankT RANK_8 = new RankT(7);

    public static RankT Create(int value)
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
    internal static RankT[] AllFiles = { RANK_1, RANK_2, RANK_3, RANK_4, RANK_5, RANK_6, RANK_7, RANK_8 };

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static RankT relative_rank_CtRt(ColorT c, RankT r)
    {
        return Create(r ^ (c * 7));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static RankT relative_rank_CtSt(ColorT c, SquareT s)
    {
        return relative_rank_CtRt(c, Square.rank_of(s));
    }
}