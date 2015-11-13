using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

/// Position class stores information regarding the board representation as
/// pieces, side to move, hash keys, castling info, etc. Important methods are
/// do_move() and undo_move(), used by the search to update node info when
/// traversing the search tree.
internal class Position
{
    internal const string PieceToChar = " PNBRQK  pnbrqk";

    // Data members
    private Piece[] board = new Piece[Square.SQUARE_NB_C];

    private Bitboard[] byColorBB = new Bitboard[Color.COLOR_NB_C];

    private Bitboard[] byTypeBB = new Bitboard[PieceType.PIECE_TYPE_NB_C];

    private Bitboard[] castlingPath = new Bitboard[(int) CastlingRight.CASTLING_RIGHT_NB];

    private int[] castlingRightsMask = new int[Square.SQUARE_NB_C];

    private Square[] castlingRookSquare = new Square[(int) CastlingRight.CASTLING_RIGHT_NB];

    private bool chess960;

    private int gamePly;

    private int[] index = new int[Square.SQUARE_NB_C];

    private int nodes;

    private int[,] pieceCount = new int[Color.COLOR_NB_C, PieceType.PIECE_TYPE_NB_C];

    private Square[,,] pieceList = new Square[Color.COLOR_NB_C, PieceType.PIECE_TYPE_NB_C, 16];

    private Color sideToMove;

    internal StateInfo st;

    private StateInfo startState;

    private Thread thisThread;

    internal Position(Position other)
        : this(other, other.thisThread)
    {
    }

    internal Position(Position other, Thread thread)
    {
        Array.Copy(other.board, board, other.board.Length);
        Array.Copy(other.byColorBB, byColorBB, other.byColorBB.Length);
        Array.Copy(other.byTypeBB, byTypeBB, other.byTypeBB.Length);
        Array.Copy(other.castlingPath, castlingPath, other.castlingPath.Length);
        Array.Copy(other.castlingRightsMask, castlingRightsMask, other.castlingRightsMask.Length);
        Array.Copy(other.castlingRookSquare, castlingRookSquare, other.castlingRookSquare.Length);
        Array.Copy(other.index, index, other.index.Length);
        Array.Copy(other.pieceCount, pieceCount, other.pieceCount.Length);
        Array.Copy(other.pieceList, pieceList, other.pieceList.Length);

        chess960 = other.chess960;
        gamePly = other.gamePly;
        sideToMove = other.sideToMove;

        thisThread = thread;
        startState = new StateInfo();
        startState.copyFrom(other.st);
        st = startState;

        nodes = 0;
        Debug.Assert(pos_is_ok());
    }

    private void clearBoard()
    {
        for (int idx = 0; idx < board.Length; idx++)
        {
            board[idx] = Piece.NO_PIECE;
        }
    }

    internal Position(string f, bool c960, Thread th)
    {
        this.clearBoard();

        set(f, c960, th);
    }

