using System.Diagnostics;
using System.Linq;

internal static class Movegen
{
    internal static ExtMoveArrayWrapper generate_castling(
        CastlingRight Cr,
        bool Checks,
        bool Chess960,
        Position pos,
        ExtMoveArrayWrapper moveList,
        Color us,
        CheckInfo ci)
    {
        var KingSide = (Cr == CastlingRight.WHITE_OO || Cr == CastlingRight.BLACK_OO);

        if (pos.castling_impeded(Cr) || !pos.can_castle(Cr))
        {
            return moveList;
        }

        // After castling, the rook and king final positions are the same in Chess960
        // as they would be in standard chess.
        var kfrom = pos.square(PieceType.KING, us);
        var rfrom = pos.castling_rook_square(Cr);
        var kto = Square.relative_square(us, KingSide ? Square.SQ_G1 : Square.SQ_C1);
        var enemies = pos.pieces(~us);

        Debug.Assert(!pos.checkers());

        var K = Chess960 ? kto > kfrom ? Square.DELTA_W : Square.DELTA_E : KingSide ? Square.DELTA_W : Square.DELTA_E;

        for (var s = kto; s != kfrom; s += K)
        {
            if (pos.attackers_to(s) & enemies)
            {
                return moveList;
            }
        }

        // Because we generate only legal castling moves we need to verify that
        // when moving the castling rook we do not discover some hidden checker.
        // For instance an enemy queen in SQ_A1 when castling rook is in SQ_B1.
        if (Chess960
            && (Utils.attacks_bb(PieceType.ROOK, kto, pos.pieces() ^ rfrom)
                & pos.pieces(~us, PieceType.ROOK, PieceType.QUEEN)))
        {
            return moveList;
        }

        var m = Move.make(MoveType.CASTLING, kfrom, rfrom);

        if (Checks && !pos.gives_check(m, ci))
        {
            return moveList;
        }

        moveList.Add(m);
        return moveList;
    }

    internal static ExtMoveArrayWrapper make_promotions(
        GenType Type,
        Square Delta,
        ExtMoveArrayWrapper moveList,
        Square to,
        CheckInfo ci)
    {
        if (Type == GenType.CAPTURES || Type == GenType.EVASIONS || Type == GenType.NON_EVASIONS)
        {
            (moveList).Add(Move.make(MoveType.PROMOTION, to - Delta, to, PieceType.QUEEN));
        }

        if (Type == GenType.QUIETS || Type == GenType.EVASIONS || Type == GenType.NON_EVASIONS)
        {
            (moveList).Add(Move.make(MoveType.PROMOTION, to - Delta, to, PieceType.ROOK));

            (moveList).Add(Move.make(MoveType.PROMOTION, to - Delta, to, PieceType.BISHOP));

            (moveList).Add(Move.make(MoveType.PROMOTION, to - Delta, to, PieceType.KNIGHT));
        }

        // Knight promotion is the only promotion that can give a direct check
        // that's not already included in the queen promotion.
        if (Type == GenType.QUIET_CHECKS && (Utils.StepAttacksBB[Piece.W_KNIGHT, to] & ci.ksq))
        {
            (moveList).Add(Move.make(MoveType.PROMOTION, to - Delta, to, PieceType.KNIGHT));
        }

        return moveList;
    }

