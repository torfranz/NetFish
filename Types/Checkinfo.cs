﻿#if PRIMITIVE
using SquareT = System.Int32;
using BitboardT = System.UInt64;
#endif

internal class CheckInfo
{
    internal readonly BitboardT[] checkSquares = new BitboardT[PieceType.PIECE_TYPE_NB];

    internal readonly SquareT ksq;

    internal BitboardT dcCandidates;

    internal BitboardT pinned;

    internal CheckInfo(Position pos)
    {
        var them = Color.opposite(pos.side_to_move());
        ksq = pos.square(PieceType.KING, them);

        pinned = pos.pinned_pieces(pos.side_to_move());
        dcCandidates = pos.discovered_check_candidates();

        checkSquares[PieceType.PAWN] = pos.attacks_from_PS(PieceType.PAWN, ksq, them);
        checkSquares[PieceType.KNIGHT] = pos.attacks_from_PtS(PieceType.KNIGHT, ksq);
        checkSquares[PieceType.BISHOP] = pos.attacks_from_PtS(PieceType.BISHOP, ksq);
        checkSquares[PieceType.ROOK] = pos.attacks_from_PtS(PieceType.ROOK, ksq);
        checkSquares[PieceType.QUEEN] = checkSquares[PieceType.BISHOP] | checkSquares[PieceType.ROOK];
        checkSquares[PieceType.KING] = Bitboard.Create(0);
    }
};