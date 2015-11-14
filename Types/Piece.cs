using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

internal class Piece
{
    internal const int NO_PIECE_C = 0;
    internal const int W_PAWN_C = 1;
    internal const int W_KNIGHT_C = 2;
    internal const int W_BISHOP_C = 3;
    internal const int W_ROOK_C = 4;
    internal const int W_QUEEN_C = 5;
    internal const int W_KING_C = 6;
    
    internal const int B_PAWN_C = 9;
    internal const int B_KNIGHT_C = 10;
    internal const int B_BISHOP_C = 11;
    internal const int B_ROOK_C = 12;
    internal const int B_QUEEN_C = 13;
    internal const int B_KING_C = 14;
    internal const int PIECE_NB_C = 16;


    internal static Piece NO_PIECE = new Piece(NO_PIECE_C);

    internal static Piece W_PAWN = new Piece(W_PAWN_C);

    internal static Piece W_KNIGHT = new Piece(W_KNIGHT_C);

    internal static Piece W_BISHOP = new Piece(W_BISHOP_C);

    internal static Piece W_ROOK = new Piece(W_ROOK_C);

    internal static Piece W_QUEEN = new Piece(W_QUEEN_C);

    internal static Piece W_KING = new Piece(W_KING_C);

    internal static Piece B_PAWN = new Piece(B_PAWN_C);

    internal static Piece B_KNIGHT = new Piece(B_KNIGHT_C);

    internal static Piece B_BISHOP = new Piece(B_BISHOP_C);

    internal static Piece B_ROOK = new Piece(B_ROOK_C);

    internal static Piece B_QUEEN = new Piece(B_QUEEN_C);

    internal static Piece B_KING = new Piece(B_KING_C);

    private readonly int Value;

    #region constructors

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Piece Create(int value)
    {
        switch (value)
        {
            case NO_PIECE_C: return NO_PIECE;
            case W_PAWN_C: return W_PAWN;
            case W_KNIGHT_C: return W_KNIGHT;
            case W_BISHOP_C: return W_BISHOP;
            case W_ROOK_C: return W_ROOK;
            case W_QUEEN_C: return W_QUEEN;
            case W_KING_C: return W_KING;

            case B_PAWN_C: return B_PAWN;
            case B_KNIGHT_C: return B_KNIGHT;
            case B_BISHOP_C: return B_BISHOP;
            case B_ROOK_C: return B_ROOK;
            case B_QUEEN_C: return B_QUEEN;
            case B_KING_C: return B_KING;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private Piece(int value)
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
    /*
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
    */
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public override string ToString()
    {
        return Value.ToString();
    }

    #endregion

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static PieceType type_of(Piece p)
    {
        return PieceType.Create(p.Value & 7);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Color color_of(Piece p)
    {
        return Color.Create(p.Value >> 3);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Piece make_piece(Color c, PieceType pt)
    {
        return make_piece(c.ValueMe, pt);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Piece make_piece(int c, PieceType pt)
    {
        return Piece.Create((c << 3) | pt);
    }
}