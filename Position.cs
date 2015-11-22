using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

#if PRIMITIVE
using ColorT = System.Int32;
using PieceTypeT = System.Int32;
using PieceT = System.Int32;
using ValueT = System.Int32;
using ScoreT = System.Int32;
using SquareT = System.Int32;
using MoveT = System.Int32;
using BitboardT = System.UInt64;
#endif

/// Position class stores information regarding the board representation as
/// pieces, side to move, hash keys, castling info, etc. Important methods are
/// do_move() and undo_move(), used by the search to update node info when
/// traversing the search tree.
internal class Position
{
    internal const string PieceToChar = " PNBRQK  pnbrqk";

    // Data members
    private PieceT[] board = new PieceT[Square.SQUARE_NB];

    private BitboardT[] byColorBB = new BitboardT[Color.COLOR_NB];

    private BitboardT[] byTypeBB = new BitboardT[PieceType.PIECE_TYPE_NB];

    private BitboardT[] castlingPath = new BitboardT[(int) CastlingRight.CASTLING_RIGHT_NB];

    private int[] castlingRightsMask = new int[Square.SQUARE_NB];

    private SquareT[] castlingRookSquare = new SquareT[(int) CastlingRight.CASTLING_RIGHT_NB];

    private bool chess960;

    private int gamePly;

    private int[] index = new int[Square.SQUARE_NB];

    private int nodes;

    private int[,] pieceCount = new int[Color.COLOR_NB, PieceType.PIECE_TYPE_NB];

    private SquareT[,,] pieceList = new SquareT[Color.COLOR_NB, PieceType.PIECE_TYPE_NB, 16];

    private ColorT sideToMove;

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

        foreach (var c in Color.AllColors)
        {
            foreach (var pt in PieceType.AllPieceTypes)
            {
                for (var s = Square.SQ_A1; s <= Square.SQ_H8; ++s)
                {
                    Zobrist.psq[c, pt, s] = rng.rand();
                }
            }
        }

        foreach (var f in File.AllFiles)
        {
            Zobrist.enpassant[f] = rng.rand();
        }

