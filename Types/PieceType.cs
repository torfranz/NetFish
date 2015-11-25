using System;
using System.Diagnostics;

#if PRIMITIVE
using PieceTypeT = System.Int32;
#else
internal class PieceTypeT
{
    private readonly int Value;

    #region constructors

    internal PieceTypeT(int value)
    {
        this.Value = value;
        Debug.Assert(this.Value >= 0 && this.Value <= 8);
    }

    #endregion

    #region base operators

    public static PieceTypeT operator +(PieceTypeT v1, int v2)
    {
        return PieceType.Create(v1.Value + v2);
    }

    public static implicit operator int(PieceTypeT pt)
    {
        return pt.Value;
    }

    public override string ToString()
    {
        return this.Value.ToString();
    }

    #endregion
}
#endif

internal static class PieceType
{
#if PRIMITIVE
    internal const PieceTypeT PAWN = 1;

    internal const PieceTypeT KNIGHT = 2;

    internal const PieceTypeT BISHOP = 3;

    internal const PieceTypeT ROOK = 4;

    internal const PieceTypeT QUEEN = 5;

    internal const PieceTypeT KING = 6;

    internal const PieceTypeT NO_PIECE_TYPE = 0;

    internal const PieceTypeT ALL_PIECES = 0;

    
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static PieceTypeT Create(int value)
    {
        return value;
    }

#else
    internal static PieceTypeT PAWN = new PieceTypeT(1);

    internal static PieceTypeT KNIGHT = new PieceTypeT(2);

    internal static PieceTypeT BISHOP = new PieceTypeT(3);

    internal static PieceTypeT ROOK = new PieceTypeT(4);

    internal static PieceTypeT QUEEN = new PieceTypeT(5);

    internal static PieceTypeT KING = new PieceTypeT(6);

    internal static PieceTypeT NO_PIECE_TYPE = new PieceTypeT(0);

    internal static PieceTypeT ALL_PIECES = new PieceTypeT(0);

    internal static PieceTypeT Create(int value)
    {
        switch (value)
        {
            case 0:
                return ALL_PIECES;
            case 3:
                return BISHOP;
            case 6:
                return KING;
            case 2:
                return KNIGHT;
            case 1:
                return PAWN;
            case 4:
                return ROOK;
            case 5:
                return QUEEN;
            default:
                throw new ArgumentOutOfRangeException(nameof(value));
        }
    }
#endif

    internal static PieceTypeT[] AllPieceTypes = { PAWN, KNIGHT, BISHOP, ROOK, QUEEN, KING };

    internal const int PIECE_TYPE_NB = 8;
}