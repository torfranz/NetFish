using System.Runtime.CompilerServices;

public struct Depth
{
    public static Depth ONE_PLY = new Depth(1);

    public static Depth DEPTH_ZERO = new Depth(0);

    public static Depth DEPTH_QS_CHECKS = new Depth(0);

    public static Depth DEPTH_QS_NO_CHECKS = new Depth(-1);

    public static Depth DEPTH_QS_RECAPTURES = new Depth(-5);

    public static Depth DEPTH_NONE = new Depth(-6);

    public static Depth DEPTH_MAX = new Depth(_.MAX_PLY);

    private int Value { get; }

    #region constructors

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Depth(int value)
    {
        this.Value = value;
    }

    #endregion

    #region base operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Depth operator +(Depth v1, Depth v2)
    {
        return new Depth(v1.Value + v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Depth operator +(Depth v1, int v2)
    {
        return new Depth(v1.Value + v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Depth operator +(int v1, Depth v2)
    {
        return new Depth(v1 + v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Depth operator -(Depth v1, Depth v2)
    {
        return new Depth(v1.Value - v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Depth operator -(Depth v1, int v2)
    {
        return new Depth(v1.Value - v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Depth operator -(Depth v1)
    {
        return new Depth(-v1.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Depth operator *(int v1, Depth v2)
    {
        return new Depth(v1 * v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Depth operator *(Depth v1, int v2)
    {
        return new Depth(v1.Value * v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator int(Depth d)
    {
        return d.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Depth v1, Depth v2)
    {
        return v1.Value == v2.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Depth v1, Depth v2)
    {
        return v1.Value != v2.Value;
    }

    #endregion

    #region extended operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator /(Depth v1, Depth v2)
    {
        return v1.Value / v2.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Depth operator /(Depth v1, int v2)
    {
        return new Depth(v1.Value / v2);
    }

    #endregion
}