    /// Position::init() initializes at startup the various arrays used to compute
    /// hash keys.
    internal static void init()
    {
        var rng = new PRNG(1070372);

        for (var c = Color.WHITE_C; c <= Color.BLACK_C; ++c)
        {
            for (var pt = PieceType.PAWN_C; pt <= PieceType.KING_C; ++pt)
            {
                for (var s = Square.SQ_A1; s <= Square.SQ_H8; ++s)
                {
                    Zobrist.psq[c, pt, (int)s] = rng.rand();
                }
            }
        }

        for (var f = File.FILE_A; f <= File.FILE_H; ++f)
        {
            Zobrist.enpassant[f] = rng.rand();
        }

        for (var cr = (int) CastlingRight.NO_CASTLING; cr <= (int) CastlingRight.ANY_CASTLING; ++cr)
        {
            Zobrist.castling[cr] = 0;
            var b = new Bitboard((ulong) cr);
            while (b)
            {
                var k = Zobrist.castling[1 << (int)Utils.pop_lsb(ref b)];
                Zobrist.castling[cr] ^= (k != 0 ? k : rng.rand());
            }
        }

        Zobrist.side = rng.rand();
        Zobrist.exclusion = rng.rand();
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Bitboard pieces()
    {
        return byTypeBB[PieceType.ALL_PIECES_C];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Color side_to_move()
    {
        return sideToMove;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal bool empty(Square s)
    {
        return board[(int)s] == Piece.NO_PIECE;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Piece piece_on(Square s)
    {
        return board[(int)s];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Piece moved_piece(Move m)
    {
        return board[(int)Move.from_sq(m)];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Bitboard pieces(PieceType pt)
    {
        return byTypeBB[(int)pt];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Bitboard pieces(PieceType pt1, PieceType pt2)
    {
        return byTypeBB[(int)pt1] | byTypeBB[(int)pt2];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Bitboard pieces(Color c)
    {
        return byColorBB[c.ValueMe];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Bitboard pieces(Color c, PieceType pt)
    {
        return byColorBB[c.ValueMe] & byTypeBB[(int)pt];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Bitboard pieces(Color c, PieceType pt1, PieceType pt2)
    {
        return byColorBB[c.ValueMe] & (byTypeBB[(int)pt1] | byTypeBB[(int)pt2]);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal int count(PieceType Pt, Color c)
    {
        return pieceCount[c.ValueMe, (int)Pt];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Square square(PieceType Pt, Color c, int idx)
    {
        return pieceList[c.ValueMe, (int)Pt, idx];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Square square(PieceType Pt, Color c)
    {
        Debug.Assert(pieceCount[c.ValueMe, (int)Pt] == 1);
        return pieceList[c.ValueMe, (int)Pt, 0];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Square ep_square()
    {
        return st.epSquare;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal bool can_castle(CastlingRight cr)
    {
        return (st.castlingRights & (int) cr) != 0;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal int can_castle(Color c)
    {
        return (st.castlingRights & (((int) CastlingRight.WHITE_OO | (int) CastlingRight.WHITE_OOO) << (2*c.ValueMe)));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal bool castling_impeded(CastlingRight cr)
    {
        return byTypeBB[PieceType.ALL_PIECES_C] & castlingPath[(int) cr];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Square castling_rook_square(CastlingRight cr)
    {
        return castlingRookSquare[(int) cr];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Bitboard attacks_from(PieceType Pt, Square s)
    {
        return Pt == PieceType.BISHOP || Pt == PieceType.ROOK
            ? Utils.attacks_bb(Pt, s, byTypeBB[PieceType.ALL_PIECES_C])
            : Pt == PieceType.QUEEN
                ? attacks_from(PieceType.ROOK, s) | attacks_from(PieceType.BISHOP, s)
                : Utils.StepAttacksBB[(int)Pt, (int)s];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Bitboard attacks_from(PieceType Pt, Square s, Color c)
    {
        Debug.Assert(Pt == PieceType.PAWN);
        return Utils.StepAttacksBB[(int)Piece.make_piece(c, PieceType.PAWN), (int)s];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Bitboard attacks_from(Piece pc, Square s)
    {
        return Utils.attacks_bb(pc, s, byTypeBB[PieceType.ALL_PIECES_C]);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Bitboard attackers_to(Square s)
    {
        return attackers_to(s, byTypeBB[PieceType.ALL_PIECES_C]);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Bitboard checkers()
    {
        return st.checkersBB;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Bitboard discovered_check_candidates()
    {
        return check_blockers(sideToMove, ~sideToMove);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Bitboard pinned_pieces(Color c)
    {
        return check_blockers(c, c);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal bool pawn_passed(Color c, Square s)
    {
        return !(pieces(~c, PieceType.PAWN) & Utils.passed_pawn_mask(c, s));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal bool advanced_pawn_push(Move m)
    {
        return Piece.type_of(moved_piece(m)) == PieceType.PAWN
               && Rank.relative_rank(sideToMove, Move.from_sq(m)) > Rank.RANK_4;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal ulong key()
    {
        return st.key;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal ulong pawn_key()
    {
        return st.pawnKey;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal ulong material_key()
    {
        return st.materialKey;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Score psq_score()
    {
        return st.psq;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Value non_pawn_material(Color c)
    {
        return st.nonPawnMaterial[c.ValueMe];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal int game_ply()
    {
        return gamePly;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal int rule50_count()
    {
        return st.rule50;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal int nodes_searched()
    {
        return nodes;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal void set_nodes_searched(int n)
    {
        nodes = n;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal bool opposite_bishops()
    {
        return pieceCount[Color.WHITE_C, PieceType.BISHOP_C] == 1
               && pieceCount[Color.BLACK_C, PieceType.BISHOP_C] == 1
               && Square.opposite_colors(
                   square(PieceType.BISHOP, Color.WHITE),
                   square(PieceType.BISHOP, Color.BLACK));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal bool is_chess960()
    {
        return chess960;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal bool capture_or_promotion(Move m)
    {
        Debug.Assert(Move.is_ok(m));
        return Move.type_of(m) != MoveType.NORMAL ? Move.type_of(m) != MoveType.CASTLING : !empty(Move.to_sq(m));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal bool capture(Move m)
    {
        // Castling is encoded as "king captures the rook"
        Debug.Assert(Move.is_ok(m));
        return (!empty(Move.to_sq(m)) && Move.type_of(m) != MoveType.CASTLING)
               || Move.type_of(m) == MoveType.ENPASSANT;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal PieceType captured_piece_type()
    {
        return st.capturedType;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Thread this_thread()
    {
        return thisThread;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void put_piece(Color c, PieceType pieceType, Square s)
    {
        var pt = (int) pieceType;
        board[(int)s] = Piece.make_piece(c, pieceType);
        byTypeBB[PieceType.ALL_PIECES_C] |= s;
        byTypeBB[pt] |= s;
        byColorBB[c.ValueMe] |= s;
        index[(int)s] = pieceCount[c.ValueMe, pt]++;
        pieceList[c.ValueMe, pt, index[(int)s]] = s;
        pieceCount[c.ValueMe, PieceType.ALL_PIECES_C]++;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void remove_piece(Color c, PieceType pieceType, Square s)
    {
        var pt = (int) pieceType;
        // WARNING: This is not a reversible operation. If we remove a piece in
        // do_move() and then replace it in undo_move() we will put it at the end of
        // the list and not in its original place, it means index[] and pieceList[]
        // are not guaranteed to be invariant to a do_move() + undo_move() sequence.
        byTypeBB[PieceType.ALL_PIECES_C] ^= s;
        byTypeBB[pt] ^= s;
        byColorBB[c.ValueMe] ^= s;
        /* board[s] = NO_PIECE;  Not needed, overwritten by the capturing one */
        var lastSquare = pieceList[c.ValueMe, pt, --pieceCount[c.ValueMe, pt]];
        index[(int)lastSquare] = index[(int)s];
        pieceList[c.ValueMe, pt, index[(int)lastSquare]] = lastSquare;
        pieceList[c.ValueMe, pt, pieceCount[c.ValueMe, pt]] = Square.SQ_NONE;
        pieceCount[c.ValueMe, PieceType.ALL_PIECES_C]--;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void move_piece(Color c, PieceType pieceType, Square from, Square to)
    {
        var pt = (int) pieceType;
        // index[from] is not updated and becomes stale. This works as long as index[]
        // is accessed just by known occupied squares.
        var from_to_bb = Utils.SquareBB[(int)from] ^ Utils.SquareBB[(int)to];
        byTypeBB[PieceType.ALL_PIECES_C] ^= from_to_bb;
        byTypeBB[pt] ^= from_to_bb;
        byColorBB[c.ValueMe] ^= from_to_bb;
        board[(int)from] = Piece.NO_PIECE;
        board[(int)to] = Piece.make_piece(c, pieceType);
        index[(int)to] = index[(int)from];
        pieceList[c.ValueMe, pt, index[(int)to]] = to;
    }

    /// Position::set_castling_right() is a helper function used to set castling
    /// rights given the corresponding color and the rook starting square.
    private void set_castling_right(Color c, Square rfrom)
    {
        var kfrom = square(PieceType.KING, c);
        var cs = kfrom < rfrom ? CastlingSide.KING_SIDE : CastlingSide.QUEEN_SIDE;
        var cr = (c | cs);

        st.castlingRights |= (int) cr;
        castlingRightsMask[(int)kfrom] |= (int) cr;
        castlingRightsMask[(int)rfrom] |= (int) cr;
        castlingRookSquare[(int) cr] = rfrom;

        var kto = Square.relative_square(c, cs == CastlingSide.KING_SIDE ? Square.SQ_G1 : Square.SQ_C1);
        var rto = Square.relative_square(c, cs == CastlingSide.KING_SIDE ? Square.SQ_F1 : Square.SQ_D1);

        for (var s = rfrom < rto ? rfrom : rto; s <= (kfrom > kto ? kfrom : kto); ++s)
        {
            if (s != kfrom && s != rfrom)
            {
                castlingPath[(int) cr] |= s;
            }
        }

        for (var s = rfrom < rto ? rfrom : rto; s <= (kfrom > kto ? kfrom : kto); ++s)
        {
            if (s != kfrom && s != rfrom)
            {
                castlingPath[(int) cr] |= s;
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
        si.nonPawnMaterial[Color.WHITE_C] = si.nonPawnMaterial[Color.BLACK_C] = Value.VALUE_ZERO;
        si.psq = Score.SCORE_ZERO;

        si.checkersBB = attackers_to(square(PieceType.KING, sideToMove)) & pieces(~sideToMove);

        for (var b = pieces(); b;)
        {
            var s = Utils.pop_lsb(ref b);
            var pc = piece_on(s);
            var color = Piece.color_of(pc).ValueMe;
            var pieceType = (int) Piece.type_of(pc);
            si.key ^= Zobrist.psq[color, pieceType, (int)s];
            si.psq += PSQT.psq[color, pieceType, (int)s];
        }

        if (si.epSquare != Square.SQ_NONE)
        {
            si.key ^= Zobrist.enpassant[Square.file_of(si.epSquare)];
        }

        if (sideToMove == Color.BLACK)
        {
            si.key ^= Zobrist.side;
        }

        si.key ^= Zobrist.castling[si.castlingRights];

        for (var b = pieces(PieceType.PAWN); b;)
        {
            var s = Utils.pop_lsb(ref b);
            si.pawnKey ^= Zobrist.psq[Piece.color_of(piece_on(s)).ValueMe, PieceType.PAWN_C, (int)s];
        }

        for (var c = Color.WHITE_C; c <= Color.BLACK_C; ++c)
        {
            for (var pt = PieceType.PAWN_C; pt <= PieceType.KING_C; ++pt)
            {
                for (var cnt = 0; cnt < pieceCount[c, pt]; ++cnt)
                {
                    si.materialKey ^= Zobrist.psq[c, pt, cnt];
                }
            }
        }

        for (var c = Color.WHITE_C; c <= Color.BLACK_C; ++c)
        {
            for (var pt = PieceType.KNIGHT_C; pt <= PieceType.QUEEN_C; ++pt)
            {
                si.nonPawnMaterial[c] += pieceCount[c, pt]*Value.PieceValue[(int) Phase.MG][pt];
            }
        }
    }

    /// Position::game_phase() calculates the game phase interpolating total non-pawn
    /// material between endgame and midgame limits.
    internal Phase game_phase()
    {
        var npm = st.nonPawnMaterial[Color.WHITE_C] + st.nonPawnMaterial[Color.BLACK_C];

        npm = new Value(Math.Max(Value.EndgameLimit, Math.Min(npm, Value.MidgameLimit)));

        return
            (Phase) (((npm - Value.EndgameLimit)*(int) Phase.PHASE_MIDGAME)/(Value.MidgameLimit - Value.EndgameLimit));
    }

    /// Position::check_blockers() returns a bitboard of all the pieces with color
    /// 'c' that are blocking check on the king with color 'kingColor'. A piece
    /// blocks a check if removing that piece from the board would result in a
    /// position where the king is in check. A check blocking piece can be either a
    /// pinned or a discovered check piece, according if its color 'c' is the same
    /// or the opposite of 'kingColor'.
    private Bitboard check_blockers(Color c, Color kingColor)
    {
        Bitboard result = new Bitboard(0);
        var ksq = square(PieceType.KING, kingColor);

        // Pinners are sliders that give check when a pinned piece is removed
        var pinners = ((pieces(PieceType.ROOK, PieceType.QUEEN) & Utils.PseudoAttacks[PieceType.ROOK_C, (int)ksq])
                            | (pieces(PieceType.BISHOP, PieceType.QUEEN) & Utils.PseudoAttacks[PieceType.BISHOP_C, (int)ksq]))
                           & pieces(~kingColor);

        while (pinners)
        {
            var b = Utils.between_bb(ksq, Utils.pop_lsb(ref pinners)) & pieces();

            if (!Bitboard.more_than_one(b))
            {
                result |= b & pieces(c);
            }
        }
        return result;
    }

    /// Position::attackers_to() computes a bitboard of all pieces which attack a
    /// given square. Slider attacks use the occupied bitboard to indicate occupancy.
    private Bitboard attackers_to(Square s, Bitboard occupied)
    {
        return (attacks_from(PieceType.PAWN, s, Color.BLACK) & pieces(Color.WHITE, PieceType.PAWN))
               | (attacks_from(PieceType.PAWN, s, Color.WHITE) & pieces(Color.BLACK, PieceType.PAWN))
               | (attacks_from(PieceType.KNIGHT, s) & pieces(PieceType.KNIGHT))
               | (Utils.attacks_bb(PieceType.ROOK, s, occupied) & pieces(PieceType.ROOK, PieceType.QUEEN))
               | (Utils.attacks_bb(PieceType.BISHOP, s, occupied) & pieces(PieceType.BISHOP, PieceType.QUEEN))
               | (attacks_from(PieceType.KING, s) & pieces(PieceType.KING));
    }

    /// Position::legal() tests whether a pseudo-legal move is legal
    internal bool legal(Move m, Bitboard pinned)
    {
        Debug.Assert(Move.is_ok(m));
        Debug.Assert(pinned == pinned_pieces(sideToMove));

        var us = sideToMove;
        var from = Move.from_sq(m);

        Debug.Assert(Piece.color_of(moved_piece(m)) == us);
        Debug.Assert(piece_on(square(PieceType.KING, us)) == Piece.make_piece(us, PieceType.KING));

        // En passant captures are a tricky special case. Because they are rather
        // uncommon, we do it simply by testing whether the king is attacked after
        // the move is made.
        if (Move.type_of(m) == MoveType.ENPASSANT)
        {
            var ksq = square(PieceType.KING, us);
            var to = Move.to_sq(m);
            var capsq = to - Square.pawn_push(us);
            var occupied = (pieces() ^ from ^ capsq) | to;

            Debug.Assert(to == ep_square());
            Debug.Assert(moved_piece(m) == Piece.make_piece(us, PieceType.PAWN));
            Debug.Assert(piece_on(capsq) == Piece.make_piece(~us, PieceType.PAWN));
            Debug.Assert(piece_on(to) == Piece.NO_PIECE);

            return
                !(Utils.attacks_bb(PieceType.ROOK, ksq, occupied) & pieces(~us, PieceType.QUEEN, PieceType.ROOK))
                && !(Utils.attacks_bb(PieceType.BISHOP, ksq, occupied)
                     & pieces(~us, PieceType.QUEEN, PieceType.BISHOP));
        }

        // If the moving piece is a king, check whether the destination
        // square is attacked by the opponent. Castling moves are checked
        // for legality during move generation.
        if (Piece.type_of(piece_on(from)) == PieceType.KING)
        {
            return Move.type_of(m) == MoveType.CASTLING || !(attackers_to(Move.to_sq(m)) & pieces(~us));
        }

        // A non-king move is legal if and only if it is not pinned or it
        // is moving along the ray towards or away from the king.
        return !pinned || !(pinned & from) || Utils.aligned(from, Move.to_sq(m), square(PieceType.KING, us));
    }

    /// Position::pseudo_legal() takes a random move and tests whether the move is
    /// pseudo legal. It is used to validate moves from TT that can be corrupted
    /// due to SMP concurrent access or hash position key aliasing.
    internal bool pseudo_legal(Move m)
    {
        var us = sideToMove;
        var from = Move.from_sq(m);
        var to = Move.to_sq(m);
        var pc = moved_piece(m);

        // Use a slower but simpler function for uncommon cases
        if (Move.type_of(m) != MoveType.NORMAL)
        {
            return new MoveList(GenType.LEGAL, this).contains(m);
        }

        // Is not a promotion, so promotion piece must be empty
        if ((int)Move.promotion_type(m) - PieceType.KNIGHT_C != PieceType.NO_PIECE_TYPE_C)
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
        if (pieces(us) & to)
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

            if (!(attacks_from(PieceType.PAWN, from, us) & pieces(~us) & to) // Not a capture
                && !((from + Square.pawn_push(us) == to) && empty(to)) // Not a single push
                && !((from + 2*Square.pawn_push(us) == to) // Not a double push
                     && (Square.rank_of(from) == Rank.relative_rank(us, Rank.RANK_2)) && empty(to)
                     && empty(to - Square.pawn_push(us))))
            {
                return false;
            }
        }
        else if (!(attacks_from(pc, from) & to))
        {
            return false;
        }

        // Evasions generator already takes care to avoid some kind of illegal moves
        // and legal() relies on this. We therefore have to take care that the same
        // kind of moves are filtered out here.
        if (checkers())
        {
            if (Piece.type_of(pc) != PieceType.KING)
            {
                // Double check? In this case a king move is required
                if (Bitboard.more_than_one(checkers()))
                {
                    return false;
                }

                // Our move must be a blocking evasion or a capture of the checking piece
                if (
                    !((Utils.between_bb(Utils.lsb(checkers()), square(PieceType.KING, us)) | checkers())
                      & to))
                {
                    return false;
                }
            }
            // In case of king moves under check we have to remove king so as to catch
            // invalid moves like b1a1 when opposite queen is on c1.
            else if (attackers_to(to, pieces() ^ from) & pieces(~us))
            {
                return false;
            }
        }

        return true;
    }

    /// Position::gives_check() tests whether a pseudo-legal move gives a check
    internal bool gives_check(Move m, CheckInfo ci)
    {
        Debug.Assert(Move.is_ok(m));
        Debug.Assert(ci.dcCandidates == discovered_check_candidates());
        Debug.Assert(Piece.color_of(moved_piece(m)) == sideToMove);

        var from = Move.from_sq(m);
        var to = Move.to_sq(m);

        // Is there a direct check?
        if (ci.checkSquares[(int)Piece.type_of(piece_on(from))] & to)
        {
            return true;
        }

        // Is there a discovered check?
        if ((bool) ci.dcCandidates && (ci.dcCandidates & from) && !Utils.aligned(from, to, ci.ksq))
        {
            return true;
        }

        switch (Move.type_of(m))
        {
            case MoveType.NORMAL:
                return false;

            case MoveType.PROMOTION:
                return Utils.attacks_bb(Piece.Create((int)Move.promotion_type(m)), to, pieces() ^ from) & ci.ksq;

            // En passant capture with check? We have already handled the case
            // of direct checks and ordinary discovered check, so the only case we
            // need to handle is the unusual case of a discovered check through
            // the captured pawn.
            case MoveType.ENPASSANT:
            {
                var capsq = Square.make_square(Square.file_of(to), Square.rank_of(from));
                var b = (pieces() ^ from ^ capsq) | to;

                return (Utils.attacks_bb(PieceType.ROOK, ci.ksq, b)
                        & pieces(sideToMove, PieceType.QUEEN, PieceType.ROOK))
                       | (Utils.attacks_bb(PieceType.BISHOP, ci.ksq, b)
                          & pieces(sideToMove, PieceType.QUEEN, PieceType.BISHOP));
            }
            case MoveType.CASTLING:
            {
                var kfrom = from;
                var rfrom = to; // Castling is encoded as 'King captures the rook'
                var kto = Square.relative_square(sideToMove, rfrom > kfrom ? Square.SQ_G1 : Square.SQ_C1);
                var rto = Square.relative_square(sideToMove, rfrom > kfrom ? Square.SQ_F1 : Square.SQ_D1);

                return (bool) (Utils.PseudoAttacks[PieceType.ROOK_C, (int)rto] & ci.ksq)
                       && (Utils.attacks_bb(PieceType.ROOK, rto, (pieces() ^ kfrom ^ rfrom) | rto | kto)
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
    internal void do_move(Move m, StateInfo newSt, bool givesCheck)
    {
        Debug.Assert(Move.is_ok(m));
        Debug.Assert(newSt != st);

        ++nodes;
        var k = st.key ^ Zobrist.side;

        // Copy some fields of the old state to our new StateInfo object except the
        // ones which are going to be recalculated from scratch anyway and then switch
        // our state pointer to point to the new (ready to be updated) state.
        newSt.copyFrom(st);

        newSt.previous = st;
        st = newSt;

        // Increment ply counters. In particular, rule50 will be reset to zero later on
        // in case of a capture or a pawn move.
        ++gamePly;
        ++st.rule50;
        ++st.pliesFromNull;

        var us = sideToMove;
        var them = ~us;
        var from = Move.from_sq(m);
        var to = Move.to_sq(m);
        var pt = (int)Piece.type_of(piece_on(from));
        var captured = Move.type_of(m) == MoveType.ENPASSANT ? PieceType.PAWN_C : (int)Piece.type_of(piece_on(to));

        Debug.Assert(Piece.color_of(piece_on(from)) == us);
        Debug.Assert(
            piece_on(to) == Piece.NO_PIECE
            || Piece.color_of(piece_on(to)) == (Move.type_of(m) != MoveType.CASTLING ? them : us));
        Debug.Assert(captured != PieceType.KING_C);

        if (Move.type_of(m) == MoveType.CASTLING)
        {
            Debug.Assert(pt == PieceType.KING_C);

            Square rfrom, rto;
            do_castling(true, us, from, ref to, out rfrom, out rto);

            captured = PieceType.NO_PIECE_TYPE_C;
            st.psq += PSQT.psq[us.ValueMe, PieceType.ROOK_C, (int)rto] - PSQT.psq[us.ValueMe, PieceType.ROOK_C, (int)rfrom];
            k ^= Zobrist.psq[us.ValueMe, PieceType.ROOK_C, (int)rfrom] ^ Zobrist.psq[us.ValueMe, PieceType.ROOK_C, (int)rto];
        }

        if (captured != 0)
        {
            var capsq = to;

            // If the captured piece is a pawn, update pawn hash key, otherwise
            // update non-pawn material.
            if (captured == PieceType.PAWN_C)
            {
                if (Move.type_of(m) == MoveType.ENPASSANT)
                {
                    capsq -= Square.pawn_push(us);

                    Debug.Assert(pt == PieceType.PAWN_C);
                    Debug.Assert(to == st.epSquare);
                    Debug.Assert(Rank.relative_rank(us, to) == Rank.RANK_6);
                    Debug.Assert(piece_on(to) == Piece.NO_PIECE);
                    Debug.Assert(piece_on(capsq) == Piece.make_piece(them, PieceType.PAWN));

                    board[(int)capsq] = Piece.NO_PIECE; // Not done by remove_piece()
                }

                st.pawnKey ^= Zobrist.psq[them.ValueMe, PieceType.PAWN_C, (int)capsq];
            }
            else
            {
                st.nonPawnMaterial[them.ValueMe] -= Value.PieceValue[(int) Phase.MG][captured];
            }

            // Update board and piece lists
            remove_piece(them, PieceType.Create(captured), capsq);

            // Update material hash key and prefetch access to materialTable
            k ^= Zobrist.psq[them.ValueMe, captured, (int)capsq];
            st.materialKey ^= Zobrist.psq[them.ValueMe, captured, pieceCount[them.ValueMe, captured]];

            // Update incremental scores
            st.psq -= PSQT.psq[them.ValueMe, captured, (int)capsq];

            // Reset rule 50 counter
            st.rule50 = 0;
        }

        // Update hash key
        k ^= Zobrist.psq[us.ValueMe, pt, (int)from] ^ Zobrist.psq[us.ValueMe, pt, (int)to];

        // Reset en passant square
        if (st.epSquare != Square.SQ_NONE)
        {
            k ^= Zobrist.enpassant[Square.file_of(st.epSquare)];
            st.epSquare = Square.SQ_NONE;
        }

        // Update castling rights if needed
        if (st.castlingRights != 0 && ((castlingRightsMask[(int)from] | castlingRightsMask[(int)to]) != 0))
        {
            var cr = castlingRightsMask[(int)from] | castlingRightsMask[(int)to];
            k ^= Zobrist.castling[st.castlingRights & cr];
            st.castlingRights &= ~cr;
        }

        // Move the piece. The tricky Chess960 castling is handled earlier
        if (Move.type_of(m) != MoveType.CASTLING)
        {
            move_piece(us, PieceType.Create(pt), from, to);
        }

        // If the moving piece is a pawn do some special extra work
        if (pt == PieceType.PAWN_C)
        {
            // Set en-passant square if the moved pawn can be captured
            if (((int)to ^ (int)from) == 16
                && (attacks_from(PieceType.PAWN, to - Square.pawn_push(us), us) & pieces(them, PieceType.PAWN)))
            {
                st.epSquare = (from + to)/2;
                k ^= Zobrist.enpassant[Square.file_of(st.epSquare)];
            }

            else if (Move.type_of(m) == MoveType.PROMOTION)
            {
                var promotion = (int)Move.promotion_type(m);

                Debug.Assert(Rank.relative_rank(us, to) == Rank.RANK_8);
                Debug.Assert(promotion >= PieceType.KNIGHT_C && promotion <= PieceType.QUEEN_C);

                remove_piece(us, PieceType.PAWN, to);
                put_piece(us, PieceType.Create(promotion), to);

                // Update hash keys
                k ^= Zobrist.psq[us.ValueMe, PieceType.PAWN_C, (int)to] ^ Zobrist.psq[us.ValueMe, promotion, (int)to];
                st.pawnKey ^= Zobrist.psq[us.ValueMe, PieceType.PAWN_C, (int)to];
                st.materialKey ^= Zobrist.psq[us.ValueMe, promotion, pieceCount[us.ValueMe, promotion] - 1]
                                  ^ Zobrist.psq[us.ValueMe, PieceType.PAWN_C, pieceCount[us.ValueMe, PieceType.PAWN_C]];

                // Update incremental score
                st.psq += PSQT.psq[us.ValueMe, promotion, (int)to] - PSQT.psq[us.ValueMe, PieceType.PAWN_C, (int)to];

                // Update material
                st.nonPawnMaterial[us.ValueMe] += Value.PieceValue[(int) Phase.MG][promotion];
            }

            // Update pawn hash key and prefetch access to pawnsTable
            st.pawnKey ^= Zobrist.psq[us.ValueMe, PieceType.PAWN_C, (int)from] ^ Zobrist.psq[us.ValueMe, PieceType.PAWN_C, (int)to];

            // Reset rule 50 draw counter
            st.rule50 = 0;
        }

        // Update incremental scores
        st.psq += PSQT.psq[us.ValueMe, pt, (int)to] - PSQT.psq[us.ValueMe, pt, (int)from];

        // Set capture piece
        st.capturedType = PieceType.Create(captured);

        // Update the key with the final value
        st.key = k;

        // Calculate checkers bitboard (if move gives check)
        st.checkersBB = givesCheck
            ? attackers_to(square(PieceType.KING, them)) & pieces(us)
            : new Bitboard(0);

        sideToMove = ~sideToMove;

        Debug.Assert(pos_is_ok());
    }

    /// Position::undo_move() unmakes a move. When it returns, the position should
    /// be restored to exactly the same state as before the move was made.
    internal void undo_move(Move m)
    {
        Debug.Assert(Move.is_ok(m));

        sideToMove = ~sideToMove;

        var us = sideToMove;
        var from = Move.from_sq(m);
        var to = Move.to_sq(m);
        var pt = Piece.type_of(piece_on(to));

        Debug.Assert(empty(from) || Move.type_of(m) == MoveType.CASTLING);
        Debug.Assert(st.capturedType != PieceType.KING);

        if (Move.type_of(m) == MoveType.PROMOTION)
        {
            Debug.Assert(Rank.relative_rank(us, to) == Rank.RANK_8);
            Debug.Assert(pt == Move.promotion_type(m));
            Debug.Assert((int)pt >= PieceType.KNIGHT_C && (int)pt <= PieceType.QUEEN_C);

            remove_piece(us, pt, to);
            put_piece(us, PieceType.PAWN, to);
            pt = PieceType.PAWN;
        }

        if (Move.type_of(m) == MoveType.CASTLING)
        {
            Square rfrom, rto;
            do_castling(false, us, from, ref to, out rfrom, out rto);
        }
        else
        {
            move_piece(us, pt, to, from); // Put the piece back at the source square

            if ((bool)st.capturedType)
            {
                var capsq = to;

                if (Move.type_of(m) == MoveType.ENPASSANT)
                {
                    capsq -= Square.pawn_push(us);

                    Debug.Assert(pt == PieceType.PAWN);
                    Debug.Assert(to == st.previous.epSquare);
                    Debug.Assert(Rank.relative_rank(us, to) == Rank.RANK_6);
                    Debug.Assert(piece_on(capsq) == Piece.NO_PIECE);
                    Debug.Assert(st.capturedType == PieceType.PAWN);
                }

                put_piece(~us, st.capturedType, capsq); // Restore the captured piece
            }
        }

        // Finally point our state pointer back to the previous state
        st = st.previous;
        --gamePly;

        Debug.Assert(pos_is_ok());
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
        remove_piece(us, PieceType.KING, Do ? from : to);
        remove_piece(us, PieceType.ROOK, Do ? rfrom : rto);
        board[Do ? (int)from : (int)to] = board[Do ? (int)rfrom : (int)rto] = Piece.NO_PIECE;
        // Since remove_piece doesn't do it for us
        put_piece(us, PieceType.KING, Do ? to : from);
        put_piece(us, PieceType.ROOK, Do ? rto : rfrom);
    }

    /// Position::do(undo)_null_move() is used to do(undo) a "null move": It flips
    /// the side to move without executing any move on the board.
    internal void do_null_move(StateInfo newSt)
    {
        Debug.Assert(!checkers());
        Debug.Assert(newSt != st);

        newSt.copyFrom(st);

        newSt.previous = st;
        st = newSt;

        if (st.epSquare != Square.SQ_NONE)
        {
            st.key ^= Zobrist.enpassant[Square.file_of(st.epSquare)];
            st.epSquare = Square.SQ_NONE;
        }

        st.key ^= Zobrist.side;

        ++st.rule50;
        st.pliesFromNull = 0;

        sideToMove = ~sideToMove;

        Debug.Assert(pos_is_ok());
    }

    internal void undo_null_move()
    {
        Debug.Assert(!checkers());

        st = st.previous;
        sideToMove = ~sideToMove;
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
        var b = stmAttackers & bb[(int)Pt];
        if (!b)
        {
            return min_attacker(Pt + 1, bb, to, stmAttackers, ref occupied, ref attackers);
        }

        occupied ^= b & ~(b - new Bitboard(1));

        if (Pt == PieceType.PAWN || Pt == PieceType.BISHOP || Pt == PieceType.QUEEN)
        {
            attackers |= Utils.attacks_bb(PieceType.BISHOP, to, occupied) & (bb[PieceType.BISHOP_C] | bb[PieceType.QUEEN_C]);
        }

        if (Pt == PieceType.ROOK || Pt == PieceType.QUEEN)
        {
            attackers |= Utils.attacks_bb(PieceType.ROOK, to, occupied) & (bb[PieceType.ROOK_C] | bb[PieceType.QUEEN_C]);
        }

        attackers &= occupied; // After X-ray that may add already processed pieces
        return Pt;
    }

    /// Position::key_after() computes the new hash key after the given move. Needed
    /// for speculative prefetch. It doesn't recognize special moves like castling,
    /// en-passant and promotions.
    private ulong key_after(Move m)
    {
        var us = sideToMove;
        var from = Move.from_sq(m);
        var to = Move.to_sq(m);
        var pt = (int)Piece.type_of(piece_on(from));
        var captured = (int)Piece.type_of(piece_on(to));
        var k = st.key ^ Zobrist.side;

        if (captured!=0)
        {
            k ^= Zobrist.psq[us.ValueThem, captured, (int)to];
        }

        return k ^ Zobrist.psq[us.ValueMe, pt, (int)to] ^ Zobrist.psq[us.ValueMe, pt, (int)from];
    }

    /// Position::see() is a static exchange evaluator: It tries to estimate the
    /// material gain or loss resulting from a move.
    internal Value see_sign(Move m)
    {
        Debug.Assert(Move.is_ok(m));

        // Early return if SEE cannot be negative because captured piece value
        // is not less then capturing one. Note that king moves always return
        // here because king midgame value is set to 0.
        if (Value.PieceValue[(int) Phase.MG][(int)moved_piece(m)]
            <= Value.PieceValue[(int) Phase.MG][(int)piece_on(Move.to_sq(m))])
        {
            return Value.VALUE_KNOWN_WIN;
        }

        return see(m);
    }

    internal Value see(Move m)
    {
        var swapList = new Value[32];
        var slIndex = 1;

        Debug.Assert(Move.is_ok(m));

        var @from = Move.from_sq(m);
        var to = Move.to_sq(m);
        swapList[0] = Value.PieceValue[(int) Phase.MG][(int)piece_on(to)];
        var stm = Piece.color_of(piece_on(@from));
        var occupied = pieces() ^ @from;

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
            swapList[0] = Value.PieceValue[(int) Phase.MG][PieceType.PAWN_C];
        }

        // Find all attackers to the destination square, with the moving piece
        // removed, but possibly an X-ray attacker added behind it.
        var attackers = attackers_to(to, occupied) & occupied;

        // If the opponent has no attackers we are finished
        stm = ~stm;
        var stmAttackers = attackers & pieces(stm);
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
        var captured = (int)Piece.type_of(piece_on(@from));

        do
        {
            Debug.Assert(slIndex < 32);

            // Add the new entry to the swap list
            swapList[slIndex] = -swapList[slIndex - 1] + Value.PieceValue[(int) Phase.MG][captured];

            // Locate and remove the next least valuable attacker
            captured = (int)min_attacker(PieceType.PAWN, byTypeBB, to, stmAttackers, ref occupied, ref attackers);
            stm = ~stm;
            stmAttackers = attackers & pieces(stm);
            ++slIndex;
        } while (stmAttackers && (captured != PieceType.KING_C || DecreaseValue(ref slIndex)));
            // Stop before a king capture

        // Having built the swap list, we negamax through it to find the best
        // achievable score from the point of view of the side to move.
        while (--slIndex != 0)
        {
            swapList[slIndex - 1] = new Value(Math.Min(-swapList[slIndex], swapList[slIndex - 1]));
        }

        return swapList[0];
    }

    private static bool DecreaseValue(ref int value)
    {
        --value;
        return false;
    }

    /// Position::is_draw() tests whether the position is drawn by 50-move rule
    /// or by repetition. It does not detect stalemates.
    internal bool is_draw()
    {
        if (st.rule50 > 99 && (!checkers() || new MoveList(GenType.LEGAL, this).size() > 0))
        {
            return true;
        }

        var stp = st;
        for (int i = 2, e = Math.Min(st.rule50, st.pliesFromNull); i <= e; i += 2)
        {
            stp = stp.previous.previous;

            if (stp.key == st.key)
            {
                return true; // Draw at first repetition
            }
        }

        return false;
    }

    private bool pos_is_ok()
    {
        const bool Fast = true; // Quick (default) or full check?

        for (var step = (int) CheckStep.Default;
            step <= (Fast ? (int) CheckStep.Default : (int) CheckStep.Castling);
            step++)
        {
            if (step == (int) CheckStep.Default)
            {
                if (sideToMove != Color.WHITE && sideToMove != Color.BLACK)
                {
                    return false;
                }

                if (piece_on(square(PieceType.KING, Color.WHITE)) != Piece.W_KING)
                {
                    return false;
                }

                if (piece_on(square(PieceType.KING, Color.BLACK)) != Piece.B_KING)
                {
                    return false;
                }

                var relRank = Rank.relative_rank(sideToMove, ep_square());
                if (ep_square() != Square.SQ_NONE && relRank != Rank.RANK_6)
                {
                    return false;
                }
            }

            if (step == (int) CheckStep.King)
            {
                if (board.Count(piece => piece == Piece.W_KING) != 1
                    || board.Count(piece => piece == Piece.B_KING) != 1
                    || attackers_to(square(PieceType.KING, ~sideToMove)) & pieces(sideToMove))
                {
                    return false;
                }
            }

            if (step == (int) CheckStep.Bitboards)
            {
                if ((pieces(Color.WHITE) & pieces(Color.BLACK))
                    || (pieces(Color.WHITE) | pieces(Color.BLACK)) != pieces())
                {
                    return false;
                }

                for (var p1 = PieceType.PAWN_C; p1 <= PieceType.KING_C; ++p1)
                {
                    for (var p2 = PieceType.PAWN_C; p2 <= PieceType.KING_C; ++p2)
                    {
                        if (p1 != p2 && (pieces(PieceType.Create(p1)) & pieces(PieceType.Create(p2))))
                        {
                            return false;
                        }
                    }
                }
            }

            if (step == (int) CheckStep.Lists)
            {
                for (var c = Color.WHITE_C; c <= Color.BLACK_C; ++c)
                {
                    for (var pt = PieceType.PAWN_C; pt <= PieceType.KING_C; ++pt)
                    {
                        var pieceType = PieceType.Create(pt);
                        if (pieceCount[c, pt] != Bitcount.popcount_Full(pieces(Color.Create(c), pieceType)))
                        {
                            return false;
                        }

                        for (var i = 0; i < pieceCount[c, pt]; ++i)
                        {
                            if (board[(int)pieceList[c, pt, i]] != Piece.make_piece(c, pieceType)
                                || index[(int)pieceList[c, pt, i]] != i)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            if (step == (int) CheckStep.Castling)
            {
                for (var c = Color.WHITE_C; c <= Color.BLACK_C; ++c)
                {
                    for (var s = CastlingSide.KING_SIDE; s <= CastlingSide.QUEEN_SIDE; s++)
                    {
                        var castlingSideColor = Color.Create(c) | s;
                        if (!can_castle(castlingSideColor))
                        {
                            continue;
                        }

                        if (piece_on(castlingRookSquare[(int) (castlingSideColor)]) != Piece.make_piece(c, PieceType.ROOK)
                            || castlingRightsMask[(int)castlingRookSquare[(int) (castlingSideColor)]] != (int) (castlingSideColor)
                            || (castlingRightsMask[(int)square(PieceType.KING, Color.Create(c))] & (int) (castlingSideColor)) != (int) (castlingSideColor))
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return true;
    }

    internal string fen()
    {
        var ss = new StringBuilder();

        for (var r = Rank.RANK_8; r >= Rank.RANK_1; --r)
        {
            for (var f = File.FILE_A; f <= File.FILE_H; ++f)
            {
                int emptyCnt;
                for (emptyCnt = 0; f <= File.FILE_H && empty(Square.make_square(f, r)); ++f)
                {
                    ++emptyCnt;
                }

                if (emptyCnt != 0)
                {
                    ss.Append(emptyCnt);
                }

                if (f <= File.FILE_H)
                {
                    ss.Append(PieceToChar[(int)piece_on(Square.make_square(f, r))]);
                }
            }

            if (r > Rank.RANK_1)
            {
                ss.Append('/');
            }
        }

        ss.Append((sideToMove == Color.WHITE ? " w " : " b "));

        if (can_castle(CastlingRight.WHITE_OO))
        {
            ss.Append(
                (chess960
                    ? (char) ('A' + Square.file_of(castling_rook_square(Color.WHITE | CastlingSide.KING_SIDE)))
                    : 'K'));
        }

        if (can_castle(CastlingRight.WHITE_OOO))
        {
            ss.Append(
                (chess960
                    ? (char) ('A' + Square.file_of(castling_rook_square(Color.WHITE | CastlingSide.QUEEN_SIDE)))
                    : 'Q'));
        }

        if (can_castle(CastlingRight.BLACK_OO))
        {
            ss.Append(
                (chess960
                    ? (char) ('a' + Square.file_of(castling_rook_square(Color.BLACK | CastlingSide.KING_SIDE)))
                    : 'k'));
        }

        if (can_castle(CastlingRight.BLACK_OOO))
        {
            ss.Append(
                (chess960
                    ? (char) ('a' + Square.file_of(castling_rook_square(Color.BLACK | CastlingSide.QUEEN_SIDE)))
                    : 'q'));
        }

        if (can_castle(Color.WHITE) == 0 && can_castle(Color.BLACK) == 0)
        {
            ss.Append('-');
        }

        ss.Append((ep_square() == Square.SQ_NONE ? " - " : " " + UCI.square(ep_square()) + " "));
        ss.Append(st.rule50);
        ss.Append(" ");
        ss.Append(1 + (gamePly - (sideToMove == Color.BLACK ? 1 : 0))/2);

        return ss.ToString();
    }

    internal ulong exclusion_key()
    {
        return st.key ^ Zobrist.exclusion;
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

    internal static char tolower(char token)
    {
        return token.ToString().ToLowerInvariant()[0];
    }

    internal static Stack<string> CreateStack(string input)
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
    internal void set(string fenStr, bool isChess960, Thread th)
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
        char token;
        var sq = Square.SQ_A8;

        var fen = fenStr.ToCharArray();
        var fenPos = 0;
        clear();

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
                var p = PieceToChar.IndexOf(token);
                if (p > -1)
                {
                    put_piece(Piece.color_of(Piece.Create(p)), Piece.type_of(Piece.Create(p)), sq);
                    sq++;
                }
            }
        }

        // 2. Active color
        token = fen[fenPos++];
        sideToMove = (token == 'w' ? Color.WHITE : Color.BLACK);
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
                    Piece.type_of(piece_on(rsq)) != PieceType.ROOK;
                    rsq--)
                {
                }
            }
            else if (token == 'Q')
            {
                for (rsq = Square.relative_square(c, Square.SQ_A1);
                    Piece.type_of(piece_on(rsq)) != PieceType.ROOK;
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

            set_castling_right(c, rsq);
        }

        if (fenPos < fenStr.Length)
        {
            var col = fen[fenPos++];
            if (fenPos < fenStr.Length)
            {
                var row = fen[fenPos++];

                // 4. En passant square. Ignore if no pawn capture is possible
                if (((col >= 'a' && col <= 'h')) && ((row == '3' || row == '6')))
                {
                    st.epSquare = Square.make_square(new File(col - 'a'), new Rank(row - '1'));

                    if ((attackers_to(st.epSquare) & pieces(sideToMove, PieceType.PAWN)) == 0)
                    {
                        st.epSquare = Square.SQ_NONE;
                    }
                }
            }
        }

        // 5-6. Halfmove clock and fullmove number
        var tokens = CreateStack(fenStr.Substring(fenPos));
        if (tokens.Count > 0)
        {
            st.rule50 = int.Parse(tokens.Pop());
        }
        if (tokens.Count > 0)
        {
            gamePly = int.Parse(tokens.Pop());
        }

        // Convert from fullmove starting from 1 to ply starting from 0,
        // handle also common incorrect FEN with fullmove = 0.
        gamePly = Math.Max(2*(gamePly - 1), 0) + ((sideToMove == Color.BLACK) ? 1 : 0);

        chess960 = isChess960;
        thisThread = th;
        set_state(st);

        Debug.Assert(pos_is_ok());
    }

    /// clear() erases the position object to a pristine state, with an
    /// empty board, white to move, and no castling rights.
    internal void clear()
    {
        this.clearBoard();

        byColorBB = new Bitboard[Color.COLOR_NB_C];

        byTypeBB = new Bitboard[PieceType.PIECE_TYPE_NB_C];

        castlingPath = new Bitboard[(int) CastlingRight.CASTLING_RIGHT_NB];

        castlingRightsMask = new int[Square.SQUARE_NB_C];

        castlingRookSquare = new Square[(int) CastlingRight.CASTLING_RIGHT_NB];

        index = new int[Square.SQUARE_NB_C];

        pieceCount = new int[Color.COLOR_NB_C, PieceType.PIECE_TYPE_NB_C];

        pieceList = new Square[Color.COLOR_NB_C, PieceType.PIECE_TYPE_NB_C, 16];
        for (var i = 0; i < PieceType.PIECE_TYPE_NB_C; ++i)
        {
            for (var j = 0; j < 16; ++j)
            {
                pieceList[Color.WHITE_C, i, j] = pieceList[Color.BLACK_C, i, j] = Square.SQ_NONE;
            }
        }

        chess960 = false;

        gamePly = 0;

        nodes = 0;

        sideToMove = Color.WHITE;

        thisThread = null;
        startState = new StateInfo();

        st = startState;
    }

    /// Position::flip() flips position with the white and black sides reversed. This
    /// is only useful for debugging e.g. for finding evaluation symmetry bugs.
    internal void flip()
    {
        var tokens = CreateStack(fen());
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

        set(flippedFen.ToString(), chess960, this_thread());
    }

    internal string displayString()
    {
        var sb = new StringBuilder("\n +---+---+---+---+---+---+---+---+\n");
        for (var r = Rank.RANK_8; r >= Rank.RANK_1; --r)
        {
            for (var f = File.FILE_A; f <= File.FILE_H; ++f)
            {
                sb.Append(" | ");
                sb.Append(PieceToChar[(int)piece_on(Square.make_square(f, r))]);
            }

            sb.Append(" |\n +---+---+---+---+---+---+---+---+\n");
        }

        sb.Append($"\nFen: {fen()}\nKey: {st.key:X}\nCheckers: ");

        for (var b = checkers(); b;)
        {
            sb.Append(UCI.square(Utils.pop_lsb(ref b)) + " ");
        }
        sb.AppendLine();
        return sb.ToString();
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