        for (var cr = (int) CastlingRight.NO_CASTLING; cr <= (int) CastlingRight.ANY_CASTLING; ++cr)
        {
            Zobrist.castling[cr] = 0;
            var b = Bitboard.Create((ulong) cr);
            while (b != 0)
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
    internal BitboardT pieces()
    {
        return byTypeBB[PieceType.ALL_PIECES];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal ColorT side_to_move()
    {
        return sideToMove;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal bool empty(SquareT s)
    {
        return board[s] == Piece.NO_PIECE;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal PieceT piece_on(SquareT s)
    {
        return board[s];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal PieceT moved_piece(MoveT m)
    {
        return board[Move.from_sq(m)];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal BitboardT pieces_Pt(PieceTypeT pt)
    {
        return byTypeBB[pt];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal BitboardT pieces_PtPt(PieceTypeT pt1, PieceTypeT pt2)
    {
        return byTypeBB[pt1] | byTypeBB[pt2];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal BitboardT pieces_Ct(ColorT c)
    {
        return byColorBB[c];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal BitboardT pieces_CtPt(ColorT c, PieceTypeT pt)
    {
        return byColorBB[c] & byTypeBB[pt];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal BitboardT pieces_CtPtPt(ColorT c, PieceTypeT pt1, PieceTypeT pt2)
    {
        return byColorBB[c] & (byTypeBB[pt1] | byTypeBB[pt2]);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal int count(PieceTypeT Pt, ColorT c)
    {
        return pieceCount[c, Pt];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal SquareT square(PieceTypeT Pt, ColorT c, int idx)
    {
        return pieceList[c, Pt, idx];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal SquareT square(PieceTypeT Pt, ColorT c)
    {
        Debug.Assert(pieceCount[c, Pt] == 1);
        return pieceList[c, Pt, 0];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal SquareT ep_square()
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
    internal int can_castle(ColorT c)
    {
        return (st.castlingRights & (((int) CastlingRight.WHITE_OO | (int) CastlingRight.WHITE_OOO) << (2*c)));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal bool castling_impeded(CastlingRight cr)
    {
        return (byTypeBB[PieceType.ALL_PIECES] & castlingPath[(int) cr]) != 0;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal SquareT castling_rook_square(CastlingRight cr)
    {
        return castlingRookSquare[(int) cr];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal BitboardT attacks_from_PtS(PieceTypeT Pt, SquareT s)
    {
        return Pt == PieceType.BISHOP || Pt == PieceType.ROOK
            ? Utils.attacks_bb_PtSBb(Pt, s, byTypeBB[PieceType.ALL_PIECES])
            : Pt == PieceType.QUEEN
                ? attacks_from_PtS(PieceType.ROOK, s) | attacks_from_PtS(PieceType.BISHOP, s)
                : Utils.StepAttacksBB[Pt, s];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal BitboardT attacks_from_PS(PieceTypeT Pt, SquareT s, ColorT c)
    {
        Debug.Assert(Pt == PieceType.PAWN);
        return Utils.StepAttacksBB[Piece.make_piece(c, PieceType.PAWN), s];
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal BitboardT attacks_from(PieceT pc, SquareT s)
    {
        return Utils.attacks_bb_PSBb(pc, s, byTypeBB[PieceType.ALL_PIECES]);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal BitboardT attackers_to(SquareT s)
    {
        return attackers_to(s, byTypeBB[PieceType.ALL_PIECES]);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal BitboardT checkers()
    {
        return st.checkersBB;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal BitboardT discovered_check_candidates()
    {
        return check_blockers(sideToMove, Color.opposite(sideToMove));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal BitboardT pinned_pieces(ColorT c)
    {
        return check_blockers(c, c);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal bool pawn_passed(ColorT c, SquareT s)
    {
        return (pieces_CtPt(Color.opposite(c), PieceType.PAWN) & Utils.passed_pawn_mask(c, s)) == 0;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal bool advanced_pawn_push(MoveT m)
    {
        return Piece.type_of(moved_piece(m)) == PieceType.PAWN
               && Rank.relative_rank_CtSt(sideToMove, Move.from_sq(m)) > Rank.RANK_4;
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
    internal ScoreT psq_score()
    {
        return st.psq;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal ValueT non_pawn_material(ColorT c)
    {
        return st.nonPawnMaterial[c];
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
        return pieceCount[Color.WHITE, PieceType.BISHOP] == 1
               && pieceCount[Color.BLACK, PieceType.BISHOP] == 1
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
    internal bool capture_or_promotion(MoveT m)
    {
        Debug.Assert(Move.is_ok(m));
        return Move.type_of(m) != MoveType.NORMAL ? Move.type_of(m) != MoveType.CASTLING : !empty(Move.to_sq(m));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal bool capture(MoveT m)
    {
        // Castling is encoded as "king captures the rook"
        Debug.Assert(Move.is_ok(m));
        return (!empty(Move.to_sq(m)) && Move.type_of(m) != MoveType.CASTLING)
               || Move.type_of(m) == MoveType.ENPASSANT;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal PieceTypeT captured_piece_type()
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
    private void put_piece(ColorT c, PieceTypeT pieceType, SquareT s)
    {
        var pt = (int) pieceType;
        board[s] = Piece.make_piece(c, pieceType);
        byTypeBB[PieceType.ALL_PIECES] = Bitboard.OrWithSquare(byTypeBB[PieceType.ALL_PIECES], s);
        byTypeBB[pt] = Bitboard.OrWithSquare(byTypeBB[pt], s);
        byColorBB[c] = Bitboard.OrWithSquare(byColorBB[c], s);
        index[s] = pieceCount[c, pt]++;
        pieceList[c, pt, index[s]] = s;
        pieceCount[c, PieceType.ALL_PIECES]++;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void remove_piece(ColorT c, PieceTypeT pieceType, SquareT s)
    {
        var pt = (int) pieceType;
        // WARNING: This is not a reversible operation. If we remove a piece in
        // do_move() and then replace it in undo_move() we will put it at the end of
        // the list and not in its original place, it means index[] and pieceList[]
        // are not guaranteed to be invariant to a do_move() + undo_move() sequence.
        byTypeBB[PieceType.ALL_PIECES] = Bitboard.XorWithSquare(byTypeBB[PieceType.ALL_PIECES], s);
        byTypeBB[pt] = Bitboard.XorWithSquare(byTypeBB[pt], s);
        byColorBB[c] = Bitboard.XorWithSquare(byColorBB[c], s);
        /* board[s] = NO_PIECE;  Not needed, overwritten by the capturing one */
        var lastSquare = pieceList[c, pt, --pieceCount[c, pt]];
        index[lastSquare] = index[s];
        pieceList[c, pt, index[lastSquare]] = lastSquare;
        pieceList[c, pt, pieceCount[c, pt]] = Square.SQ_NONE;
        pieceCount[c, PieceType.ALL_PIECES]--;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    private void move_piece(ColorT c, PieceTypeT pieceType, SquareT from, SquareT to)
    {
        var pt = (int) pieceType;
        // index[from] is not updated and becomes stale. This works as long as index[]
        // is accessed just by known occupied squares.
        var from_to_bb = Utils.SquareBB[from] ^ Utils.SquareBB[to];
        byTypeBB[PieceType.ALL_PIECES] ^= from_to_bb;
        byTypeBB[pt] ^= from_to_bb;
        byColorBB[c] ^= from_to_bb;
        board[from] = Piece.NO_PIECE;
        board[to] = Piece.make_piece(c, pieceType);
        index[to] = index[from];
        pieceList[c, pt, index[to]] = to;
    }

    /// Position::set_castling_right() is a helper function used to set castling
    /// rights given the corresponding color and the rook starting square.
    private void set_castling_right(ColorT c, SquareT rfrom)
    {
        var kfrom = square(PieceType.KING, c);
        var cs = kfrom < rfrom ? CastlingSide.KING_SIDE : CastlingSide.QUEEN_SIDE;
        var cr = Color.CalculateCastlingRight(c, cs);

        st.castlingRights |= (int) cr;
        castlingRightsMask[kfrom] |= (int) cr;
        castlingRightsMask[rfrom] |= (int) cr;
        castlingRookSquare[(int) cr] = rfrom;

        var kto = Square.relative_square(c, cs == CastlingSide.KING_SIDE ? Square.SQ_G1 : Square.SQ_C1);
        var rto = Square.relative_square(c, cs == CastlingSide.KING_SIDE ? Square.SQ_F1 : Square.SQ_D1);

        for (var s = rfrom < rto ? rfrom : rto; s <= (rfrom > rto ? rfrom : rto); ++s)
        {
            if (s != kfrom && s != rfrom)
            {
                castlingPath[(int) cr] = Bitboard.OrWithSquare(castlingPath[(int)cr], s);
            }
        }

        for (var s = kfrom < kto ? kfrom : kto; s <= (kfrom > kto ? kfrom : kto); ++s)
        {
            if (s != kfrom && s != rfrom)
            {
                castlingPath[(int) cr] = Bitboard.OrWithSquare(castlingPath[(int)cr], s);
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

        si.checkersBB = attackers_to(square(PieceType.KING, sideToMove)) & pieces_Ct(Color.opposite(sideToMove));

        for (var b = pieces(); b != 0;)
        {
            var s = Utils.pop_lsb(ref b);
            var pc = piece_on(s);
            var color = Piece.color_of(pc);
            var pieceType = (int) Piece.type_of(pc);
            si.key ^= Zobrist.psq[color, pieceType, s];
            si.psq += PSQT.psq[color, pieceType, s];
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

        for (var b = pieces_Pt(PieceType.PAWN); b != 0;)
        {
            var s = Utils.pop_lsb(ref b);
            si.pawnKey ^= Zobrist.psq[Piece.color_of(piece_on(s)), PieceType.PAWN, s];
        }

        foreach (var c in Color.AllColors)
        {
            foreach (var pt in PieceType.AllPieceTypes)
            {
                for (var cnt = 0; cnt < pieceCount[c, pt]; ++cnt)
                {
                    si.materialKey ^= Zobrist.psq[c, pt, cnt];
                }
            }
        }

        foreach (var c in Color.AllColors)
        {
            for (var pt = (int)PieceType.KNIGHT; pt <= PieceType.QUEEN; ++pt)
            {
                si.nonPawnMaterial[c] += pieceCount[c, pt]*Value.PieceValue[(int) Phase.MG][pt];
            }
        }
    }

    /// Position::game_phase() calculates the game phase interpolating total non-pawn
    /// material between endgame and midgame limits.
    internal Phase game_phase()
    {
        var npm = st.nonPawnMaterial[Color.WHITE] + st.nonPawnMaterial[Color.BLACK];

        npm = Value.Create(Math.Max(Value.EndgameLimit, Math.Min(npm, Value.MidgameLimit)));

        return
            (Phase) (((npm - Value.EndgameLimit)*(int) Phase.PHASE_MIDGAME)/(Value.MidgameLimit - Value.EndgameLimit));
    }

    /// Position::check_blockers() returns a bitboard of all the pieces with color
    /// 'c' that are blocking check on the king with color 'kingColor'. A piece
    /// blocks a check if removing that piece from the board would result in a
    /// position where the king is in check. A check blocking piece can be either a
    /// pinned or a discovered check piece, according if its color 'c' is the same
    /// or the opposite of 'kingColor'.
    private BitboardT check_blockers(ColorT c, ColorT kingColor)
    {
        BitboardT result = Bitboard.Create(0);
        var ksq = square(PieceType.KING, kingColor);

        // Pinners are sliders that give check when a pinned piece is removed
        var pinners = ((pieces_PtPt(PieceType.ROOK, PieceType.QUEEN) & Utils.PseudoAttacks[PieceType.ROOK, ksq])
                            | (pieces_PtPt(PieceType.BISHOP, PieceType.QUEEN) & Utils.PseudoAttacks[PieceType.BISHOP, ksq]))
                           & pieces_Ct(Color.opposite(kingColor));

        while (pinners != 0)
        {
            var b = Utils.between_bb(ksq, Utils.pop_lsb(ref pinners)) & pieces();

            if (!Bitboard.more_than_one(b))
            {
                result |= b & pieces_Ct(c);
            }
        }
        return result;
    }

    /// Position::attackers_to() computes a bitboard of all pieces which attack a
    /// given square. Slider attacks use the occupied bitboard to indicate occupancy.
    private BitboardT attackers_to(SquareT s, BitboardT occupied)
    {
        return (attacks_from_PS(PieceType.PAWN, s, Color.BLACK) & pieces_CtPt(Color.WHITE, PieceType.PAWN))
               | (attacks_from_PS(PieceType.PAWN, s, Color.WHITE) & pieces_CtPt(Color.BLACK, PieceType.PAWN))
               | (attacks_from_PtS(PieceType.KNIGHT, s) & pieces_Pt(PieceType.KNIGHT))
               | (Utils.attacks_bb_PtSBb(PieceType.ROOK, s, occupied) & pieces_PtPt(PieceType.ROOK, PieceType.QUEEN))
               | (Utils.attacks_bb_PtSBb(PieceType.BISHOP, s, occupied) & pieces_PtPt(PieceType.BISHOP, PieceType.QUEEN))
               | (attacks_from_PtS(PieceType.KING, s) & pieces_Pt(PieceType.KING));
    }

    /// Position::legal() tests whether a pseudo-legal move is legal
    internal bool legal(MoveT m, BitboardT pinned)
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
            var occupied = Bitboard.OrWithSquare(Bitboard.XorWithSquare(Bitboard.XorWithSquare(pieces(), from), capsq), to);

            Debug.Assert(to == ep_square());
            Debug.Assert(moved_piece(m) == Piece.make_piece(us, PieceType.PAWN));
            Debug.Assert(piece_on(capsq) == Piece.make_piece(Color.opposite(us), PieceType.PAWN));
            Debug.Assert(piece_on(to) == Piece.NO_PIECE);

            return
                (Utils.attacks_bb_PtSBb(PieceType.ROOK, ksq, occupied) & pieces_CtPtPt(Color.opposite(us), PieceType.QUEEN, PieceType.ROOK)) == 0
                && (Utils.attacks_bb_PtSBb(PieceType.BISHOP, ksq, occupied)
                     & pieces_CtPtPt(Color.opposite(us), PieceType.QUEEN, PieceType.BISHOP)) == 0;
        }

        // If the moving piece is a king, check whether the destination
        // square is attacked by the opponent. Castling moves are checked
        // for legality during move generation.
        if (Piece.type_of(piece_on(from)) == PieceType.KING)
        {
            return Move.type_of(m) == MoveType.CASTLING || (attackers_to(Move.to_sq(m)) & pieces_Ct(Color.opposite(us))) == 0;
        }

        // A non-king move is legal if and only if it is not pinned or it
        // is moving along the ray towards or away from the king.
        return pinned == 0 || Bitboard.AndWithSquare(pinned, from) ==0 || Utils.aligned(from, Move.to_sq(m), square(PieceType.KING, us));
    }

    /// Position::pseudo_legal() takes a random move and tests whether the move is
    /// pseudo legal. It is used to validate moves from TT that can be corrupted
    /// due to SMP concurrent access or hash position key aliasing.
    internal bool pseudo_legal(MoveT m)
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
        if (Bitboard.AndWithSquare(pieces_Ct(us), to)!=0)
        {
            return false;
        }

        // Handle the special case of a pawn move
        if (Piece.type_of(pc) == PieceType.PAWN)
        {
            // We have already handled promotion moves, so destination
            // cannot be on the 8th/1st rank.
            if (Square.rank_of(to) == Rank.relative_rank_CtRt(us, Rank.RANK_8))
            {
                return false;
            }

            if (Bitboard.AndWithSquare(attacks_from_PS(PieceType.PAWN, from, us) & pieces_Ct(Color.opposite(us)), to)==0 // Not a capture
                && !((from + Square.pawn_push(us) == to) && empty(to)) // Not a single push
                && !((from + 2*Square.pawn_push(us) == to) // Not a double push
                     && (Square.rank_of(from) == Rank.relative_rank_CtRt(us, Rank.RANK_2)) && empty(to)
                     && empty(to - Square.pawn_push(us))))
            {
                return false;
            }
        }
        else if (Bitboard.AndWithSquare(attacks_from(pc, from), to)==0)
        {
            return false;
        }

        // Evasions generator already takes care to avoid some kind of illegal moves
        // and legal() relies on this. We therefore have to take care that the same
        // kind of moves are filtered out here.
        if (checkers() != 0)
        {
            if (Piece.type_of(pc) != PieceType.KING)
            {
                // Double check? In this case a king move is required
                if (Bitboard.more_than_one(checkers()))
                {
                    return false;
                }

                // Our move must be a blocking evasion or a capture of the checking piece
                if (Bitboard.AndWithSquare((Utils.between_bb(Utils.lsb(checkers()), square(PieceType.KING, us)) | checkers()), to)==0)
                {
                    return false;
                }
            }
            // In case of king moves under check we have to remove king so as to catch
            // invalid moves like b1a1 when opposite queen is on c1.
            else if ((attackers_to(to, Bitboard.XorWithSquare(pieces(), from)) & pieces_Ct(Color.opposite(us))) != 0)
            {
                return false;
            }
        }

        return true;
    }

    /// Position::gives_check() tests whether a pseudo-legal move gives a check
    internal bool gives_check(MoveT m, CheckInfo ci)
    {
        Debug.Assert(Move.is_ok(m));
        Debug.Assert(ci.dcCandidates == discovered_check_candidates());
        Debug.Assert(Piece.color_of(moved_piece(m)) == sideToMove);

        var from = Move.from_sq(m);
        var to = Move.to_sq(m);

        // Is there a direct check?
        if (Bitboard.AndWithSquare(ci.checkSquares[Piece.type_of(piece_on(from))], to)!=0)
        {
            return true;
        }

        // Is there a discovered check?
        if (ci.dcCandidates != 0 && Bitboard.AndWithSquare(ci.dcCandidates, from)!=0 && !Utils.aligned(from, to, ci.ksq))
        {
            return true;
        }

        switch (Move.type_of(m))
        {
            case MoveType.NORMAL:
                return false;

            case MoveType.PROMOTION:
                return Bitboard.AndWithSquare(Utils.attacks_bb_PSBb(Piece.Create(Move.promotion_type(m)), to, Bitboard.XorWithSquare(pieces(), from)), ci.ksq) != 0;

            // En passant capture with check? We have already handled the case
            // of direct checks and ordinary discovered check, so the only case we
            // need to handle is the unusual case of a discovered check through
            // the captured pawn.
            case MoveType.ENPASSANT:
            {
                var capsq = Square.make_square(Square.file_of(to), Square.rank_of(from));
                var b = Bitboard.OrWithSquare(Bitboard.XorWithSquare(Bitboard.XorWithSquare(pieces(), from), capsq), to);

                return ((Utils.attacks_bb_PtSBb(PieceType.ROOK, ci.ksq, b)
                        & pieces_CtPtPt(sideToMove, PieceType.QUEEN, PieceType.ROOK))
                       | (Utils.attacks_bb_PtSBb(PieceType.BISHOP, ci.ksq, b)
                          & pieces_CtPtPt(sideToMove, PieceType.QUEEN, PieceType.BISHOP))) != 0;
            }
            case MoveType.CASTLING:
            {
                var kfrom = from;
                var rfrom = to; // Castling is encoded as 'King captures the rook'
                var kto = Square.relative_square(sideToMove, rfrom > kfrom ? Square.SQ_G1 : Square.SQ_C1);
                var rto = Square.relative_square(sideToMove, rfrom > kfrom ? Square.SQ_F1 : Square.SQ_D1);

                var occupied = Bitboard.OrWithSquare(Bitboard.OrWithSquare(Bitboard.XorWithSquare(Bitboard.XorWithSquare(pieces(), kfrom), rfrom), rto), kto);
                return Bitboard.AndWithSquare(Utils.PseudoAttacks[PieceType.ROOK, rto], ci.ksq)!=0
                       && Bitboard.AndWithSquare(Utils.attacks_bb_PtSBb(PieceType.ROOK, rto, occupied), ci.ksq) !=0;
            }
            default:
                Debug.Assert(false);
                return false;
        }
    }

    /// Position::do_move() makes a move, and saves all information necessary
    /// to a StateInfo object. The move is assumed to be legal. Pseudo-legal
    /// moves should be filtered out before this function is called.
    internal void do_move(MoveT m, StateInfo newSt, bool givesCheck)
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
        var them = Color.opposite(us);
        var from = Move.from_sq(m);
        var to = Move.to_sq(m);
        var pt = (int)Piece.type_of(piece_on(from));
        var captured = Move.type_of(m) == MoveType.ENPASSANT ? PieceType.PAWN : Piece.type_of(piece_on(to));

        Debug.Assert(Piece.color_of(piece_on(from)) == us);
        Debug.Assert(
            piece_on(to) == Piece.NO_PIECE
            || Piece.color_of(piece_on(to)) == (Move.type_of(m) != MoveType.CASTLING ? them : us));
        Debug.Assert(captured != PieceType.KING);

        if (Move.type_of(m) == MoveType.CASTLING)
        {
            Debug.Assert(pt == PieceType.KING);

            SquareT rfrom, rto;
            do_castling(true, us, from, ref to, out rfrom, out rto);

            captured = PieceType.NO_PIECE_TYPE;
            st.psq += PSQT.psq[us, PieceType.ROOK, rto] - PSQT.psq[us, PieceType.ROOK, rfrom];
            k ^= Zobrist.psq[us, PieceType.ROOK, rfrom] ^ Zobrist.psq[us, PieceType.ROOK, rto];
        }

        if (captured != 0)
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
                    Debug.Assert(to == st.epSquare);
                    Debug.Assert(Rank.relative_rank_CtSt(us, to) == Rank.RANK_6);
                    Debug.Assert(piece_on(to) == Piece.NO_PIECE);
                    Debug.Assert(piece_on(capsq) == Piece.make_piece(them, PieceType.PAWN));

                    board[capsq] = Piece.NO_PIECE; // Not done by remove_piece()
                }

                st.pawnKey ^= Zobrist.psq[them, PieceType.PAWN, capsq];
            }
            else
            {
                st.nonPawnMaterial[them] -= Value.PieceValue[(int) Phase.MG][captured];
            }

            // Update board and piece lists
            remove_piece(them, PieceType.Create(captured), capsq);

            // Update material hash key and prefetch access to materialTable
            k ^= Zobrist.psq[them, captured, capsq];
            st.materialKey ^= Zobrist.psq[them, captured, pieceCount[them, captured]];

            // Update incremental scores
            st.psq -= PSQT.psq[them, captured, capsq];

            // Reset rule 50 counter
            st.rule50 = 0;
        }

        // Update hash key
        k ^= Zobrist.psq[us, pt, @from] ^ Zobrist.psq[us, pt, to];

        // Reset en passant square
        if (st.epSquare != Square.SQ_NONE)
        {
            k ^= Zobrist.enpassant[Square.file_of(st.epSquare)];
            st.epSquare = Square.SQ_NONE;
        }

        // Update castling rights if needed
        if (st.castlingRights != 0 && ((castlingRightsMask[@from] | castlingRightsMask[to]) != 0))
        {
            var cr = castlingRightsMask[@from] | castlingRightsMask[to];
            k ^= Zobrist.castling[st.castlingRights & cr];
            st.castlingRights &= ~cr;
        }

        // Move the piece. The tricky Chess960 castling is handled earlier
        if (Move.type_of(m) != MoveType.CASTLING)
        {
            move_piece(us, PieceType.Create(pt), from, to);
        }

        // If the moving piece is a pawn do some special extra work
        if (pt == PieceType.PAWN)
        {
            // Set en-passant square if the moved pawn can be captured
            if ((to ^ from) == 16
                && (attacks_from_PS(PieceType.PAWN, to - Square.pawn_push(us), us) & pieces_CtPt(them, PieceType.PAWN)) != 0)
            {
                st.epSquare = (from + to)/2;
                k ^= Zobrist.enpassant[Square.file_of(st.epSquare)];
            }

            else if (Move.type_of(m) == MoveType.PROMOTION)
            {
                var promotion = (int)Move.promotion_type(m);

                Debug.Assert(Rank.relative_rank_CtSt(us, to) == Rank.RANK_8);
                Debug.Assert(promotion >= PieceType.KNIGHT && promotion <= PieceType.QUEEN);

                remove_piece(us, PieceType.PAWN, to);
                put_piece(us, PieceType.Create(promotion), to);

                // Update hash keys
                k ^= Zobrist.psq[us, PieceType.PAWN, to] ^ Zobrist.psq[us, promotion, to];
                st.pawnKey ^= Zobrist.psq[us, PieceType.PAWN, to];
                st.materialKey ^= Zobrist.psq[us, promotion, pieceCount[us, promotion] - 1]
                                  ^ Zobrist.psq[us, PieceType.PAWN, pieceCount[us, PieceType.PAWN]];

                // Update incremental score
                st.psq += PSQT.psq[us, promotion, to] - PSQT.psq[us, PieceType.PAWN, to];

                // Update material
                st.nonPawnMaterial[us] += Value.PieceValue[(int) Phase.MG][promotion];
            }

            // Update pawn hash key and prefetch access to pawnsTable
            st.pawnKey ^= Zobrist.psq[us, PieceType.PAWN, from] ^ Zobrist.psq[us, PieceType.PAWN, to];

            // Reset rule 50 draw counter
            st.rule50 = 0;
        }

        // Update incremental scores
        st.psq += PSQT.psq[us, pt, to] - PSQT.psq[us, pt, @from];

        // Set capture piece
        st.capturedType = PieceType.Create(captured);

        // Update the key with the final value
        st.key = k;

        // Calculate checkers bitboard (if move gives check)
        st.checkersBB = givesCheck
            ? attackers_to(square(PieceType.KING, them)) & pieces_Ct(us)
            : Bitboard.Create(0);

        sideToMove = Color.opposite(sideToMove);

        Debug.Assert(pos_is_ok());
    }

    /// Position::undo_move() unmakes a move. When it returns, the position should
    /// be restored to exactly the same state as before the move was made.
    internal void undo_move(MoveT m)
    {
        Debug.Assert(Move.is_ok(m));

        sideToMove = Color.opposite(sideToMove);

        var us = sideToMove;
        var from = Move.from_sq(m);
        var to = Move.to_sq(m);
        var pt = Piece.type_of(piece_on(to));

        Debug.Assert(empty(from) || Move.type_of(m) == MoveType.CASTLING);
        Debug.Assert(st.capturedType != PieceType.KING);

        if (Move.type_of(m) == MoveType.PROMOTION)
        {
            Debug.Assert(Rank.relative_rank_CtSt(us, to) == Rank.RANK_8);
            Debug.Assert(pt == Move.promotion_type(m));
            Debug.Assert(pt >= PieceType.KNIGHT && pt <= PieceType.QUEEN);

            remove_piece(us, pt, to);
            put_piece(us, PieceType.PAWN, to);
            pt = PieceType.PAWN;
        }

        if (Move.type_of(m) == MoveType.CASTLING)
        {
            SquareT rfrom, rto;
            do_castling(false, us, from, ref to, out rfrom, out rto);
        }
        else
        {
            move_piece(us, pt, to, from); // Put the piece back at the source square

            if (st.capturedType != 0)
            {
                var capsq = to;

                if (Move.type_of(m) == MoveType.ENPASSANT)
                {
                    capsq -= Square.pawn_push(us);

                    Debug.Assert(pt == PieceType.PAWN);
                    Debug.Assert(to == st.previous.epSquare);
                    Debug.Assert(Rank.relative_rank_CtSt(us, to) == Rank.RANK_6);
                    Debug.Assert(piece_on(capsq) == Piece.NO_PIECE);
                    Debug.Assert(st.capturedType == PieceType.PAWN);
                }

                put_piece(Color.opposite(us), st.capturedType, capsq); // Restore the captured piece
            }
        }

        // Finally point our state pointer back to the previous state
        st = st.previous;
        --gamePly;

        Debug.Assert(pos_is_ok());
    }

    /// Position::do_castling() is a helper used to do/undo a castling move. This
    /// is a bit tricky, especially in Chess960.
    private void do_castling(bool Do, ColorT us, SquareT from, ref SquareT to, out SquareT rfrom, out SquareT rto)
    {
        var kingSide = to > from;
        rfrom = to; // Castling is encoded as "king captures friendly rook"
        rto = Square.relative_square(us, kingSide ? Square.SQ_F1 : Square.SQ_D1);
        to = Square.relative_square(us, kingSide ? Square.SQ_G1 : Square.SQ_C1);

        // Remove both pieces first since squares could overlap in Chess960
        remove_piece(us, PieceType.KING, Do ? from : to);
        remove_piece(us, PieceType.ROOK, Do ? rfrom : rto);
        board[Do ? @from : to] = board[Do ? rfrom : rto] = Piece.NO_PIECE;
        // Since remove_piece doesn't do it for us
        put_piece(us, PieceType.KING, Do ? to : from);
        put_piece(us, PieceType.ROOK, Do ? rto : rfrom);
    }

    /// Position::do(undo)_null_move() is used to do(undo) a "null move": It flips
    /// the side to move without executing any move on the board.
    internal void do_null_move(StateInfo newSt)
    {
        Debug.Assert(checkers() == 0);
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

        sideToMove = Color.opposite(sideToMove);

        Debug.Assert(pos_is_ok());
    }

    internal void undo_null_move()
    {
        Debug.Assert(checkers() == 0);

        st = st.previous;
        sideToMove = Color.opposite(sideToMove);
    }

    // min_attacker() is a helper function used by see() to locate the least
    // valuable attacker for the side to move, remove the attacker we just found
    // from the bitboards and scan for new X-ray attacks behind it.
    private PieceTypeT min_attacker(
        PieceTypeT Pt,
        BitboardT[] bb,
        SquareT to,
        BitboardT stmAttackers,
        ref BitboardT occupied,
        ref BitboardT attackers)
    {
        if (Pt == PieceType.KING)
        {
            return PieceType.KING;
        }
        var b = stmAttackers & bb[Pt];
        if (b == 0)
        {
            return min_attacker(Pt + 1, bb, to, stmAttackers, ref occupied, ref attackers);
        }

        occupied ^= b & ~(b - Bitboard.Create(1));

        if (Pt == PieceType.PAWN || Pt == PieceType.BISHOP || Pt == PieceType.QUEEN)
        {
            attackers |= Utils.attacks_bb_PtSBb(PieceType.BISHOP, to, occupied) & (bb[PieceType.BISHOP] | bb[PieceType.QUEEN]);
        }

        if (Pt == PieceType.ROOK || Pt == PieceType.QUEEN)
        {
            attackers |= Utils.attacks_bb_PtSBb(PieceType.ROOK, to, occupied) & (bb[PieceType.ROOK] | bb[PieceType.QUEEN]);
        }

        attackers &= occupied; // After X-ray that may add already processed pieces
        return Pt;
    }

    /// Position::key_after() computes the new hash key after the given move. Needed
    /// for speculative prefetch. It doesn't recognize special moves like castling,
    /// en-passant and promotions.
    private ulong key_after(MoveT m)
    {
        var us = sideToMove;
        var from = Move.from_sq(m);
        var to = Move.to_sq(m);
        var pt = (int)Piece.type_of(piece_on(from));
        var captured = (int)Piece.type_of(piece_on(to));
        var k = st.key ^ Zobrist.side;

        if (captured!=0)
        {
            k ^= Zobrist.psq[Color.opposite(us), captured, to];
        }

        return k ^ Zobrist.psq[us, pt, to] ^ Zobrist.psq[us, pt, @from];
    }

    /// Position::see() is a static exchange evaluator: It tries to estimate the
    /// material gain or loss resulting from a move.
    internal ValueT see_sign(MoveT m)
    {
        Debug.Assert(Move.is_ok(m));

        // Early return if SEE cannot be negative because captured piece value
        // is not less then capturing one. Note that king moves always return
        // here because king midgame value is set to 0.
        if (Value.PieceValue[(int) Phase.MG][moved_piece(m)]
            <= Value.PieceValue[(int) Phase.MG][piece_on(Move.to_sq(m))])
        {
            return Value.VALUE_KNOWN_WIN;
        }

        return see(m);
    }

    internal ValueT see(MoveT m)
    {
        var swapList = new ValueT[32];
        var slIndex = 1;

        Debug.Assert(Move.is_ok(m));

        var @from = Move.from_sq(m);
        var to = Move.to_sq(m);
        swapList[0] = Value.PieceValue[(int) Phase.MG][piece_on(to)];
        var stm = Piece.color_of(piece_on(@from));
        var occupied = Bitboard.XorWithSquare(pieces(), from);

        // Castling moves are implemented as king capturing the rook so cannot
        // be handled correctly. Simply return VALUE_ZERO that is always correct
        // unless in the rare case the rook ends up under attack.
        if (Move.type_of(m) == MoveType.CASTLING)
        {
            return Value.VALUE_ZERO;
        }

        if (Move.type_of(m) == MoveType.ENPASSANT)
        {
            occupied = Bitboard.XorWithSquare(occupied, to - Square.pawn_push(stm)); // Remove the captured pawn
            swapList[0] = Value.PieceValue[(int) Phase.MG][PieceType.PAWN];
        }

        // Find all attackers to the destination square, with the moving piece
        // removed, but possibly an X-ray attacker added behind it.
        var attackers = attackers_to(to, occupied) & occupied;

        // If the opponent has no attackers we are finished
        stm = Color.opposite(stm);
        var stmAttackers = attackers & pieces_Ct(stm);
        if (stmAttackers == 0)
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
            captured = min_attacker(PieceType.PAWN, byTypeBB, to, stmAttackers, ref occupied, ref attackers);
            stm = Color.opposite(stm);
            stmAttackers = attackers & pieces_Ct(stm);
            ++slIndex;
        } while (stmAttackers != 0 && (captured != PieceType.KING || DecreaseValue(ref slIndex)));
            // Stop before a king capture

        // Having built the swap list, we negamax through it to find the best
        // achievable score from the point of view of the side to move.
        while (--slIndex != 0)
        {
            swapList[slIndex - 1] = Value.Create(Math.Min(-swapList[slIndex], swapList[slIndex - 1]));
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
        if (st.rule50 > 99 && (checkers() == 0 || new MoveList(GenType.LEGAL, this).size() > 0))
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

                var epSquare = ep_square();
                if (epSquare != Square.SQ_NONE && Rank.relative_rank_CtSt(sideToMove, epSquare) != Rank.RANK_6)
                {
                    return false;
                }
            }

            if (step == (int) CheckStep.King)
            {
                if (board.Count(piece => piece == Piece.W_KING) != 1
                    || board.Count(piece => piece == Piece.B_KING) != 1
                    || (attackers_to(square(PieceType.KING, Color.opposite(sideToMove))) & pieces_Ct(sideToMove)) != 0)
                {
                    return false;
                }
            }

            if (step == (int) CheckStep.Bitboards)
            {
                if ((pieces_Ct(Color.WHITE) & pieces_Ct(Color.BLACK)) != 0
                    || (pieces_Ct(Color.WHITE) | pieces_Ct(Color.BLACK)) != pieces())
                {
                    return false;
                }

                foreach (var p1 in PieceType.AllPieceTypes)
                {
                    foreach (var p2 in PieceType.AllPieceTypes)
                    {
                        if (p1 != p2 && ((pieces_Pt(p1) & pieces_Pt(p2)) != 0))
                        {
                            return false;
                        }
                    }
                }
            }

            if (step == (int) CheckStep.Lists)
            {
                foreach (var c in Color.AllColors)
                {
                    foreach (var pt in PieceType.AllPieceTypes)
                    {
                        if (pieceCount[c, pt] != Bitcount.popcount_Full(pieces_CtPt(c, pt)))
                        {
                            return false;
                        }

                        for (var i = 0; i < pieceCount[c, pt]; ++i)
                        {
                            if (board[pieceList[c, pt, i]] != Piece.make_piece(c, pt)
                                || index[pieceList[c, pt, i]] != i)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            if (step == (int) CheckStep.Castling)
            {
                foreach (var c in Color.AllColors)
                {
                    for (var s = CastlingSide.KING_SIDE; s <= CastlingSide.QUEEN_SIDE; s++)
                    {
                        var castlingSideColor = Color.CalculateCastlingRight(c, s);
                        if (!can_castle(castlingSideColor))
                        {
                            continue;
                        }

                        if (piece_on(castlingRookSquare[(int) (castlingSideColor)]) != Piece.make_piece(c, PieceType.ROOK)
                            || castlingRightsMask[castlingRookSquare[(int) (castlingSideColor)]] != (int) (castlingSideColor)
                            || (castlingRightsMask[square(PieceType.KING, c)] & (int) (castlingSideColor)) != (int) (castlingSideColor))
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

        for (var r = (int)Rank.RANK_8; r >= Rank.RANK_1; --r)
        {
            for (var f = File.FILE_A; f <= File.FILE_H; ++f)
            {
                int emptyCnt;
                for (emptyCnt = 0; f <= File.FILE_H && empty(Square.make_square(File.Create(f), Rank.Create(r))); ++f)
                {
                    ++emptyCnt;
                }

                if (emptyCnt != 0)
                {
                    ss.Append(emptyCnt);
                }

                if (f <= File.FILE_H)
                {
                    ss.Append(PieceToChar[piece_on(Square.make_square(File.Create(f), Rank.Create(r)))]);
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
                    ? (char) ('A' + Square.file_of(castling_rook_square(Color.CalculateCastlingRight(Color.WHITE, CastlingSide.KING_SIDE))))
                    : 'K'));
        }

        if (can_castle(CastlingRight.WHITE_OOO))
        {
            ss.Append(
                (chess960
                    ? (char) ('A' + Square.file_of(castling_rook_square(Color.CalculateCastlingRight(Color.WHITE, CastlingSide.QUEEN_SIDE))))
                    : 'Q'));
        }

        if (can_castle(CastlingRight.BLACK_OO))
        {
            ss.Append(
                (chess960
                    ? (char) ('a' + Square.file_of(castling_rook_square(Color.CalculateCastlingRight(Color.BLACK, CastlingSide.KING_SIDE))))
                    : 'k'));
        }

        if (can_castle(CastlingRight.BLACK_OOO))
        {
            ss.Append(
                (chess960
                    ? (char) ('a' + Square.file_of(castling_rook_square(Color.CalculateCastlingRight(Color.BLACK, CastlingSide.QUEEN_SIDE))))
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
            SquareT rsq;
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
                rsq = Square.Create((token - 'A') | Rank.relative_rank_CtRt(c, Rank.RANK_1));
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
                    st.epSquare = Square.make_square(File.Create(col - 'a'), Rank.Create(row - '1'));

                    if ((attackers_to(st.epSquare) & pieces_CtPt(sideToMove, PieceType.PAWN)) == 0)
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

        byColorBB = new BitboardT[Color.COLOR_NB];

        byTypeBB = new BitboardT[PieceType.PIECE_TYPE_NB];

        castlingPath = new BitboardT[(int) CastlingRight.CASTLING_RIGHT_NB];

        castlingRightsMask = new int[Square.SQUARE_NB];

        castlingRookSquare = new SquareT[(int) CastlingRight.CASTLING_RIGHT_NB];

        index = new int[Square.SQUARE_NB];

        pieceCount = new int[Color.COLOR_NB, PieceType.PIECE_TYPE_NB];

        pieceList = new SquareT[Color.COLOR_NB, PieceType.PIECE_TYPE_NB, 16];
        for (var i = 0; i < PieceType.PIECE_TYPE_NB; ++i)
        {
            for (var j = 0; j < 16; ++j)
            {
                pieceList[Color.WHITE, i, j] = pieceList[Color.BLACK, i, j] = Square.SQ_NONE;
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
        for (var r = (int)Rank.RANK_8; r >= Rank.RANK_1; --r)
        {
            foreach (var f in File.AllFiles)
            {
                sb.Append(" | ");
                sb.Append(PieceToChar[piece_on(Square.make_square(f, Rank.Create(r)))]);
            }

            sb.Append(" |\n +---+---+---+---+---+---+---+---+\n");
        }

        sb.Append($"\nFen: {fen()}\nKey: {st.key:X}\nCheckers: ");

        for (var b = checkers(); b != 0;)
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