    internal static ExtMoveArrayWrapper generate_pawn_moves(
        Color Us,
        GenType Type,
        Position pos,
        ExtMoveArrayWrapper moveList,
        Bitboard target,
        CheckInfo ci)
    {
        // Compute our parametrized parameters at compile time, named according to
        // the point of view of white side.
        var Them = (Us == Color.WHITE ? Color.BLACK : Color.WHITE);
        var TRank8BB = (Us == Color.WHITE ? Bitboard.Rank8BB : Bitboard.Rank1BB);
        var TRank7BB = (Us == Color.WHITE ? Bitboard.Rank7BB : Bitboard.Rank2BB);
        var TRank3BB = (Us == Color.WHITE ? Bitboard.Rank3BB : Bitboard.Rank6BB);
        var Up = (Us == Color.WHITE ? Square.DELTA_N : Square.DELTA_S);
        var Right = (Us == Color.WHITE ? Square.DELTA_NE : Square.DELTA_SW);
        var Left = (Us == Color.WHITE ? Square.DELTA_NW : Square.DELTA_SE);

        var emptySquares = new Bitboard(0);

        var pawnsOn7 = pos.pieces(Us, PieceType.PAWN) & TRank7BB;
        var pawnsNotOn7 = pos.pieces(Us, PieceType.PAWN) & ~TRank7BB;

        var enemies = (Type == GenType.EVASIONS
            ? pos.pieces(Them) & target
            : Type == GenType.CAPTURES ? target : pos.pieces(Them));

        // Single and double pawn pushes, no promotions
        if (Type != GenType.CAPTURES)
        {
            emptySquares = (Type == GenType.QUIETS || Type == GenType.QUIET_CHECKS ? target : ~pos.pieces());

            var b1 = Bitboard.shift_bb(Up, pawnsNotOn7) & emptySquares;
            var b2 = Bitboard.shift_bb(Up, b1 & TRank3BB) & emptySquares;

            if (Type == GenType.EVASIONS) // Consider only blocking squares
            {
                b1 &= target;
                b2 &= target;
            }

            if (Type == GenType.QUIET_CHECKS)
            {
                b1 &= pos.attacks_from(PieceType.PAWN, ci.ksq, Them);
                b2 &= pos.attacks_from(PieceType.PAWN, ci.ksq, Them);

                // Add pawn pushes which give discovered check. This is possible only
                // if the pawn is not on the same file as the enemy king, because we
                // don't generate captures. Note that a possible discovery check
                // promotion has been already generated amongst the captures.
                if (pawnsNotOn7 & ci.dcCandidates)
                {
                    var dc1 = Bitboard.shift_bb(Up, pawnsNotOn7 & ci.dcCandidates) & emptySquares
                              & ~Utils.file_bb(ci.ksq);
                    var dc2 = Bitboard.shift_bb(Up, dc1 & TRank3BB) & emptySquares;

                    b1 |= dc1;
                    b2 |= dc2;
                }
            }

            while (b1)
            {
                var to = Utils.pop_lsb(ref b1);
                (moveList).Add(Move.make_move(to - Up, to));
            }

            while (b2)
            {
                var to = Utils.pop_lsb(ref b2);
                (moveList).Add(Move.make_move(to - Up - Up, to));
            }
        }

        // Promotions and underpromotions
        if (pawnsOn7 && (Type != GenType.EVASIONS || (target & TRank8BB)))
        {
            if (Type == GenType.CAPTURES)
            {
                emptySquares = ~pos.pieces();
            }

            if (Type == GenType.EVASIONS)
            {
                emptySquares &= target;
            }

            var b1 = Bitboard.shift_bb(Right, pawnsOn7) & enemies;
            var b2 = Bitboard.shift_bb(Left, pawnsOn7) & enemies;
            var b3 = Bitboard.shift_bb(Up, pawnsOn7) & emptySquares;

            while (b1)
            {
                moveList = make_promotions(Type, Right, moveList, Utils.pop_lsb(ref b1), ci);
            }

            while (b2)
            {
                moveList = make_promotions(Type, Left, moveList, Utils.pop_lsb(ref b2), ci);
            }

            while (b3)
            {
                moveList = make_promotions(Type, Up, moveList, Utils.pop_lsb(ref b3), ci);
            }
        }

        // Standard and en-passant captures
        if (Type == GenType.CAPTURES || Type == GenType.EVASIONS || Type == GenType.NON_EVASIONS)
        {
            var b1 = Bitboard.shift_bb(Right, pawnsNotOn7) & enemies;
            var b2 = Bitboard.shift_bb(Left, pawnsNotOn7) & enemies;

            while (b1)
            {
                var to = Utils.pop_lsb(ref b1);
                (moveList).Add(Move.make_move(to - Right, to));
            }

            while (b2)
            {
                var to = Utils.pop_lsb(ref b2);
                (moveList).Add(Move.make_move(to - Left, to));
            }

            if (pos.ep_square() != Square.SQ_NONE)
            {
                Debug.Assert(Square.rank_of(pos.ep_square()) == Rank.relative_rank(Us, Rank.RANK_6));

                // An en passant capture can be an evasion only if the checking piece
                // is the double pushed pawn and so is in the target. Otherwise this
                // is a discovery check and we are forced to do otherwise.
                if (Type == GenType.EVASIONS && !(target & (pos.ep_square() - Up)))
                {
                    return moveList;
                }

                b1 = pawnsNotOn7 & pos.attacks_from(PieceType.PAWN, pos.ep_square(), Them);

                Debug.Assert(b1);

                while (b1)
                {
                    (moveList).Add(Move.make(MoveType.ENPASSANT, Utils.pop_lsb(ref b1), pos.ep_square()));
                }
            }
        }

        return moveList;
    }

