public class CheckInfo
{
    public readonly Bitboard[] checkSquares = new Bitboard[PieceType.PIECE_TYPE_NB];

    public readonly Square ksq;

    public Bitboard dcCandidates;

    private Bitboard pinned;

    public CheckInfo(Position pos)
    {
        var them = ~pos.side_to_move();
        this.ksq = pos.square(PieceType.KING, them);

        this.pinned = pos.pinned_pieces(pos.side_to_move());
        this.dcCandidates = pos.discovered_check_candidates();

        this.checkSquares[PieceType.PAWN] = pos.attacks_from(PieceType.PAWN, this.ksq, them);
        this.checkSquares[PieceType.KNIGHT] = pos.attacks_from(PieceType.KNIGHT, this.ksq);
        this.checkSquares[PieceType.BISHOP] = pos.attacks_from(PieceType.BISHOP, this.ksq);
        this.checkSquares[PieceType.ROOK] = pos.attacks_from(PieceType.ROOK, this.ksq);
        this.checkSquares[PieceType.QUEEN] = this.checkSquares[PieceType.BISHOP] | this.checkSquares[PieceType.ROOK];
        this.checkSquares[PieceType.KING] = new Bitboard(0);
    }
};