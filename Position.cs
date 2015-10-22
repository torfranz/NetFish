using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

/// Position class stores information regarding the board representation as
/// pieces, side to move, hash keys, castling info, etc. Important methods are
/// do_move() and undo_move(), used by the search to update node info when
/// traversing the search tree.
public class Position
{
    public const string PieceToChar = " PNBRQK  pnbrqk";

    // Data members
    private Piece[] board = new Piece[Square.SQUARE_NB];

    private Bitboard[] byColorBB = new Bitboard[Color.COLOR_NB];

    private Bitboard[] byTypeBB = new Bitboard[PieceType.PIECE_TYPE_NB];

    private Bitboard[] castlingPath = new Bitboard[(int)CastlingRight.CASTLING_RIGHT_NB];

    private int[] castlingRightsMask = new int[Square.SQUARE_NB];

    private Square[] castlingRookSquare = new Square[(int)CastlingRight.CASTLING_RIGHT_NB];

    private bool chess960;

    private int gamePly;

    private int[] index = new int[Square.SQUARE_NB];

    private uint nodes;

    private int[,] pieceCount = new int[Color.COLOR_NB, PieceType.PIECE_TYPE_NB];

    private Square[,,] pieceList = new Square[Color.COLOR_NB, PieceType.PIECE_TYPE_NB, 16];

    private Color sideToMove;

    // Thread* thisThread;
    public StateInfo st;

    private StateInfo startState;

