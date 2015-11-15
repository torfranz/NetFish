using System.Runtime.CompilerServices;

#if PRIMITIVE
using ColorT = System.Int32;
#else

internal class ColorT
{
    private readonly int Value;

#region constructors

    internal ColorT(int value)
    {
        this.Value = value;
    }
#endregion

    public static implicit operator int (ColorT c)
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
    public static ColorT Create(int value)
    {
        return value;
    }

#else
    internal static ColorT WHITE = new ColorT(0);

    internal static ColorT BLACK = new ColorT(1);
    
    public static ColorT Create(int value)
    {
        return value != 0 ? Color.BLACK : Color.WHITE;
    }
#endif

    internal const int COLOR_NB = 2;
    internal static ColorT[] AllColors = { Color.WHITE, Color.BLACK};

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static ColorT opposite(ColorT c)
    {
        return c == Color.WHITE ? Color.BLACK : Color.WHITE;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static CastlingRight CalculateCastlingRight(ColorT c, CastlingSide s)
    {
        return (CastlingRight)((int)CastlingRight.WHITE_OO << ((s == CastlingSide.QUEEN_SIDE ? 1 : 0) + 2 * c));
    }
}