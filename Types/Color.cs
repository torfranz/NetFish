using System.Diagnostics;
using System.Runtime.CompilerServices;

public struct Color
{
    public const int WHITE = 0;

    public const int BLACK = 1;

    public const int NO_COLOR = 2;

    public const int COLOR_NB = 2;

    private int Value { get; }

    #region constructors

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color(Color value)
        : this(value.Value)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color(int value)
    {
        this.Value = value;
        Debug.Assert(this.Value >= 0 && this.Value <= 2);
    }

    #endregion

    #region base operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color operator +(Color v1, Color v2)
    {
        return new Color(v1.Value + v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color operator +(Color v1, int v2)
    {
        return new Color(v1.Value + v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color operator +(int v1, Color v2)
    {
        return new Color(v1 + v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color operator -(Color v1, Color v2)
    {
        return new Color(v1.Value - v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color operator -(Color v1, int v2)
    {
        return new Color(v1.Value - v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator int (Color c)
    {
        return c.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Color v1, Color v2)
    {
        return v1.Value == v2.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Color v1, Color v2)
    {
        return v1.Value != v2.Value;
    }
    /*
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color operator -(Color v1)
    {
        return new Color(-v1.value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color operator *(int v1, Color v2)
    {
        return new Color(v1 * v2.value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color operator *(Color v1, int v2)
    {
        return new Color(v1.value * v2);
    }

    #endregion

    #region extended operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator /(Color v1, Color v2)
    {
        return v1.value / v2.value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color operator /(Color v1, int v2)
    {
        return new Color(v1.value / v2);
    }
    */

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color operator ~(Color c)
    {
        return new Color(c.Value ^ BLACK);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CastlingRight operator |(Color c, CastlingSide s)
    {
        return (CastlingRight)((int)CastlingRight.WHITE_OO << ((s == CastlingSide.QUEEN_SIDE ? 1 : 0) + 2 * c.Value));
    }
}