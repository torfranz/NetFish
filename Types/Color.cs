using System.Diagnostics;
using System.Runtime.CompilerServices;

internal struct Color
{
    internal const int WHITE_C = 0;
    internal const int BLACK_C = 1;
    internal const int COLOR_NB_C = 2;
    internal static Color WHITE = new Color(WHITE_C);

    internal static Color BLACK = new Color(BLACK_C);

    internal static Color COLOR_NB = new Color(COLOR_NB_C);

    internal int Value;
//    internal int Value
//    {
//#if FORCEINLINE
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//#endif
//        get;
//#if FORCEINLINE
//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//#endif
//        private set;
//    }

    #region constructors

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Color(uint value)
        : this((int) value)
    {
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Color(int value)
    {
        Value = value;
        Debug.Assert(Value >= 0 && Value <= 2);
    }

    #endregion

    #region base operators

//#if FORCEINLINE
//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//#endif
//    internal static implicit operator int(Color c)
//    {
//        return c.Value;
//    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator ==(Color v1, Color v2)
    {
        return v1.Value == v2.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator !=(Color v1, Color v2)
    {
        return v1.Value != v2.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Color operator ++(Color v1)
    {
        v1.Value += 1;
        return v1;
    }

    /*
    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    internal static Color operator -(Color v1)
    {
        return new Color(-v1.value);
    }

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    internal static Color operator *(int v1, Color v2)
    {
        return new Color(v1 * v2.value);
    }

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    internal static Color operator *(Color v1, int v2)
    {
        return new Color(v1.value * v2);
    }

    #endregion

    #region extended operators

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    internal static int operator /(Color v1, Color v2)
    {
        return v1.value / v2.value;
    }

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    internal static Color operator /(Color v1, int v2)
    {
        return new Color(v1.value / v2);
    }
    */

#if FORCEINLINE
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    public static Color operator +(Color v1, Color v2)
    {
        return new Color(v1.Value + v2.Value);
    }

#if FORCEINLINE
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    public static Color operator -(Color v1, Color v2)
    {
        return new Color(v1.Value - v2.Value);
    }

#if FORCEINLINE
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    public static Color operator -(Color v1, int v2)
    {
        return new Color(v1.Value - v2);
    }

#if FORCEINLINE
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    public static Color operator +(Color v1, int v2)
    {
        return new Color(v1.Value + v2);
    }

#if FORCEINLINE
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    public static Color operator +(int v1, Color v2)
    {
        return new Color(v1 + v2.Value);
    }
    #endregion

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Color operator ~(Color c)
    {
        return new Color(c.Value ^ BLACK.Value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static CastlingRight operator |(Color c, CastlingSide s)
    {
        return (CastlingRight) ((int) CastlingRight.WHITE_OO << ((s == CastlingSide.QUEEN_SIDE ? 1 : 0) + 2*c.Value));
    }
}