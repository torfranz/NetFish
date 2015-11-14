using System.Runtime.CompilerServices;

internal struct Depth
{
    internal const int ONE_PLY_C = 1;
    internal const int DEPTH_ZERO_C = 0;
    internal const int DEPTH_QS_CHECKS_C = 0;
    internal const int DEPTH_QS_NO_CHECKS_C = -1;
    internal const int DEPTH_QS_RECAPTURESS_C = -5;
    internal const int DEPTH_NONE_C = -6;


    internal static Depth ONE_PLY = new Depth(ONE_PLY_C);

    internal static Depth DEPTH_ZERO = new Depth(DEPTH_ZERO_C);

    internal static Depth DEPTH_QS_CHECKS = new Depth(DEPTH_QS_CHECKS_C);

    internal static Depth DEPTH_QS_NO_CHECKS = new Depth(DEPTH_QS_NO_CHECKS_C);

    internal static Depth DEPTH_QS_RECAPTURES = new Depth(DEPTH_QS_RECAPTURESS_C);

    internal static Depth DEPTH_NONE = new Depth(DEPTH_NONE_C);

    internal static Depth DEPTH_MAX = new Depth(_.MAX_PLY);

    private int Value;

    #region constructors

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Depth(int value)
    {
        Value = value;
    }

    #endregion

    #region base operators

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Depth operator +(Depth v1, Depth v2)
    {
        return new Depth(v1.Value + v2.Value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Depth operator +(Depth v1, int v2)
    {
        return new Depth(v1.Value + v2);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Depth operator +(int v1, Depth v2)
    {
        return new Depth(v1 + v2.Value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Depth operator -(Depth v1, Depth v2)
    {
        return new Depth(v1.Value - v2.Value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Depth operator *(int v1, Depth v2)
    {
        return new Depth(v1*v2.Value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Depth operator *(Depth v1, int v2)
    {
        return new Depth(v1.Value*v2);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static implicit operator int(Depth d)
    {
        return d.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator <(Depth v1, Depth v2)
    {
        return v1.Value < v2.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator >(Depth v1, Depth v2)
    {
        return v1.Value > v2.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Depth operator ++(Depth v1)
    {
        v1.Value += 1;
        return v1;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator ==(Depth v1, Depth v2)
    {
        return v1.Value == v2.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator !=(Depth v1, Depth v2)
    {
        return v1.Value != v2.Value;
    }

    #endregion

    #region extended operators

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static int operator /(Depth v1, Depth v2)
    {
        return v1.Value/v2.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Depth operator /(Depth v1, int v2)
    {
        return new Depth(v1.Value/v2);
    }

    #endregion
}