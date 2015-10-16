using System.Runtime.CompilerServices;

public struct Depth
{
    public const int ONE_PLY = 1;

    public const int DEPTH_ZERO = 0;

    public const int DEPTH_QS_CHECKS = 0;

    public const int DEPTH_QS_NO_CHECKS = -1;

    public const int DEPTH_QS_RECAPTURES = -5;

    public const int DEPTH_NONE = -6;

    public const int DEPTH_MAX = _.MAX_PLY;

    public int Value { get; }

    #region constructors

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Depth(Depth depth)
        : this(depth.Value)
    {
    }

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