    /// Position::init() initializes at startup the various arrays used to compute
    /// hash keys.
    public static void init()
    {
        var rng = new PRNG(1070372);

        for (var c = Color.WHITE; c <= Color.BLACK; ++c)
        {
            for (var pt = PieceType.PAWN; pt <= PieceType.KING; ++pt)
            {
                for (var s = Square.SQ_A1; s <= Square.SQ_H8; ++s)
                {
                    Zobrist.psq[c, pt, s] = rng.rand();
                }
            }
        }

        for (var f = File.FILE_A; f <= File.FILE_H; ++f)
        {
            Zobrist.enpassant[f] = rng.rand();
        }

        for (var cr = (int)CastlingRight.NO_CASTLING; cr <= (int)CastlingRight.ANY_CASTLING; ++cr)
        {
            Zobrist.castling[cr] = 0;
            var b = new Bitboard((ulong)cr);
            while (b)
            {
                var k = Zobrist.castling[1 << Utils.pop_lsb(ref b)];
                Zobrist.castling[cr] ^= (k != 0 ? k : rng.rand());
            }
        }

        Zobrist.side = rng.rand();
        Zobrist.exclusion = rng.rand();
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Bitboard pieces()
    {
        return this.byTypeBB[PieceType.ALL_PIECES];
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Color side_to_move()
    {
        return this.sideToMove;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private bool empty(Square s)
    {
        return this.board[s] == Piece.NO_PIECE;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Piece piece_on(Square s)
    {
        return this.board[s];
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private Piece moved_piece(Move m)
    {
        return this.board[Move.from_sq(m)];
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Bitboard pieces(PieceType pt)
    {
        return this.byTypeBB[pt];
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Bitboard pieces(PieceType pt1, PieceType pt2)
    {
        return this.byTypeBB[pt1] | this.byTypeBB[pt2];
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Bitboard pieces(Color c)
    {
        return this.byColorBB[c];
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Bitboard pieces(Color c, PieceType pt)
    {
        return this.byColorBB[c] & this.byTypeBB[pt];
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Bitboard pieces(Color c, PieceType pt1, PieceType pt2)
    {
        return this.byColorBB[c] & (this.byTypeBB[pt1] | this.byTypeBB[pt2]);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private int count(PieceType Pt, Color c)
    {
        return this.pieceCount[c, Pt];
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Square[] squares(PieceType Pt, Color c)
    {
        var result = new Square[16];
        for (var idx = 0; idx < result.Length; idx++)
        {
            result[idx] = this.pieceList[c, Pt, idx];
        }
        return result;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Square square(PieceType Pt, Color c)
    {
        Debug.Assert(this.pieceCount[c, Pt] == 1);
        return this.pieceList[c, Pt, 0];
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Square ep_square()
    {
        return this.st.epSquare;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public bool can_castle(CastlingRight cr)
    {
        return (this.st.castlingRights & (int)cr) != 0;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public bool can_castle(Color c)
    {
        return (this.st.castlingRights & (((int)CastlingRight.WHITE_OO | (int)CastlingRight.WHITE_OOO) << (2 * c))) != 0;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public bool castling_impeded(CastlingRight cr)
    {
        return this.byTypeBB[PieceType.ALL_PIECES] & this.castlingPath[(int)cr];
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Square castling_rook_square(CastlingRight cr)
    {
        return this.castlingRookSquare[(int)cr];
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Bitboard attacks_from(PieceType Pt, Square s)
    {
        return Pt == PieceType.BISHOP || Pt == PieceType.ROOK
                   ? Utils.attacks_bb(Pt, s, this.byTypeBB[PieceType.ALL_PIECES])
                   : Pt == PieceType.QUEEN
                         ? this.attacks_from(PieceType.ROOK, s) | this.attacks_from(PieceType.BISHOP, s)
                         : Utils.StepAttacksBB[Pt, s];
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Bitboard attacks_from(PieceType Pt, Square s, Color c)
    {
        Debug.Assert(Pt == PieceType.PAWN);
        return Utils.StepAttacksBB[Piece.make_piece(c, PieceType.PAWN), s];
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Bitboard attacks_from(Piece pc, Square s)
    {
        return Utils.attacks_bb(pc, s, this.byTypeBB[PieceType.ALL_PIECES]);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Bitboard attackers_to(Square s)
    {
        return this.attackers_to(s, this.byTypeBB[PieceType.ALL_PIECES]);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Bitboard checkers()
    {
        return this.st.checkersBB;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Bitboard discovered_check_candidates()
    {
        return this.check_blockers(this.sideToMove, ~this.sideToMove);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Bitboard pinned_pieces(Color c)
    {
        return this.check_blockers(c, c);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private bool pawn_passed(Color c, Square s)
    {
        return !(this.pieces(~c, PieceType.PAWN) & Utils.passed_pawn_mask(c, s));
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private bool advanced_pawn_push(Move m)
    {
        return Piece.type_of(this.moved_piece(m)) == PieceType.PAWN
               && Rank.relative_rank(this.sideToMove, Move.from_sq(m)) > Rank.RANK_4;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private ulong key()
    {
        return this.st.key;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private ulong pawn_key()
    {
        return this.st.pawnKey;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private ulong material_key()
    {
        return this.st.materialKey;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private Score psq_score()
    {
        return this.st.psq;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private Value non_pawn_material(Color c)
    {
        return this.st.nonPawnMaterial[c];
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private int game_ply()
    {
        return this.gamePly;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private int rule50_count()
    {
        return this.st.rule50;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private ulong nodes_searched()
    {
        return this.nodes;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private void set_nodes_searched(ulong n)
    {
        this.nodes = (uint)n;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private bool opposite_bishops()
    {
        return this.pieceCount[Color.WHITE, PieceType.BISHOP] == 1
               && this.pieceCount[Color.BLACK, PieceType.BISHOP] == 1
               && Square.opposite_colors(
                   this.square(PieceType.BISHOP, Color.WHITE),
                   this.square(PieceType.BISHOP, Color.BLACK));
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public bool is_chess960()
    {
        return this.chess960;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private bool capture_or_promotion(Move m)
    {
        Debug.Assert(Move.is_ok(m));
        return Move.type_of(m) != MoveType.NORMAL ? Move.type_of(m) != MoveType.CASTLING : !this.empty(Move.to_sq(m));
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private bool capture(Move m)
    {
        // Castling is encoded as "king captures the rook"
        Debug.Assert(Move.is_ok(m));
        return (!this.empty(Move.to_sq(m)) && Move.type_of(m) != MoveType.CASTLING)
               || Move.type_of(m) == MoveType.ENPASSANT;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private PieceType captured_piece_type()
    {
        return this.st.capturedType;
    }

    /*
    #if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    Thread this_thread() {
  return thisThread;
}
*/

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private void put_piece(Color c, PieceType pt, Square s)
    {
        this.board[s] = Piece.make_piece(c, pt);
        this.byTypeBB[PieceType.ALL_PIECES] |= s;
        this.byTypeBB[pt] |= s;
        this.byColorBB[c] |= s;
        this.index[s] = this.pieceCount[c, pt]++;
        this.pieceList[c, pt, this.index[s]] = s;
        this.pieceCount[c, PieceType.ALL_PIECES]++;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private void remove_piece(Color c, PieceType pt, Square s)
    {
        // WARNING: This is not a reversible operation. If we remove a piece in
        // do_move() and then replace it in undo_move() we will put it at the end of
        // the list and not in its original place, it means index[] and pieceList[]
        // are not guaranteed to be invariant to a do_move() + undo_move() sequence.
        this.byTypeBB[PieceType.ALL_PIECES] ^= s;
        this.byTypeBB[pt] ^= s;
        this.byColorBB[c] ^= s;
        /* board[s] = NO_PIECE;  Not needed, overwritten by the capturing one */
        var lastSquare = this.pieceList[c, pt, --this.pieceCount[c, pt]];
        this.index[lastSquare] = this.index[s];
        this.pieceList[c, pt, this.index[lastSquare]] = lastSquare;
        this.pieceList[c, pt, this.pieceCount[c, pt]] = Square.SQ_NONE;
        this.pieceCount[c, PieceType.ALL_PIECES]--;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    private void move_piece(Color c, PieceType pt, Square from, Square to)
    {
        // index[from] is not updated and becomes stale. This works as long as index[]
        // is accessed just by known occupied squares.
        var from_to_bb = Utils.SquareBB[from] ^ Utils.SquareBB[to];
        this.byTypeBB[PieceType.ALL_PIECES] ^= from_to_bb;
        this.byTypeBB[pt] ^= from_to_bb;
        this.byColorBB[c] ^= from_to_bb;
        this.board[from] = Piece.NO_PIECE;
        this.board[to] = Piece.make_piece(c, pt);
        this.index[to] = this.index[from];
        this.pieceList[c, pt, this.index[to]] = to;
    }

    /// Position::set_castling_right() is a helper function used to set castling
    /// rights given the corresponding color and the rook starting square.
    private void set_castling_right(Color c, Square rfrom)
    {
        var kfrom = this.square(PieceType.KING, c);
        var cs = kfrom < rfrom ? CastlingSide.KING_SIDE : CastlingSide.QUEEN_SIDE;
        var cr = (c | cs);

        this.st.castlingRights |= (int)cr;
        this.castlingRightsMask[kfrom] |= (int)cr;
        this.castlingRightsMask[rfrom] |= (int)cr;
        this.castlingRookSquare[(int)cr] = rfrom;

        var kto = Square.relative_square(c, cs == CastlingSide.KING_SIDE ? Square.SQ_G1 : Square.SQ_C1);
        var rto = Square.relative_square(c, cs == CastlingSide.KING_SIDE ? Square.SQ_F1 : Square.SQ_D1);

        for (var s = rfrom < rto ? rfrom : rto; s <= (kfrom > kto ? kfrom : kto); ++s)
        {
            if (s != kfrom && s != rfrom)
            {
                this.castlingPath[(int)cr] |= s;
            }
        }

        for (var s = rfrom < rto ? rfrom : rto; s <= (kfrom > kto ? kfrom : kto); ++s)
        {
            if (s != kfrom && s != rfrom)
            {
                this.castlingPath[(int)cr] |= s;
            }
        }
    }

    /// Position::set_state() computes the hash keys of the position, and other
    /// data that once computed is updated incrementally as moves are made.
    /// The function is only used when a new position is set up, and to verify
    /// the correctness of the StateInfo data when running in debug mode.
    private void set_state(StateInfo si)
    {
        si.key = si.pawnKey = si.materialKey = 0;
        si.nonPawnMaterial[Color.WHITE] = si.nonPawnMaterial[Color.BLACK] = Value.VALUE_ZERO;
        si.psq = Score.SCORE_ZERO;

        si.checkersBB = this.attackers_to(this.square(PieceType.KING, this.sideToMove)) & this.pieces(~this.sideToMove);

        for (var b = this.pieces(); b;)
        {
            var s = Utils.pop_lsb(ref b);
            var pc = this.piece_on(s);
            si.key ^= Zobrist.psq[Piece.color_of(pc), Piece.type_of(pc), s];
            si.psq += PSQT.psq[Piece.color_of(pc), Piece.type_of(pc), s];
        }

        if (si.epSquare != Square.SQ_NONE)
        {
            si.key ^= Zobrist.enpassant[Square.file_of(si.epSquare)];
        }

        if (this.sideToMove == Color.BLACK)
        {
            si.key ^= Zobrist.side;
        }

        si.key ^= Zobrist.castling[si.castlingRights];

        for (var b = this.pieces(PieceType.PAWN); b;)
        {
            var s = Utils.pop_lsb(ref b);
            si.pawnKey ^= Zobrist.psq[Piece.color_of(this.piece_on(s)), PieceType.PAWN, s];
        }

        for (var c = Color.WHITE; c <= Color.BLACK; ++c)
        {
            for (var pt = PieceType.PAWN; pt <= PieceType.KING; ++pt)
            {
                for (var cnt = 0; cnt < this.pieceCount[c, pt]; ++cnt)
                {
                    si.materialKey ^= Zobrist.psq[c, pt, cnt];
                }
            }
        }

        for (var c = Color.WHITE; c <= Color.BLACK; ++c)
        {
            for (var pt = PieceType.KNIGHT; pt <= PieceType.QUEEN; ++pt)
            {
                si.nonPawnMaterial[c] += this.pieceCount[c, pt] * Value.PieceValue[(int)Phase.MG][pt];
            }
        }
    }

    /// Position::game_phase() calculates the game phase interpolating total non-pawn
    /// material between endgame and midgame limits.
    private Phase game_phase()
    {
        var npm = this.st.nonPawnMaterial[Color.WHITE] + this.st.nonPawnMaterial[Color.BLACK];

        npm = new Value(Math.Max(Value.EndgameLimit, Math.Min(npm, Value.MidgameLimit)));

        return
            (Phase)(((npm - Value.EndgameLimit) * (int)Phase.PHASE_MIDGAME) / (Value.MidgameLimit - Value.EndgameLimit));
    }

    /// Position::check_blockers() returns a bitboard of all the pieces with color
    /// 'c' that are blocking check on the king with color 'kingColor'. A piece
    /// blocks a check if removing that piece from the board would result in a
    /// position where the king is in check. A check blocking piece can be either a
    /// pinned or a discovered check piece, according if its color 'c' is the same
    /// or the opposite of 'kingColor'.
    private Bitboard check_blockers(Color c, Color kingColor)
    {
        Bitboard b, pinners, result = new Bitboard(0);
        var ksq = this.square(PieceType.KING, kingColor);

        // Pinners are sliders that give check when a pinned piece is removed
        pinners = ((this.pieces(PieceType.ROOK, PieceType.QUEEN) & Utils.PseudoAttacks[PieceType.ROOK, ksq])
                   | (this.pieces(PieceType.BISHOP, PieceType.QUEEN) & Utils.PseudoAttacks[PieceType.BISHOP, ksq]))
                  & this.pieces(~kingColor);

        while (pinners)
        {
            b = Utils.between_bb(ksq, Utils.pop_lsb(ref pinners)) & this.pieces();

            if (!Bitboard.more_than_one(b))
            {
                result |= b & this.pieces(c);
            }
        }
        return result;
    }

    /// Position::attackers_to() computes a bitboard of all pieces which attack a
    /// given square. Slider attacks use the occupied bitboard to indicate occupancy.
    private Bitboard attackers_to(Square s, Bitboard occupied)
    {
        return (this.attacks_from(PieceType.PAWN, s, Color.BLACK) & this.pieces(Color.WHITE, PieceType.PAWN))
               | (this.attacks_from(PieceType.PAWN, s, Color.WHITE) & this.pieces(Color.BLACK, PieceType.PAWN))
               | (this.attacks_from(PieceType.KNIGHT, s) & this.pieces(PieceType.KNIGHT))
               | (Utils.attacks_bb(PieceType.ROOK, s, occupied) & this.pieces(PieceType.ROOK, PieceType.QUEEN))
               | (Utils.attacks_bb(PieceType.BISHOP, s, occupied) & this.pieces(PieceType.BISHOP, PieceType.QUEEN))
               | (this.attacks_from(PieceType.KING, s) & this.pieces(PieceType.KING));
    }

    /// Position::legal() tests whether a pseudo-legal move is legal
    public bool legal(Move m, Bitboard pinned)
    {
        Debug.Assert(Move.is_ok(m));
        Debug.Assert(pinned == this.pinned_pieces(this.sideToMove));

        var us = this.sideToMove;
        var from = Move.from_sq(m);

        Debug.Assert(Piece.color_of(this.moved_piece(m)) == us);
        Debug.Assert(this.piece_on(this.square(PieceType.KING, us)) == Piece.make_piece(us, PieceType.KING));

        // En passant captures are a tricky special case. Because they are rather
        // uncommon, we do it simply by testing whether the king is attacked after
        // the move is made.
        if (Move.type_of(m) == MoveType.ENPASSANT)
        {
            var ksq = this.square(PieceType.KING, us);
            var to = Move.to_sq(m);
            var capsq = to - Square.pawn_push(us);
            var occupied = (this.pieces() ^ from ^ capsq) | to;

            Debug.Assert(to == this.ep_square());
            Debug.Assert(this.moved_piece(m) == Piece.make_piece(us, PieceType.PAWN));
            Debug.Assert(this.piece_on(capsq) == Piece.make_piece(~us, PieceType.PAWN));
            Debug.Assert(this.piece_on(to) == Piece.NO_PIECE);

            return
                !(Utils.attacks_bb(PieceType.ROOK, ksq, occupied) & this.pieces(~us, PieceType.QUEEN, PieceType.ROOK))
                && !(Utils.attacks_bb(PieceType.BISHOP, ksq, occupied)
                     & this.pieces(~us, PieceType.QUEEN, PieceType.BISHOP));
        }

        // If the moving piece is a king, check whether the destination
        // square is attacked by the opponent. Castling moves are checked
        // for legality during move generation.
        if (Piece.type_of(this.piece_on(from)) == PieceType.KING)
        {
            return Move.type_of(m) == MoveType.CASTLING || !(this.attackers_to(Move.to_sq(m)) & this.pieces(~us));
        }

        // A non-king move is legal if and only if it is not pinned or it
        // is moving along the ray towards or away from the king.
        return !pinned || !(pinned & from) || Utils.aligned(from, Move.to_sq(m), this.square(PieceType.KING, us));
    }

    /// Position::pseudo_legal() takes a random move and tests whether the move is
    /// pseudo legal. It is used to validate moves from TT that can be corrupted
    /// due to SMP concurrent access or hash position key aliasing.
    private bool pseudo_legal(Move m)
    {
        var us = this.sideToMove;
        var from = Move.from_sq(m);
        var to = Move.to_sq(m);
        var pc = this.moved_piece(m);

        // Use a slower but simpler function for uncommon cases
        if (Move.type_of(m) != MoveType.NORMAL)
        {
            return new MoveList(GenType.LEGAL, this).contains(m);
        }

        // Is not a promotion, so promotion piece must be empty
        if (Move.promotion_type(m) - PieceType.KNIGHT != PieceType.NO_PIECE_TYPE)
        {
            return false;
        }

        // If the 'from' square is not occupied by a piece belonging to the side to
        // move, the move is obviously not legal.
        if (pc == Piece.NO_PIECE || Piece.color_of(pc) != us)
        {
            return false;
        }

        // The destination square cannot be occupied by a friendly piece
        if (this.pieces(us) & to)
        {
            return false;
        }

        // Handle the special case of a pawn move
        if (Piece.type_of(pc) == PieceType.PAWN)
        {
            // We have already handled promotion moves, so destination
            // cannot be on the 8th/1st rank.
            if (Square.rank_of(to) == Rank.relative_rank(us, Rank.RANK_8))
            {
                return false;
            }

            if (!(this.attacks_from(PieceType.PAWN, from, us) & this.pieces(~us) & to) // Not a capture
                && !((from + Square.pawn_push(us) == to) && this.empty(to)) // Not a single push
                && !((from + 2 * Square.pawn_push(us) == to) // Not a double push
                     && (Square.rank_of(from) == Rank.relative_rank(us, Rank.RANK_2)) && this.empty(to)
                     && this.empty(to - Square.pawn_push(us))))
            {
                return false;
            }
        }
        else if (!(this.attacks_from(pc, from) & to))
        {
            return false;
        }

        // Evasions generator already takes care to avoid some kind of illegal moves
        // and legal() relies on this. We therefore have to take care that the same
        // kind of moves are filtered out here.
        if (this.checkers())
        {
            if (Piece.type_of(pc) != PieceType.KING)
            {
                // Double check? In this case a king move is required
                if (Bitboard.more_than_one(this.checkers()))
                {
                    return false;
                }

                // Our move must be a blocking evasion or a capture of the checking piece
                if (
                    !((Utils.between_bb(Utils.lsb(this.checkers()), this.square(PieceType.KING, us)) | this.checkers())
                      & to))
                {
                    return false;
                }
            }
            // In case of king moves under check we have to remove king so as to catch
            // invalid moves like b1a1 when opposite queen is on c1.
            else if (this.attackers_to(to, this.pieces() ^ from) & this.pieces(~us))
            {
                return false;
            }
        }

        return true;
    }

    /// Position::gives_check() tests whether a pseudo-legal move gives a check
    public bool gives_check(Move m, CheckInfo ci)
    {
        Debug.Assert(Move.is_ok(m));
        Debug.Assert(ci.dcCandidates == this.discovered_check_candidates());
        Debug.Assert(Piece.color_of(this.moved_piece(m)) == this.sideToMove);

        var from = Move.from_sq(m);
        var to = Move.to_sq(m);

        // Is there a direct check?
        if (ci.checkSquares[Piece.type_of(this.piece_on(from))] & to)
        {
            return true;
        }

        // Is there a discovered check?
        if ((bool)ci.dcCandidates && (ci.dcCandidates & from) && !Utils.aligned(from, to, ci.ksq))
        {
            return true;
        }

        switch (Move.type_of(m))
        {
            case MoveType.NORMAL:
                return false;

            case MoveType.PROMOTION:
                return Utils.attacks_bb(Move.promotion_type(m), to, this.pieces() ^ from) & ci.ksq;

            // En passant capture with check? We have already handled the case
            // of direct checks and ordinary discovered check, so the only case we
            // need to handle is the unusual case of a discovered check through
            // the captured pawn.
            case MoveType.ENPASSANT:
                {
                    var capsq = Square.make_square(Square.file_of(to), Square.rank_of(from));
                    var b = (this.pieces() ^ from ^ capsq) | to;

                    return (Utils.attacks_bb(PieceType.ROOK, ci.ksq, b)
                            & this.pieces(this.sideToMove, PieceType.QUEEN, PieceType.ROOK))
                           | (Utils.attacks_bb(PieceType.BISHOP, ci.ksq, b)
                              & this.pieces(this.sideToMove, PieceType.QUEEN, PieceType.BISHOP));
                }
            case MoveType.CASTLING:
                {
                    var kfrom = from;
                    var rfrom = to; // Castling is encoded as 'King captures the rook'
                    var kto = Square.relative_square(this.sideToMove, rfrom > kfrom ? Square.SQ_G1 : Square.SQ_C1);
                    var rto = Square.relative_square(this.sideToMove, rfrom > kfrom ? Square.SQ_F1 : Square.SQ_D1);

                    return (bool)(Utils.PseudoAttacks[PieceType.ROOK, rto] & ci.ksq)
                           && (Utils.attacks_bb(PieceType.ROOK, rto, (this.pieces() ^ kfrom ^ rfrom) | rto | kto)
                               & ci.ksq);
                }
            default:
                Debug.Assert(false);
                return false;
        }
    }

    /// Position::do_move() makes a move, and saves all information necessary
    /// to a StateInfo object. The move is assumed to be legal. Pseudo-legal
    /// moves should be filtered out before this function is called.
    private void do_move(Move m, StateInfo newSt, bool givesCheck)
    {
        Debug.Assert(Move.is_ok(m));
        Debug.Assert(newSt != this.st);

        ++this.nodes;
        var k = this.st.key ^ Zobrist.side;

        // Copy some fields of the old state to our new StateInfo object except the
        // ones which are going to be recalculated from scratch anyway and then switch
        // our state pointer to point to the new (ready to be updated) state.
        newSt.copyFrom(this.st);

        newSt.previous = this.st;
        this.st = newSt;

        // Increment ply counters. In particular, rule50 will be reset to zero later on
        // in case of a capture or a pawn move.
        ++this.gamePly;
        ++this.st.rule50;
        ++this.st.pliesFromNull;

        var us = this.sideToMove;
        var them = ~us;
        var from = Move.from_sq(m);
        var to = Move.to_sq(m);
        var pt = Piece.type_of(this.piece_on(from));
        var captured = Move.type_of(m) == MoveType.ENPASSANT ? PieceType.PAWN : Piece.type_of(this.piece_on(to));

        Debug.Assert(Piece.color_of(this.piece_on(from)) == us);
        Debug.Assert(
            this.piece_on(to) == Piece.NO_PIECE
            || Piece.color_of(this.piece_on(to)) == (Move.type_of(m) != MoveType.CASTLING ? them : us));
        Debug.Assert(captured != PieceType.KING);

        if (Move.type_of(m) == MoveType.CASTLING)
        {
            Debug.Assert(pt == PieceType.KING);

            Square rfrom, rto;
            this.do_castling(true, us, from, ref to, out rfrom, out rto);

            captured = PieceType.NO_PIECE_TYPE;
            this.st.psq += PSQT.psq[us, PieceType.ROOK, rto] - PSQT.psq[us, PieceType.ROOK, rfrom];
            k ^= Zobrist.psq[us, PieceType.ROOK, rfrom] ^ Zobrist.psq[us, PieceType.ROOK, rto];
        }

        if (captured)
        {
            var capsq = to;

            // If the captured piece is a pawn, update pawn hash key, otherwise
            // update non-pawn material.
            if (captured == PieceType.PAWN)
            {
                if (Move.type_of(m) == MoveType.ENPASSANT)
                {
                    capsq -= Square.pawn_push(us);

                    Debug.Assert(pt == PieceType.PAWN);
                    Debug.Assert(to == this.st.epSquare);
                    Debug.Assert(Rank.relative_rank(us, to) == Rank.RANK_6);
                    Debug.Assert(this.piece_on(to) == Piece.NO_PIECE);
                    Debug.Assert(this.piece_on(capsq) == Piece.make_piece(them, PieceType.PAWN));

                    this.board[capsq] = Piece.NO_PIECE; // Not done by remove_piece()
                }

                this.st.pawnKey ^= Zobrist.psq[them, PieceType.PAWN, capsq];
            }
            else
            {
                this.st.nonPawnMaterial[them] -= Value.PieceValue[(int)Phase.MG][captured];
            }

            // Update board and piece lists
            this.remove_piece(them, captured, capsq);

            // Update material hash key and prefetch access to materialTable
            k ^= Zobrist.psq[them, captured, capsq];
            this.st.materialKey ^= Zobrist.psq[them, captured, this.pieceCount[them, captured]];

            // Update incremental scores
            this.st.psq -= PSQT.psq[them, captured, capsq];

            // Reset rule 50 counter
            this.st.rule50 = 0;
        }

        // Update hash key
        k ^= Zobrist.psq[us, pt, from] ^ Zobrist.psq[us, pt, to];

        // Reset en passant square
        if (this.st.epSquare != Square.SQ_NONE)
        {
            k ^= Zobrist.enpassant[Square.file_of(this.st.epSquare)];
            this.st.epSquare = Square.SQ_NONE;
        }

        // Update castling rights if needed
        if (this.st.castlingRights != 0 && ((this.castlingRightsMask[from] | this.castlingRightsMask[to]) != 0))
        {
            var cr = this.castlingRightsMask[from] | this.castlingRightsMask[to];
            k ^= Zobrist.castling[this.st.castlingRights & cr];
            this.st.castlingRights &= ~cr;
        }

        // Move the piece. The tricky Chess960 castling is handled earlier
        if (Move.type_of(m) != MoveType.CASTLING)
        {
            this.move_piece(us, pt, from, to);
        }

        // If the moving piece is a pawn do some special extra work
        if (pt == PieceType.PAWN)
        {
            // Set en-passant square if the moved pawn can be captured
            if ((to ^ from) == 16
                && (this.attacks_from(PieceType.PAWN, to - Square.pawn_push(us), us) & this.pieces(them, PieceType.PAWN)))
            {
                this.st.epSquare = (from + to) / 2;
                k ^= Zobrist.enpassant[Square.file_of(this.st.epSquare)];
            }

            else if (Move.type_of(m) == MoveType.PROMOTION)
            {
                var promotion = Move.promotion_type(m);

                Debug.Assert(Rank.relative_rank(us, to) == Rank.RANK_8);
                Debug.Assert(promotion >= PieceType.KNIGHT && promotion <= PieceType.QUEEN);

                this.remove_piece(us, PieceType.PAWN, to);
                this.put_piece(us, promotion, to);

                // Update hash keys
                k ^= Zobrist.psq[us, PieceType.PAWN, to] ^ Zobrist.psq[us, promotion, to];
                this.st.pawnKey ^= Zobrist.psq[us, PieceType.PAWN, to];
                this.st.materialKey ^= Zobrist.psq[us, promotion, this.pieceCount[us, promotion] - 1]
                                       ^ Zobrist.psq[us, PieceType.PAWN, this.pieceCount[us, PieceType.PAWN]];

                // Update incremental score
                this.st.psq += PSQT.psq[us, promotion, to] - PSQT.psq[us, PieceType.PAWN, to];

                // Update material
                this.st.nonPawnMaterial[us] += Value.PieceValue[(int)Phase.MG][promotion];
            }

            // Update pawn hash key and prefetch access to pawnsTable
            this.st.pawnKey ^= Zobrist.psq[us, PieceType.PAWN, from] ^ Zobrist.psq[us, PieceType.PAWN, to];

            // Reset rule 50 draw counter
            this.st.rule50 = 0;
        }

        // Update incremental scores
        this.st.psq += PSQT.psq[us, pt, to] - PSQT.psq[us, pt, from];

        // Set capture piece
        this.st.capturedType = captured;

        // Update the key with the final value
        this.st.key = k;

        // Calculate checkers bitboard (if move gives check)
        this.st.checkersBB = givesCheck
                                 ? this.attackers_to(this.square(PieceType.KING, them)) & this.pieces(us)
                                 : new Bitboard(0);

        this.sideToMove = ~this.sideToMove;

        Debug.Assert(this.pos_is_ok());
    }

    /// Position::undo_move() unmakes a move. When it returns, the position should
    /// be restored to exactly the same state as before the move was made.
    private void undo_move(Move m)
    {
        Debug.Assert(Move.is_ok(m));

        this.sideToMove = ~this.sideToMove;

        var us = this.sideToMove;
        var from = Move.from_sq(m);
        var to = Move.to_sq(m);
        var pt = Piece.type_of(this.piece_on(to));

        Debug.Assert(this.empty(from) || Move.type_of(m) == MoveType.CASTLING);
        Debug.Assert(this.st.capturedType != PieceType.KING);

        if (Move.type_of(m) == MoveType.PROMOTION)
        {
            Debug.Assert(Rank.relative_rank(us, to) == Rank.RANK_8);
            Debug.Assert(pt == Move.promotion_type(m));
            Debug.Assert(pt >= PieceType.KNIGHT && pt <= PieceType.QUEEN);

            this.remove_piece(us, pt, to);
            this.put_piece(us, PieceType.PAWN, to);
            pt = PieceType.PAWN;
        }

        if (Move.type_of(m) == MoveType.CASTLING)
        {
            Square rfrom, rto;
            this.do_castling(false, us, from, ref to, out rfrom, out rto);
        }
        else
        {
            this.move_piece(us, pt, to, from); // Put the piece back at the source square

            if (this.st.capturedType)
            {
                var capsq = to;

                if (Move.type_of(m) == MoveType.ENPASSANT)
                {
                    capsq -= Square.pawn_push(us);

                    Debug.Assert(pt == PieceType.PAWN);
                    Debug.Assert(to == this.st.previous.epSquare);
                    Debug.Assert(Rank.relative_rank(us, to) == Rank.RANK_6);
                    Debug.Assert(this.piece_on(capsq) == Piece.NO_PIECE);
                    Debug.Assert(this.st.capturedType == PieceType.PAWN);
                }

                this.put_piece(~us, this.st.capturedType, capsq); // Restore the captured piece
            }
        }

        // Finally point our state pointer back to the previous state
        this.st = this.st.previous;
        --this.gamePly;

        Debug.Assert(this.pos_is_ok());
    }

    /// Position::do_castling() is a helper used to do/undo a castling move. This
    /// is a bit tricky, especially in Chess960.
    private void do_castling(bool Do, Color us, Square from, ref Square to, out Square rfrom, out Square rto)
    {
        var kingSide = to > from;
        rfrom = to; // Castling is encoded as "king captures friendly rook"
        rto = Square.relative_square(us, kingSide ? Square.SQ_F1 : Square.SQ_D1);
        to = Square.relative_square(us, kingSide ? Square.SQ_G1 : Square.SQ_C1);

        // Remove both pieces first since squares could overlap in Chess960
        this.remove_piece(us, PieceType.KING, Do ? from : to);
        this.remove_piece(us, PieceType.ROOK, Do ? rfrom : rto);
        this.board[Do ? from : to] = this.board[Do ? rfrom : rto] = Piece.NO_PIECE;
        // Since remove_piece doesn't do it for us
        this.put_piece(us, PieceType.KING, Do ? to : from);
        this.put_piece(us, PieceType.ROOK, Do ? rto : rfrom);
    }

    /// Position::do(undo)_null_move() is used to do(undo) a "null move": It flips
    /// the side to move without executing any move on the board.
    private void do_null_move(StateInfo newSt)
    {
        Debug.Assert(!this.checkers());
        Debug.Assert(newSt != this.st);

        newSt.copyFrom(this.st);

        newSt.previous = this.st;
        this.st = newSt;

        if (this.st.epSquare != Square.SQ_NONE)
        {
            this.st.key ^= Zobrist.enpassant[Square.file_of(this.st.epSquare)];
            this.st.epSquare = Square.SQ_NONE;
        }

        this.st.key ^= Zobrist.side;

        ++this.st.rule50;
        this.st.pliesFromNull = 0;

        this.sideToMove = ~this.sideToMove;

        Debug.Assert(this.pos_is_ok());
    }

    private void undo_null_move()
    {
        Debug.Assert(!this.checkers());

        this.st = this.st.previous;
        this.sideToMove = ~this.sideToMove;
    }

    // min_attacker() is a helper function used by see() to locate the least
    // valuable attacker for the side to move, remove the attacker we just found
    // from the bitboards and scan for new X-ray attacks behind it.
    private PieceType min_attacker(
        PieceType Pt,
        Bitboard[] bb,
        Square to,
        Bitboard stmAttackers,
        ref Bitboard occupied,
        ref Bitboard attackers)
    {
        if (Pt == PieceType.KING)
        {
            return PieceType.KING;
        }
        var b = stmAttackers & bb[Pt];
        if (!b)
        {
            return this.min_attacker(Pt + 1, bb, to, stmAttackers, ref occupied, ref attackers);
        }

        occupied ^= b & ~(b - new Bitboard(1));

        if (Pt == PieceType.PAWN || Pt == PieceType.BISHOP || Pt == PieceType.QUEEN)
        {
            attackers |= Utils.attacks_bb(PieceType.BISHOP, to, occupied) & (bb[PieceType.BISHOP] | bb[PieceType.QUEEN]);
        }

        if (Pt == PieceType.ROOK || Pt == PieceType.QUEEN)
        {
            attackers |= Utils.attacks_bb(PieceType.ROOK, to, occupied) & (bb[PieceType.ROOK] | bb[PieceType.QUEEN]);
        }

        attackers &= occupied; // After X-ray that may add already processed pieces
        return Pt;
    }

    /// Position::key_after() computes the new hash key after the given move. Needed
    /// for speculative prefetch. It doesn't recognize special moves like castling,
    /// en-passant and promotions.
    private ulong key_after(Move m)
    {
        var us = this.sideToMove;
        var from = Move.from_sq(m);
        var to = Move.to_sq(m);
        var pt = Piece.type_of(this.piece_on(from));
        var captured = Piece.type_of(this.piece_on(to));
        var k = this.st.key ^ Zobrist.side;

        if (captured)
        {
            k ^= Zobrist.psq[~us, captured, to];
        }

        return k ^ Zobrist.psq[us, pt, to] ^ Zobrist.psq[us, pt, from];
    }

    /// Position::see() is a static exchange evaluator: It tries to estimate the
    /// material gain or loss resulting from a move.
    private Value see_sign(Move m)
    {
        Debug.Assert(Move.is_ok(m));

        // Early return if SEE cannot be negative because captured piece value
        // is not less then capturing one. Note that king moves always return
        // here because king midgame value is set to 0.
        if (Value.PieceValue[(int)Phase.MG][this.moved_piece(m)]
            <= Value.PieceValue[(int)Phase.MG][this.piece_on(Move.to_sq(m))])
        {
            return Value.VALUE_KNOWN_WIN;
        }

        return this.see(m);
    }

    private Value see(Move m)
    {
        Square from, to;
        Bitboard occupied, attackers, stmAttackers;
        var swapList = new Value[32];
        var slIndex = 1;
        PieceType captured;
        Color stm;

        Debug.Assert(Move.is_ok(m));

        from = Move.from_sq(m);
        to = Move.to_sq(m);
        swapList[0] = Value.PieceValue[(int)Phase.MG][this.piece_on(to)];
        stm = Piece.color_of(this.piece_on(from));
        occupied = this.pieces() ^ from;

        // Castling moves are implemented as king capturing the rook so cannot
        // be handled correctly. Simply return VALUE_ZERO that is always correct
        // unless in the rare case the rook ends up under attack.
        if (Move.type_of(m) == MoveType.CASTLING)
        {
            return Value.VALUE_ZERO;
        }

        if (Move.type_of(m) == MoveType.ENPASSANT)
        {
            occupied ^= to - Square.pawn_push(stm); // Remove the captured pawn
            swapList[0] = Value.PieceValue[(int)Phase.MG][PieceType.PAWN];
        }

        // Find all attackers to the destination square, with the moving piece
        // removed, but possibly an X-ray attacker added behind it.
        attackers = this.attackers_to(to, occupied) & occupied;

        // If the opponent has no attackers we are finished
        stm = ~stm;
        stmAttackers = attackers & this.pieces(stm);
        if (!stmAttackers)
        {
            return swapList[0];
        }

        // The destination square is defended, which makes things rather more
        // difficult to compute. We proceed by building up a "swap list" containing
        // the material gain or loss at each stop in a sequence of captures to the
        // destination square, where the sides alternately capture, and always
        // capture with the least valuable piece. After each capture, we look for
        // new X-ray attacks from behind the capturing piece.
        captured = Piece.type_of(this.piece_on(from));

        do
        {
            Debug.Assert(slIndex < 32);

            // Add the new entry to the swap list
            swapList[slIndex] = -swapList[slIndex - 1] + Value.PieceValue[(int)Phase.MG][captured];

            // Locate and remove the next least valuable attacker
            captured = this.min_attacker(PieceType.PAWN, this.byTypeBB, to, stmAttackers, ref occupied, ref attackers);
            stm = ~stm;
            stmAttackers = attackers & this.pieces(stm);
            ++slIndex;
        }
        while (stmAttackers && (captured != PieceType.KING || (--slIndex != 0))); // Stop before a king capture

        // Having built the swap list, we negamax through it to find the best
        // achievable score from the point of view of the side to move.
        while (--slIndex != 0)
        {
            swapList[slIndex - 1] = new Value(Math.Min(-swapList[slIndex], swapList[slIndex - 1]));
        }

        return swapList[0];
    }

    /// Position::is_draw() tests whether the position is drawn by 50-move rule
    /// or by repetition. It does not detect stalemates.
    private bool is_draw()
    {
        if (this.st.rule50 > 99 && (!this.checkers() || new MoveList(GenType.LEGAL, this).size() > 0))
        {
            return true;
        }

        var stp = this.st;
        for (int i = 2, e = Math.Min(this.st.rule50, this.st.pliesFromNull); i <= e; i += 2)
        {
            stp = stp.previous.previous;

            if (stp.key == this.st.key)
            {
                return true; // Draw at first repetition
            }
        }

        return false;
    }

    private bool pos_is_ok()
    {
        const bool Fast = true; // Quick (default) or full check?

        for (var step = (int)CheckStep.Default;
             step <= (Fast ? (int)CheckStep.Default : (int)CheckStep.Castling);
             step++)
        {
            if (step == (int)CheckStep.Default)
            {
                if (this.sideToMove != Color.WHITE && this.sideToMove != Color.BLACK)
                {
                    return false;
                }

                if (this.piece_on(this.square(PieceType.KING, Color.WHITE)) != Piece.W_KING)
                {
                    return false;
                }

                if (this.piece_on(this.square(PieceType.KING, Color.BLACK)) != Piece.B_KING)
                {
                    return false;
                }

                var relRank = Rank.relative_rank(this.sideToMove, this.ep_square());
                if (this.ep_square() != Square.SQ_NONE && relRank != Rank.RANK_6)
                {
                    return false;
                }
            }

            if (step == (int)CheckStep.King)
            {
                if (this.board.Count(piece => piece == Piece.W_KING) != 1
                    || this.board.Count(piece => piece == Piece.B_KING) != 1
                    || this.attackers_to(this.square(PieceType.KING, ~this.sideToMove)) & this.pieces(this.sideToMove))
                {
                    return false;
                }
            }

            if (step == (int)CheckStep.Bitboards)
            {
                if ((this.pieces(Color.WHITE) & this.pieces(Color.BLACK))
                    || (this.pieces(Color.WHITE) | this.pieces(Color.BLACK)) != this.pieces())
                {
                    return false;
                }

                for (var p1 = PieceType.PAWN; p1 <= PieceType.KING; ++p1)
                {
                    for (var p2 = PieceType.PAWN; p2 <= PieceType.KING; ++p2)
                    {
                        if (p1 != p2 && (this.pieces(p1) & this.pieces(p2)))
                        {
                            return false;
                        }
                    }
                }
            }

            if (step == (int)CheckStep.Lists)
            {
                for (var c = Color.WHITE; c <= Color.BLACK; ++c)
                {
                    for (var pt = PieceType.PAWN; pt <= PieceType.KING; ++pt)
                    {
                        if (this.pieceCount[c, pt] != Bitcount.popcount_Full(this.pieces(c, pt)))
                        {
                            return false;
                        }

                        for (var i = 0; i < this.pieceCount[c, pt]; ++i)
                        {
                            if (this.board[this.pieceList[c, pt, i]] != Piece.make_piece(c, pt)
                                || this.index[this.pieceList[c, pt, i]] != i)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            if (step == (int)CheckStep.Castling)
            {
                for (var c = Color.WHITE; c <= Color.BLACK; ++c)
                {
                    for (var s = CastlingSide.KING_SIDE; s <= CastlingSide.QUEEN_SIDE; s++)
                    {
                        if (!this.can_castle(c | s))
                        {
                            continue;
                        }

                        if (this.piece_on(this.castlingRookSquare[(int)(c | s)]) != Piece.make_piece(c, PieceType.ROOK)
                            || this.castlingRightsMask[this.castlingRookSquare[(int)(c | s)]] != (int)(c | s)
                            || (this.castlingRightsMask[this.square(PieceType.KING, c)] & (int)(c | s)) != (int)(c | s))
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }

    public string fen()
    {
        int emptyCnt;
        var ss = new StringBuilder();

        for (var r = Rank.RANK_8; r >= Rank.RANK_1; --r)
        {
            for (var f = File.FILE_A; f <= File.FILE_H; ++f)
            {
                for (emptyCnt = 0; f <= File.FILE_H && this.empty(Square.make_square(f, r)); ++f)
                {
                    ++emptyCnt;
                }

                if (emptyCnt != 0)
                {
                    ss.Append(emptyCnt);
                }

                if (f <= File.FILE_H)
                {
                    ss.Append(PieceToChar[this.piece_on(Square.make_square(f, r))]);
                }
            }

            if (r > Rank.RANK_1)
            {
                ss.Append('/');
            }
        }

        ss.Append((this.sideToMove == Color.WHITE ? " w " : " b "));

        if (this.can_castle(CastlingRight.WHITE_OO))
        {
            ss.Append(
                (this.chess960
                     ? (char)('A' + Square.file_of(this.castling_rook_square(Color.WHITE | CastlingSide.KING_SIDE)))
                     : 'K'));
        }

        if (this.can_castle(CastlingRight.WHITE_OOO))
        {
            ss.Append(
                (this.chess960
                     ? (char)('A' + Square.file_of(this.castling_rook_square(Color.WHITE | CastlingSide.QUEEN_SIDE)))
                     : 'Q'));
        }

        if (this.can_castle(CastlingRight.BLACK_OO))
        {
            ss.Append(
                (this.chess960
                     ? (char)('a' + Square.file_of(this.castling_rook_square(Color.BLACK | CastlingSide.KING_SIDE)))
                     : 'k'));
        }

        if (this.can_castle(CastlingRight.BLACK_OOO))
        {
            ss.Append(
                (this.chess960
                     ? (char)('a' + Square.file_of(this.castling_rook_square(Color.BLACK | CastlingSide.QUEEN_SIDE)))
                     : 'q'));
        }

        if (!this.can_castle(Color.WHITE) && !this.can_castle(Color.BLACK))
        {
            ss.Append('-');
        }

        ss.Append((this.ep_square() == Square.SQ_NONE ? " - " : " " + UCI.square(this.ep_square()) + " "));
        ss.Append(this.st.rule50);
        ss.Append(" ");
        ss.Append(1 + (this.gamePly - (this.sideToMove == Color.BLACK ? 1 : 0)) / 2);

        return ss.ToString();
    }

    private ulong exclusion_key()
    {
        return this.st.key ^ Zobrist.exclusion;
    }

    private static bool isdigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    private static bool islower(char token)
    {
        return token.ToString().ToLowerInvariant() == token.ToString();
    }

    private static char toupper(char token)
    {
        return token.ToString().ToUpperInvariant()[0];
    }

    private static char tolower(char token)
    {
        return token.ToString().ToLowerInvariant()[0];
    }

    private static Stack<string> CreateStack(string input)
    {
        var lines = input.Trim().Split(' ');
        var stack = new Stack<string>(); // LIFO
        for (var i = (lines.Length - 1); i >= 0; i--)
        {
            var line = lines[i];
            if (!string.IsNullOrEmpty(line))
            {
                line = line.Trim();
                stack.Push(line);
            }
        }
        return stack;
    }

    /// Position::set() initializes the position object with the given FEN string.
    /// This function is not very robust - make sure that input FENs are correct,
    /// this is assumed to be the responsibility of the GUI.
    private void set(string fenStr, bool isChess960 /*, Thread th*/)
    {
        /*
           A FEN string defines a particular position using only the ASCII character set.

           A FEN string contains six fields separated by a space. The fields are:

           1) Piece placement (from white's perspective). Each rank is described, starting
              with rank 8 and ending with rank 1. Within each rank, the contents of each
              square are described from file A through file H. Following the Standard
              Algebraic Notation (SAN), each piece is identified by a single letter taken
              from the standard English names. White pieces are designated using upper-case
              letters ("PNBRQK") whilst Black uses lowercase ("pnbrqk"). Blank squares are
              noted using digits 1 through 8 (the number of blank squares), and "/"
              separates ranks.

           2) Active color. "w" means white moves next, "b" means black.

           3) Castling availability. If neither side can castle, this is "-". Otherwise,
              this has one or more letters: "K" (White can castle kingside), "Q" (White
              can castle queenside), "k" (Black can castle kingside), and/or "q" (Black
              can castle queenside).

           4) En passant target square (in algebraic notation). If there's no en passant
              target square, this is "-". If a pawn has just made a 2-square move, this
              is the position "behind" the pawn. This is recorded regardless of whether
              there is a pawn in position to make an en passant capture.

           5) Halfmove clock. This is the number of halfmoves since the last pawn advance
              or capture. This is used to determine if a draw can be claimed under the
              fifty-move rule.

           6) Fullmove number. The number of the full move. It starts at 1, and is
              incremented after Black's move.
        */
        char col, row, token;
        int p;
        var sq = Square.SQ_A8;

        var fen = fenStr.ToCharArray();
        var fenPos = 0;
        this.clear();

        // 1. Piece placement
        while ((token = fen[fenPos++]) != ' ')
        {
            if (isdigit(token))
            {
                sq += (token - '0'); // Advance the given number of files
            }
            else if (token == '/')
            {
                sq -= 16;
            }
            else
            {
                p = PieceToChar.IndexOf(token);
                if (p > -1)
                {
                    this.put_piece(Piece.color_of(new Piece(p)), Piece.type_of(new Piece(p)), sq);
                    sq++;
                }
            }
        }

        // 2. Active color
        token = fen[fenPos++];
        this.sideToMove = (token == 'w' ? Color.WHITE : Color.BLACK);
        token = fen[fenPos++];

        // 3. Castling availability. Compatible with 3 standards: Normal FEN standard,
        // Shredder-FEN that uses the letters of the columns on which the rooks began
        // the game instead of KQkq and also X-FEN standard that, in case of Chess960,
        // if an inner rook is associated with the castling right, the castling tag is
        // replaced by the file letter of the involved rook, as for the Shredder-FEN.
        while ((token = fen[fenPos++]) != ' ')
        {
            Square rsq;
            var c = islower(token) ? Color.BLACK : Color.WHITE;
            token = toupper(token);

            if (token == 'K')
            {
                for (rsq = Square.relative_square(c, Square.SQ_H1);
                     Piece.type_of(this.piece_on(rsq)) != PieceType.ROOK;
                     rsq--)
                {
                }
            }
            else if (token == 'Q')
            {
                for (rsq = Square.relative_square(c, Square.SQ_A1);
                     Piece.type_of(this.piece_on(rsq)) != PieceType.ROOK;
                     rsq++)
                {
                }
            }
            else if (token >= 'A' && token <= 'H')
            {
                rsq = new Square((token - 'A') | Rank.relative_rank(c, Rank.RANK_1));
            }
            else
            {
                continue;
            }

            this.set_castling_right(c, rsq);
        }

        if (fenPos < fenStr.Length)
        {
            col = fen[fenPos++];
            if (fenPos < fenStr.Length)
            {
                row = fen[fenPos++];

                // 4. En passant square. Ignore if no pawn capture is possible
                if (((col >= 'a' && col <= 'h')) && ((row == '3' || row == '6')))
                {
                    this.st.epSquare = Square.make_square(new File(col - 'a'), new Rank(row - '1'));

                    if ((this.attackers_to(this.st.epSquare) & this.pieces(this.sideToMove, PieceType.PAWN)) == 0)
                    {
                        this.st.epSquare = Square.SQ_NONE;
                    }
                }
            }
        }

        // 5-6. Halfmove clock and fullmove number
        var tokens = CreateStack(fenStr.Substring(fenPos));
        if (tokens.Count > 0)
        {
            this.st.rule50 = int.Parse(tokens.Pop());
        }
        if (tokens.Count > 0)
        {
            this.gamePly = int.Parse(tokens.Pop());
        }

        // Convert from fullmove starting from 1 to ply starting from 0,
        // handle also common incorrect FEN with fullmove = 0.
        this.gamePly = Math.Max(2 * (this.gamePly - 1), 0) + ((this.sideToMove == Color.BLACK) ? 1 : 0);

        this.chess960 = isChess960;
        //this.thisThread = th;
        this.set_state(this.st);

        Debug.Assert(this.pos_is_ok());
    }

    /// clear() erases the position object to a pristine state, with an
    /// empty board, white to move, and no castling rights.
    internal void clear()
    {
        this.board = new Piece[Square.SQUARE_NB];

        this.byColorBB = new Bitboard[Color.COLOR_NB];

        this.byTypeBB = new Bitboard[PieceType.PIECE_TYPE_NB];

        this.castlingPath = new Bitboard[(int)CastlingRight.CASTLING_RIGHT_NB];

        this.castlingRightsMask = new int[Square.SQUARE_NB];

        this.castlingRookSquare = new Square[(int)CastlingRight.CASTLING_RIGHT_NB];

        this.index = new int[Square.SQUARE_NB];

        this.pieceCount = new int[Color.COLOR_NB, PieceType.PIECE_TYPE_NB];

        this.pieceList = new Square[Color.COLOR_NB, PieceType.PIECE_TYPE_NB, 16];

        this.chess960 = false;

        this.gamePly = 0;

        this.nodes = 0;

        this.sideToMove = Color.WHITE;

        // thisThread = ;
        this.startState = new StateInfo();

        this.st = this.startState;
    }

    /// Position::pos_is_ok() performs some consistency checks for the position object.
    /// This is meant to be helpful when debugging.
    private enum CheckStep
    {
        Default,

        King,

        Bitboards,

        State,

        Lists,

        Castling
    };

    /// Position::flip() flips position with the white and black sides reversed. This
    /// is only useful for debugging e.g. for finding evaluation symmetry bugs.
    public void flip()
    {
        var tokens = CreateStack(this.fen());
        Debug.Assert(tokens.Count == 6);

        var flippedFen = new StringBuilder();
        // 1.Position
        var ranks = tokens.Pop().Split('/');
        Debug.Assert(ranks.Length == 8);
        for (var idx = ranks.Length - 1; idx >= 0; idx--)
        {
            foreach (var posValue in ranks[idx].ToCharArray())
            {
                flippedFen.Append(islower(posValue) ? toupper(posValue) : tolower(posValue));
            }

            flippedFen.Append(idx > 0 ? '/' : ' ');
        }

        // 2. Color
        flippedFen.Append(tokens.Pop() == "w" ? "b " : "w ");

        // 3. Castling
        var castling = tokens.Pop();
        for (var idx = castling.Length - 1; idx >= 0; idx--)
        {
            foreach (var castlingValue in castling.ToCharArray())
            {
                flippedFen.Append(islower(castlingValue) ? toupper(castlingValue) : tolower(castlingValue));
            }
        }
        flippedFen.Append(' ');

        // 4. Enpassant
        var ep = tokens.Pop();
        foreach (var epValue in ep.ToCharArray())
        {
            flippedFen.Append(epValue == '3' ? '6' : epValue == '6' ? '3' : epValue);
        }
        flippedFen.Append(' ');

        // 5. Halfmoves
        flippedFen.Append(tokens.Pop());
        flippedFen.Append(' ');

        // 6. FullMoves
        flippedFen.Append(tokens.Pop());
        flippedFen.Append(' ');

        this.set(flippedFen.ToString(), this.chess960);
    }

    public Position(Position other)
    {
        Array.Copy(other.board, this.board, other.board.Length);
        Array.Copy(other.byColorBB, this.byColorBB, other.byColorBB.Length);
        Array.Copy(other.byTypeBB, this.byTypeBB, other.byTypeBB.Length);
        Array.Copy(other.castlingPath, this.castlingPath, other.castlingPath.Length);
        Array.Copy(other.castlingRightsMask, this.castlingRightsMask, other.castlingRightsMask.Length);
        Array.Copy(other.castlingRookSquare, this.castlingRookSquare, other.castlingRookSquare.Length);
        Array.Copy(other.index, this.index, other.index.Length);
        Array.Copy(other.pieceCount, this.pieceCount, other.pieceCount.Length);
        Array.Copy(other.pieceList, this.pieceList, other.pieceList.Length);

        this.chess960 = other.chess960;
        this.gamePly = other.gamePly;
        this.sideToMove = other.sideToMove;

        // thisThread = other.thisThread;
        this.startState = new StateInfo();
        this.startState.copyFrom(other.st);
        this.st = this.startState;

        Debug.Assert(this.pos_is_ok());
    }

    public Position(string f, bool c960 /*, Thread* th*/)
    {
        this.set(f, c960 /*, th*/);
    }

    public string displayString()
    {
        var sb = new StringBuilder("\n +---+---+---+---+---+---+---+---+\n");
        for (var r = Rank.RANK_8; r >= Rank.RANK_1; --r)
        {
            for (var f = File.FILE_A; f <= File.FILE_H; ++f)
            {
                sb.Append(" | ");
                sb.Append(PieceToChar[this.piece_on(Square.make_square(f, r))]);
            }

            sb.Append(" |\n +---+---+---+---+---+---+---+---+\n");
        }

        sb.Append($"\nFen: {this.fen()}\nKey: {this.st.key}\nCheckers: ");

        for (var b = this.checkers(); b;)
        {
            sb.Append(UCI.square(Utils.pop_lsb(ref b)) + " ");
        }

        return sb.ToString();
    }

    /*
    /// Position::flip() flips position with the white and black sides reversed. This
/// is only useful for debugging e.g. for finding evaluation symmetry bugs.

void Position::flip() {

  string f, token;
  std::stringstream ss(fen());

  for (Rank r = RANK_8; r >= RANK_1; --r) // Piece placement
  {
      std::getline(ss, token, r > RANK_1 ? '/' : ' ');
      f.insert(0, token + (f.empty() ? " " : "/"));
  }

  ss >> token; // Active color
  f += (token == "w" ? "B " : "W "); // Will be lowercased later

  ss >> token; // Castling availability
  f += token + " ";

  std::transform(f.begin(), f.end(), f.begin(),
                 [](char c) { return char(islower(c) ? toupper(c) : tolower(c)); });

  ss >> token; // En passant square
  f += (token == "-" ? token : token.replace(1, 1, token[1] == '3' ? "6" : "3"));

  std::getline(ss, token); // Half and full moves
  f += token;

  set(f, is_chess960(), this_thread());

  assert(pos_is_ok());
}
    */
}