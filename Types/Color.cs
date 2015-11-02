using System.Diagnostics;

public struct Color
{
    public static Color WHITE = new Color(0);

    public static Color BLACK = new Color(1);

    public static Color NO_COLOR = new Color(2);

    public static Color COLOR_NB = new Color(2);

    private int Value { get; }

    #region constructors

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Color(uint value)
        : this((int)value)
    {
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Color(int value)
    {
        this.Value = value;
        Debug.Assert(this.Value >= 0 && this.Value <= 2);
    }

    #endregion

    #region base operators

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

    public static implicit operator int(Color c)
    {
        return c.Value;
    }

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
        return new Color(v1.Value + 1);
    }

    /*
    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    public static Color operator -(Color v1)
    {
        return new Color(-v1.value);
    }

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    public static Color operator *(int v1, Color v2)
    {
        return new Color(v1 * v2.value);
    }

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    public static Color operator *(Color v1, int v2)
    {
        return new Color(v1.value * v2);
    }

    #endregion

    #region extended operators

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    public static int operator /(Color v1, Color v2)
    {
        return v1.value / v2.value;
    }

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    public static Color operator /(Color v1, int v2)
    {
        return new Color(v1.value / v2);
    }
    */

    #endregion

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Color operator ~(Color c)
    {
        return new Color(c.Value ^ BLACK);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static CastlingRight operator |(Color c, CastlingSide s)
    {
        return (CastlingRight)((int)CastlingRight.WHITE_OO << ((s == CastlingSide.QUEEN_SIDE ? 1 : 0) + 2 * c.Value));
    }
}