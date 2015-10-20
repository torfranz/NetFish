using System.Diagnostics;
using System.Runtime.CompilerServices;

public struct Rank
{
    public static Rank RANK_1 = new Rank(0);

    public static Rank RANK_2 = new Rank(1);

    public static Rank RANK_3 = new Rank(2);

    public static Rank RANK_4 = new Rank(3);

    public static Rank RANK_5 = new Rank(4);

    public static Rank RANK_6 = new Rank(5);

    public static Rank RANK_7 = new Rank(6);

    public static Rank RANK_8 = new Rank(7);

    public static Rank RANK_NB = new Rank(8);

    private int Value { get; }

    #region constructors

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rank(uint value)
        : this((int)value)
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
    public static implicit operator int(Rank r)
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rank operator ++(Rank v1)
    {
        return new Rank(v1.Value + 1);
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