    internal static ExtMoveArrayWrapper generate_moves(
        PieceType pieceType,
        bool Checks,
        Position pos,
        ExtMoveArrayWrapper moveList,
        Color us,
        Bitboard target,
        CheckInfo ci)
    {
        var Pt = (int) pieceType;
        Debug.Assert(Pt != PieceType.KING_C && Pt != PieceType.PAWN_C);

        for(var idx=0; idx<16;idx++)
        {
            var square = pos.square(pieceType, us, idx);
            if (square == Square.SQ_NONE)
            {
                break;
            }

            if (Checks)
            {
                if ((Pt == PieceType.BISHOP_C || Pt == PieceType.ROOK_C || Pt == PieceType.QUEEN_C)
                    && !(Utils.PseudoAttacks[Pt, square] & target & ci.checkSquares[Pt]))
                {
                    continue;
                }

                if ((bool) ci.dcCandidates && (ci.dcCandidates & square))
                {
                    continue;
                }
            }

            var b = pos.attacks_from(pieceType, square) & target;

            if (Checks)
            {
                b &= ci.checkSquares[Pt];
            }

            while (b)
            {
                (moveList).Add(Move.make_move(square, Utils.pop_lsb(ref b)));
            }
        }

        return moveList;
    }

    internal static ExtMoveArrayWrapper generate_all(
        Color Us,
        GenType Type,
        Position pos,
        ExtMoveArrayWrapper moveList,
        Bitboard target,
        CheckInfo ci = null)
    {
        var Checks = Type == GenType.QUIET_CHECKS;

        moveList = generate_pawn_moves(Us, Type, pos, moveList, target, ci);
        moveList = generate_moves(PieceType.KNIGHT, Checks, pos, moveList, Us, target, ci);
        moveList = generate_moves(PieceType.BISHOP, Checks, pos, moveList, Us, target, ci);
        moveList = generate_moves(PieceType.ROOK, Checks, pos, moveList, Us, target, ci);
        moveList = generate_moves(PieceType.QUEEN, Checks, pos, moveList, Us, target, ci);

        if (Type != GenType.QUIET_CHECKS && Type != GenType.EVASIONS)
        {
            var ksq = pos.square(PieceType.KING, Us);
            var b = pos.attacks_from(PieceType.KING, ksq) & target;
            while (b)
            {
                (moveList).Add(Move.make_move(ksq, Utils.pop_lsb(ref b)));
            }
        }

        if (Type != GenType.CAPTURES && Type != GenType.EVASIONS && pos.can_castle(Us) != 0)
        {
            if (pos.is_chess960())
            {
                moveList = generate_castling(
                    MakeCastling(Us, CastlingSide.KING_SIDE),
                    Checks,
                    true,
                    pos,
                    moveList,
                    Us,
                    ci);
                moveList = generate_castling(
                    MakeCastling(Us, CastlingSide.QUEEN_SIDE),
                    Checks,
                    true,
                    pos,
                    moveList,
                    Us,
                    ci);
            }
            else
            {
                moveList = generate_castling(
                    MakeCastling(Us, CastlingSide.KING_SIDE),
                    Checks,
                    false,
                    pos,
                    moveList,
                    Us,
                    ci);
                moveList = generate_castling(
                    MakeCastling(Us, CastlingSide.QUEEN_SIDE),
                    Checks,
                    false,
                    pos,
                    moveList,
                    Us,
                    ci);
            }
        }

        return moveList;
    }

    internal static CastlingRight MakeCastling(Color C, CastlingSide S)
    {
        return C == Color.WHITE
            ? S == CastlingSide.QUEEN_SIDE ? CastlingRight.WHITE_OOO : CastlingRight.WHITE_OO
            : S == CastlingSide.QUEEN_SIDE ? CastlingRight.BLACK_OOO : CastlingRight.BLACK_OO;
    }

    internal static ExtMoveArrayWrapper generate(GenType Type, Position pos, ExtMoveArrayWrapper moveList)
    {
        switch (Type)
        {
            case GenType.EVASIONS:
                return generate_EVASIONS(pos, moveList);
            case GenType.LEGAL:
                return generate_LEGAL(pos, moveList);
            case GenType.QUIET_CHECKS:
                return generate_QUIET_CHECKS(pos, moveList);
        }

        Debug.Assert(Type == GenType.CAPTURES || Type == GenType.QUIETS || Type == GenType.NON_EVASIONS);
        Debug.Assert(!pos.checkers());

        var us = pos.side_to_move();

        var target = Type == GenType.CAPTURES
            ? pos.pieces(~us)
            : Type == GenType.QUIETS
                ? ~pos.pieces()
                : Type == GenType.NON_EVASIONS ? ~pos.pieces(us) : new Bitboard(0);

        return us == Color.WHITE
            ? generate_all(Color.WHITE, Type, pos, moveList, target)
            : generate_all(Color.BLACK, Type, pos, moveList, target);
    }

