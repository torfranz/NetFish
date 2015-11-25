using System;
using System.Diagnostics;

#if PRIMITIVE
using ColorT = System.Int32;
using PieceTypeT = System.Int32;
using PieceT = System.Int32;
#else
internal class PieceT
{
    private readonly int Value;

    #region constructors

    internal PieceT(int value)
    {
        this.Value = value;
        Debug.Assert(this.Value >= 0 && this.Value <= 16);
        Debug.Assert(this.Value != 7);
        Debug.Assert(this.Value != 8);
    }

    #endregion

    #region base operators

    public static implicit operator int(PieceT p)
    {
        return p.Value;
    }

    public override string ToString()
    {
        return this.Value.ToString();
    }

    #endregion
}
#endif

internal static class Piece
{
#if PRIMITIVE
    internal static PieceT NO_PIECE = 0;

    internal static PieceT W_PAWN = 1;

    internal static PieceT W_KNIGHT = 2;

    internal static PieceT W_BISHOP = 3;

    internal static PieceT W_ROOK = 4;

    internal static PieceT W_QUEEN = 5;

    internal static PieceT W_KING = 6;

    internal static PieceT B_PAWN = 9;

    internal static PieceT B_KNIGHT = 10;

    internal static PieceT B_BISHOP = 11;

    internal static PieceT B_ROOK = 12;

    internal static PieceT B_QUEEN = 13;

    internal static PieceT B_KING = 14;

    
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static PieceT Create(int value)
    {
        return value;
    }

#else
    internal static PieceT NO_PIECE = new PieceT(0);

    internal static PieceT W_PAWN = new PieceT(1);

    internal static PieceT W_KNIGHT = new PieceT(2);

    internal static PieceT W_BISHOP = new PieceT(3);

    internal static PieceT W_ROOK = new PieceT(4);

    internal static PieceT W_QUEEN = new PieceT(5);

    internal static PieceT W_KING = new PieceT(6);

    internal static PieceT B_PAWN = new PieceT(9);

    internal static PieceT B_KNIGHT = new PieceT(10);

    internal static PieceT B_BISHOP = new PieceT(11);

    internal static PieceT B_ROOK = new PieceT(12);

    internal static PieceT B_QUEEN = new PieceT(13);

    internal static PieceT B_KING = new PieceT(14);

    internal static PieceT Create(int value)
    {
        switch (value)
        {
            case 0:
                return NO_PIECE;
            case 1:
                return W_PAWN;
            case 2:
                return W_KNIGHT;
            case 3:
                return W_BISHOP;
            case 4:
                return W_ROOK;
            case 5:
                return W_QUEEN;
            case 6:
                return W_KING;

            case 9:
                return B_PAWN;
            case 10:
                return B_KNIGHT;
            case 11:
                return B_BISHOP;
            case 12:
                return B_ROOK;
            case 13:
                return B_QUEEN;
            case 14:
                return B_KING;

            default:
                throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
#endif

    internal const int PIECE_NB = 16;

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    internal static PieceTypeT type_of(PieceT p)
    {
        return PieceType.Create(p & 7);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    internal static ColorT color_of(PieceT p)
    {
        return Color.Create(p >> 3);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    internal static PieceT make_piece(ColorT c, PieceTypeT pt)
    {
        return Create((c << 3) | pt);
    }
}