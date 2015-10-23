public class CheckInfo
{
    public readonly Bitboard[] checkSquares = new Bitboard[PieceType.PIECE_TYPE_NB];

    public readonly Square ksq;

    public Bitboard dcCandidates;

    private Bitboard pinned;

    public CheckInfo(Position pos)
    {
        var them = ~pos.side_to_move();
        ksq = pos.square(PieceType.KING, them);

        pinned = pos.pinned_pieces(pos.side_to_move());
        dcCandidates = pos.discovered_check_candidates();

        checkSquares[PieceType.PAWN] = pos.attacks_from(PieceType.PAWN, ksq, them);
        checkSquares[PieceType.KNIGHT] = pos.attacks_from(PieceType.KNIGHT, ksq);
        checkSquares[PieceType.BISHOP] = pos.attacks_from(PieceType.BISHOP, ksq);
        checkSquares[PieceType.ROOK] = pos.attacks_from(PieceType.ROOK, ksq);
        checkSquares[PieceType.QUEEN] = checkSquares[PieceType.BISHOP] | checkSquares[PieceType.ROOK];
        checkSquares[PieceType.KING] = new Bitboard(0);
    }
};