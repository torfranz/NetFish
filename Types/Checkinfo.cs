
#if PRIMITIVE
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
        this.ksq = pos.square(PieceType.KING, them);

        this.pinned = pos.pinned_pieces(pos.side_to_move());
        this.dcCandidates = pos.discovered_check_candidates();

        this.checkSquares[PieceType.PAWN] = Position.attacks_from_Pawn(this.ksq, them);
        this.checkSquares[PieceType.KNIGHT] = pos.attacks_from_PtS(PieceType.KNIGHT, this.ksq);
        this.checkSquares[PieceType.BISHOP] = pos.attacks_from_PtS(PieceType.BISHOP, this.ksq);
        this.checkSquares[PieceType.ROOK] = pos.attacks_from_PtS(PieceType.ROOK, this.ksq);
        this.checkSquares[PieceType.QUEEN] = this.checkSquares[PieceType.BISHOP] | this.checkSquares[PieceType.ROOK];
        this.checkSquares[PieceType.KING] = Bitboard.Create(0);
    }
};