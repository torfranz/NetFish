using System.Runtime.CompilerServices;

internal class Color
{
    internal const int WHITE_C = 0;
    internal const int BLACK_C = 1;
    internal const int COLOR_NB_C = 2;
    internal static Color WHITE = new Color(WHITE_C);

    internal static Color BLACK = new Color(BLACK_C);

    internal readonly int ValueMe;
    internal readonly int ValueThem;

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Color Create(uint value)
    {
        return value != 0 ? Color.BLACK : Color.WHITE;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Color Create(int value)
    {
        return value != 0 ? Color.BLACK : Color.WHITE;
    }
    #region constructors

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private Color(int valueMe)
    {
        this.ValueMe = valueMe;
        this.ValueThem = valueMe ^ 1;
    }
    #endregion

    #if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Color operator ~(Color c)
    {
        return c == WHITE ? BLACK : WHITE;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static CastlingRight operator |(Color c, CastlingSide s)
    {
        return (CastlingRight) ((int) CastlingRight.WHITE_OO << ((s == CastlingSide.QUEEN_SIDE ? 1 : 0) + 2*c.ValueMe));
    }
}