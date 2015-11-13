using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

internal class PieceType
{
    internal const int NO_PIECE_TYPE_C = 0;
    internal const int ALL_PIECES_C = 0;
    internal const int PIECE_TYPE_NB_C = 8;
    internal const int PAWN_C = 1;
    internal const int KNIGHT_C = 2;
    internal const int BISHOP_C = 3;

    internal const int ROOK_C = 4;

    internal const int QUEEN_C = 5;

    internal const int KING_C = 6;

    internal static PieceType PAWN = new PieceType(PAWN_C);

    internal static PieceType KNIGHT = new PieceType(KNIGHT_C);

    internal static PieceType BISHOP = new PieceType(BISHOP_C);

    internal static PieceType ROOK = new PieceType(ROOK_C);

    internal static PieceType QUEEN = new PieceType(QUEEN_C);

    internal static PieceType KING = new PieceType(KING_C);

    internal static PieceType ALL_PIECES = new PieceType(ALL_PIECES_C);

    private int Value;
   
    #region constructors

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static PieceType Create(int value)
    {
        switch (value)
        {
            case ALL_PIECES_C:
                return ALL_PIECES;
            case BISHOP_C:
                return BISHOP;
            case KING_C:
                return KING;
            case KNIGHT_C:
                return KNIGHT;
            case PAWN_C:
                return PAWN;
            case ROOK_C:
                return ROOK;
            case QUEEN_C:
                return QUEEN;
            default: throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private PieceType(int value)
    {
        Value = value;
        Debug.Assert(Value >= 0 && Value <= 8);
    }
    #endregion

    #region base operators

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static PieceType operator +(PieceType v1, int v2)
    {
        return PieceType.Create(v1.Value + v2);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static explicit operator int(PieceType pt)
    {
        return pt.Value;
    }
    /*
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator ==(PieceType v1, PieceType v2)
    {
        return v1.Value == v2.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator !=(PieceType v1, PieceType v2)
    {
        return v1.Value != v2.Value;
    }
    */

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static explicit operator bool(PieceType pt)
    {
        return pt.Value != 0;
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
    internal static PieceType operator -(PieceType v1)
    {
        return new PieceType(-v1.value);
    }

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    internal static PieceType operator *(int v1, PieceType v2)
    {
        return new PieceType(v1 * v2.value);
    }

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    internal static PieceType operator *(PieceType v1, int v2)
    {
        return new PieceType(v1.value * v2);
    }

    #endregion

    #region extended operators

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    internal static int operator /(PieceType v1, PieceType v2)
    {
        return v1.value / v2.value;
    }

    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    internal static PieceType operator /(PieceType v1, int v2)
    {
        return new PieceType(v1.value / v2);
    }
    */

    #endregion
}