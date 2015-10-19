using System.Diagnostics;
using System.Runtime.CompilerServices;

public struct Rank
{
    public const int RANK_1 = 0;

    public const int RANK_2 = 1;

    public const int RANK_3 = 2;

    public const int RANK_4 = 3;

    public const int RANK_5 = 4;

    public const int RANK_6 = 5;

    public const int RANK_7 = 6;

    public const int RANK_8 = 7;

    public const int RANK_NB = 8;

    private int Value { get; }

    #region constructors

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rank(Rank rank)
        : this(rank.Value)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rank(int value)
    {
        this.Value = value;
        Debug.Assert(this.Value >= -8 && this.Value <= 8);
    }

    #endregion

    #region base operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator +(Rank v1, Rank v2)
    {
        return new Rank(v1.Value + v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator +(Rank v1, int v2)
    {
        return new Rank(v1.Value + v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator +(int v1, Rank v2)
    {
        return new Rank(v1 + v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator -(Rank v1, Rank v2)
    {
        return new Rank(v1.Value - v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator -(Rank v1, int v2)
    {
        return new Rank(v1.Value - v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator -(Rank v1)
    {
        return new Rank(-v1.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator *(int v1, Rank v2)
    {
        return new Rank(v1 * v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator *(Rank v1, int v2)
    {
        return new Rank(v1.Value * v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator int (Rank r)
    {
        return r.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Rank v1, Rank v2)
    {
        return v1.Value == v2.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Rank v1, Rank v2)
    {
        return v1.Value != v2.Value;
    }

    public override string ToString()
    {
        return this.Value.ToString();
    }
    #endregion

    #region extended operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator /(Rank v1, Rank v2)
    {
        return v1.Value / v2.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator /(Rank v1, int v2)
    {
        return new Rank(v1.Value / v2);
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank relative_rank(Color c, Rank r)
    {
        return new Rank(r.Value ^ (c * 7));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank relative_rank(Color c, Square s)
    {
        return relative_rank(c, s.rank_of());
    }
}