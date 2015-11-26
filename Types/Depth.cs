#if PRIMITIVE
using System.Runtime.CompilerServices;

using DepthT = System.Int32;
#else

internal struct DepthT
{
    private int Value;

    #region constructors

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal DepthT(int value)
    {
        Value = value;
    }

    #endregion

    #region base operators

    public static DepthT operator +(DepthT v1, DepthT v2)
    {
        return Depth.Create(v1.Value + v2.Value);
    }
    public static DepthT operator +(DepthT v1, int v2)
    {
        return Depth.Create(v1.Value + v2);
    }

    public static DepthT operator +(int v1, DepthT v2)
    {
        return Depth.Create(v1 + v2.Value);
    }

    public static DepthT operator -(DepthT v1, DepthT v2)
    {
        return Depth.Create(v1.Value - v2.Value);
    }

    public static DepthT operator *(int v1, DepthT v2)
    {
        return Depth.Create(v1 * v2.Value);
    }

    public static DepthT operator *(DepthT v1, int v2)
    {
        return Depth.Create(v1.Value * v2);
    }

    public static implicit operator int (DepthT d)
    {
        return d.Value;
    }

    public static bool operator <(DepthT v1, DepthT v2)
    {
        return v1.Value < v2.Value;
    }

    public static bool operator >(DepthT v1, DepthT v2)
    {
        return v1.Value > v2.Value;
    }

    public static DepthT operator ++(DepthT v1)
    {
        v1.Value += 1;
        return v1;
    }

    public static bool operator ==(DepthT v1, DepthT v2)
    {
        return v1.Value == v2.Value;
    }

    public static bool operator !=(DepthT v1, DepthT v2)
    {
        return v1.Value != v2.Value;
    }

    #endregion

    #region extended operators

    public static int operator /(DepthT v1, DepthT v2)
    {
        return v1.Value / v2.Value;
    }

    public static DepthT operator /(DepthT v1, int v2)
    {
        return Depth.Create(v1.Value / v2);
    }

    #endregion
}
#endif

internal static class Depth
{
#if PRIMITIVE
    internal const DepthT ONE_PLY = 1;

    internal const DepthT DEPTH_ZERO = 0;

    internal const DepthT DEPTH_QS_CHECKS = 0;

    internal const DepthT DEPTH_QS_NO_CHECKS = -1;

    internal const DepthT DEPTH_QS_RECAPTURES = -5;

    internal const DepthT DEPTH_NONE = -6;

    internal const DepthT DEPTH_MAX = _.MAX_PLY;

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static DepthT Create(int value)
    {
        return value;
    }

#else
    internal static DepthT ONE_PLY = Depth.Create(1);

    internal static DepthT DEPTH_ZERO = Depth.Create(0);

    internal static DepthT DEPTH_QS_CHECKS = Depth.Create(0);

    internal static DepthT DEPTH_QS_NO_CHECKS = Depth.Create(-1);

    internal static DepthT DEPTH_QS_RECAPTURES = Depth.Create(-5);

    internal static DepthT DEPTH_NONE = Depth.Create(-6);

    internal static DepthT DEPTH_MAX = Depth.Create(_.MAX_PLY);
    public static DepthT Create(int value)
    {
        return new DepthT(value);
    }
#endif

}