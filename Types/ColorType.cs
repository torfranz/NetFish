using System.Runtime.CompilerServices;

#if PRIMITIVE
using ColorType = System.Int32;
#else

internal class ColorType
{
    private readonly int Value;

#region constructors

    internal ColorType(int value)
    {
        this.Value = value;
    }
#endregion

    public static implicit operator int (ColorType c)
    {
        return c.Value;
    }
}
#endif

internal static class Color
{

#if PRIMITIVE
    internal const int WHITE = 0;
    internal const int BLACK = 1;
    
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static ColorType Create(int value)
    {
        return value;
    }

#else
    internal static ColorType WHITE = new ColorType(0);

    internal static ColorType BLACK = new ColorType(1);
    
    public static ColorType Create(int value)
    {
        return value != 0 ? Color.BLACK : Color.WHITE;
    }
#endif

    internal const int COLOR_NB = 2;
    internal static ColorType[] AllColors = { Color.WHITE, Color.BLACK};

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static ColorType opposite(ColorType c)
    {
        return c == Color.WHITE ? Color.BLACK : Color.WHITE;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static CastlingRight CalculateCastlingRight(ColorType c, CastlingSide s)
    {
        return (CastlingRight)((int)CastlingRight.WHITE_OO << ((s == CastlingSide.QUEEN_SIDE ? 1 : 0) + 2 * c));
    }
}