    /// generate
    /// QUIET_CHECKS
    ///     generates all pseudo-legal non-captures and knight
    ///     underpromotions that give check. Returns a pointer to the end of the move list.
    private static ExtMoveArrayWrapper generate_QUIET_CHECKS(Position pos, ExtMoveArrayWrapper moveList)
    {
        Debug.Assert(!pos.checkers());

        var us = pos.side_to_move();
        var ci = new CheckInfo(pos);
        var dc = ci.dcCandidates;

        while (dc)
        {
            var from = Utils.pop_lsb(ref dc);
            var pt = Piece.type_of(pos.piece_on(from));

            if (pt == PieceType.PAWN)
            {
                continue; // Will be generated together with direct checks
            }

            var b = pos.attacks_from(pt, from) & ~pos.pieces();

            if (pt == PieceType.KING)
            {
                b &= ~Utils.PseudoAttacks[PieceType.QUEEN_C, ci.ksq];
            }

            while (b)
            {
                (moveList).Add(Move.make_move(from, Utils.pop_lsb(ref b)));
            }
        }

        return us == Color.WHITE
            ? generate_all(Color.WHITE, GenType.QUIET_CHECKS, pos, moveList, ~pos.pieces(), ci)
            : generate_all(Color.BLACK, GenType.QUIET_CHECKS, pos, moveList, ~pos.pieces(), ci);
    }

    /// generate
    /// EVASIONS
    ///     generates all pseudo-legal check evasions when the side
    ///     to move is in check. Returns a pointer to the end of the move list.
    private static ExtMoveArrayWrapper generate_EVASIONS(Position pos, ExtMoveArrayWrapper moveList)
    {
        Debug.Assert(pos.checkers());

        var us = pos.side_to_move();
        var ksq = pos.square(PieceType.KING, us);
        var sliderAttacks = new Bitboard(0);
        var sliders = pos.checkers() & ~pos.pieces(PieceType.KNIGHT, PieceType.PAWN);

        // Find all the squares attacked by slider checkers. We will remove them from
        // the king evasions in order to skip known illegal moves, which avoids any
        // useless legality checks later on.
        while (sliders)
        {
            var checksq1 = Utils.pop_lsb(ref sliders);
            sliderAttacks |= Utils.LineBB[checksq1, ksq] ^ checksq1;
        }

        // Generate evasions for king, capture and non capture moves
        var b = pos.attacks_from(PieceType.KING, ksq) & ~pos.pieces(us) & ~sliderAttacks;
        while (b)
        {
            (moveList).Add(Move.make_move(ksq, Utils.pop_lsb(ref b)));
        }

        if (Bitboard.more_than_one(pos.checkers()))
        {
            return moveList; // Double check, only a king move can save the day
        }

        // Generate blocking evasions or captures of the checking piece
        var checksq = Utils.lsb(pos.checkers());
        var target = Utils.between_bb(checksq, ksq) | checksq;

        return us == Color.WHITE
            ? generate_all(Color.WHITE, GenType.EVASIONS, pos, moveList, target)
            : generate_all(Color.BLACK, GenType.EVASIONS, pos, moveList, target);
    }

    /// generate
    /// LEGAL generates all the legal moves in the given position
    private static ExtMoveArrayWrapper generate_LEGAL(Position pos, ExtMoveArrayWrapper moveList)
    {
        var pinned = pos.pinned_pieces(pos.side_to_move());
        var ksq = pos.square(PieceType.KING, pos.side_to_move());
        var cur = moveList.current;

        moveList = pos.checkers()
            ? generate(GenType.EVASIONS, pos, moveList)
            : generate(GenType.NON_EVASIONS, pos, moveList);

        while (cur != moveList.current)
        {
            if ((pinned || Move.from_sq(moveList[cur]) == ksq || Move.type_of(moveList[cur]) == MoveType.ENPASSANT)
                && !pos.legal(moveList[cur], pinned))
            {
                for (var idx = cur; idx < moveList.current; idx++)
                {
                    moveList.table[idx] = moveList.table[idx + 1];
                }
                --moveList;
            }
            else
            {
                ++cur;
            }
        }

        return moveList;
    }
}