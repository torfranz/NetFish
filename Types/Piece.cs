using System.Diagnostics;
using System.Runtime.CompilerServices;

internal struct Piece
{
    internal static Piece NO_PIECE = new Piece(0);

    internal static Piece W_PAWN = new Piece(1);

    internal static Piece W_KNIGHT = new Piece(2);

    internal static Piece W_BISHOP = new Piece(3);

    internal static Piece W_ROOK = new Piece(4);

    internal static Piece W_QUEEN = new Piece(5);

    internal static Piece W_KING = new Piece(6);

    internal static Piece B_PAWN = new Piece(9);

    internal static Piece B_KNIGHT = new Piece(10);

    internal static Piece B_BISHOP = new Piece(11);

    internal static Piece B_ROOK = new Piece(12);

    internal static Piece B_QUEEN = new Piece(13);

    internal static Piece B_KING = new Piece(14);

    internal static Piece PIECE_NB = new Piece(16);

    private int Value
    {
#if FORCEINLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        get;
#if FORCEINLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        set;
    }

        #region constructors

#if FORCEINLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Piece(Piece value)
        : this(value.Value)
    {
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Piece(int value)
    {
        Value = value;
        Debug.Assert(Value >= 0 && Value <= 16);
        Debug.Assert(Value != 7);
        Debug.Assert(Value != 8);
        Debug.Assert(Value != 15);
    }

    #endregion

    #region base operators

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static implicit operator int(Piece p)
    {
        return p.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator ==(Piece v1, Piece v2)
    {
        return v1.Value == v2.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator !=(Piece v1, Piece v2)
    {
        return v1.Value != v2.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Piece operator ++(Piece v1)
    {
        v1.Value += 1;
        return v1;
    }
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public override string ToString()
    {
        return Value.ToString();
    }

    /*
    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    internal static Piece operator -(Piece v1)
    {
        return new Piece(-v1.value);
    }

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    internal static Piece operator *(int v1, Piece v2)
    {
        return new Piece(v1 * v2.value);
    }

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    internal static Piece operator *(Piece v1, int v2)
    {
        return new Piece(v1.value * v2);
    }

    #endregion

    #region extended operators

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    internal static int operator /(Piece v1, Piece v2)
    {
        return v1.value / v2.value;
    }

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    internal static Piece operator /(Piece v1, int v2)
    {
        return new Piece(v1.value / v2);
    }
    */

    #endregion

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static PieceType type_of(Piece p)
    {
        return new PieceType(p.Value & 7);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Color color_of(Piece p)
    {
        return new Color(p.Value >> 3);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Piece make_piece(Color c, PieceType pt)
    {
        return make_piece(c.Value, pt);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Piece make_piece(int c, PieceType pt)
    {
        return new Piece((c << 3) | pt);
    }
}