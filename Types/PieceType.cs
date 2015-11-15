using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

#if PRIMITIVE
using PieceTypeType = System.Int32;
#else
internal class PieceTypeType
{
    private int Value;
   
    #region constructors
    
    internal PieceTypeType(int value)
    {
        Value = value;
        Debug.Assert(Value >= 0 && Value <= 8);
    }
    #endregion

    #region base operators

    public static PieceTypeType operator +(PieceTypeType v1, int v2)
    {
        return PieceType.Create(v1.Value + v2);
    }

    public static implicit operator int(PieceTypeType pt)
    {
        return pt.Value;
    }
    
    public override string ToString()
    {
        return Value.ToString();
    }

    #endregion
}
#endif

internal static class PieceType
{

#if PRIMITIVE
    internal const PieceTypeType PAWN = 1;

    internal const PieceTypeType KNIGHT = 2;

    internal const PieceTypeType BISHOP = 3;

    internal const PieceTypeType ROOK = 4;

    internal const PieceTypeType QUEEN = 5;

    internal const PieceTypeType KING = 6;

    internal const int NO_PIECE_TYPE = 0;

    internal const PieceTypeType ALL_PIECES = 0;

    
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static PieceTypeType Create(int value)
    {
        return value;
    }

#else
    internal static PieceTypeType PAWN = new PieceTypeType(1);

    internal static PieceTypeType KNIGHT = new PieceTypeType(2);

    internal static PieceTypeType BISHOP = new PieceTypeType(3);

    internal static PieceTypeType ROOK = new PieceTypeType(4);

    internal static PieceTypeType QUEEN = new PieceTypeType(5);

    internal static PieceTypeType KING = new PieceTypeType(6);

    internal static PieceTypeType NO_PIECE_TYPE = new PieceTypeType(0);
    internal static PieceTypeType ALL_PIECES = new PieceTypeType(0);

    internal static PieceTypeType Create(int value)
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

    internal static PieceTypeType[] AllPieceTypes = { PAWN, KNIGHT, BISHOP, ROOK, QUEEN, KING };
    internal const int PIECE_TYPE_NB = 8;
    
}