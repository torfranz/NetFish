internal class CheckInfo
{
    internal readonly Bitboard[] checkSquares = new Bitboard[PieceType.PIECE_TYPE_NB_C];

    internal readonly Square ksq;

    internal Bitboard dcCandidates;

    internal Bitboard pinned;

    internal CheckInfo(Position pos)
    {
        var them = ~pos.side_to_move();
        ksq = pos.square(PieceType.KING, them);

        pinned = pos.pinned_pieces(pos.side_to_move());
        dcCandidates = pos.discovered_check_candidates();

        checkSquares[PieceType.PAWN_C] = pos.attacks_from(PieceType.PAWN, ksq, them);
        checkSquares[PieceType.KNIGHT_C] = pos.attacks_from(PieceType.KNIGHT, ksq);
        checkSquares[PieceType.BISHOP_C] = pos.attacks_from(PieceType.BISHOP, ksq);
        checkSquares[PieceType.ROOK_C] = pos.attacks_from(PieceType.ROOK, ksq);
        checkSquares[PieceType.QUEEN_C] = checkSquares[PieceType.BISHOP_C] | checkSquares[PieceType.ROOK_C];
        checkSquares[PieceType.KING_C] = new Bitboard(0);
    }
};