using System;
using System.Diagnostics;

#if PRIMITIVE
using ColorT = System.Int32;
using PieceTypeT = System.Int32;
#endif

internal abstract class Endgame
{
    internal delegate int EndgameEvaluator(int c, Position pos);

    // Table used to drive the king towards the edge of the board
    // in KX vs K and KQ vs KR endgames.
    internal static int[] PushToEdges =
    {
        100, 90, 80, 70, 70, 80, 90, 100, 90, 70, 60, 50, 50, 60, 70, 90, 80, 60, 40, 30,
        30, 40, 60, 80, 70, 50, 30, 20, 20, 30, 50, 70, 70, 50, 30, 20, 20, 30, 50, 70,
        80, 60, 40, 30, 30, 40, 60, 80, 90, 70, 60, 50, 50, 60, 70, 90, 100, 90, 80, 70,
        70, 80, 90, 100
    };

    // Table used to drive the king towards a corner square of the
    // right color in KBN vs K endgames.
    internal static int[] PushToCorners =
    {
        200, 190, 180, 170, 160, 150, 140, 130, 190, 180, 170, 160, 150, 140, 130, 140,
        180, 170, 155, 140, 140, 125, 140, 150, 170, 160, 140, 120, 110, 140, 150, 160,
        160, 150, 140, 110, 120, 140, 160, 170, 150, 140, 125, 140, 140, 155, 170, 180,
        140, 130, 140, 150, 160, 170, 180, 190, 130, 140, 150, 160, 170, 180, 190, 200
    };

    // Tables used to drive a piece towards or away from another piece
    internal static int[] PushClose = {0, 0, 100, 80, 60, 40, 20, 10};

    internal static int[] PushAway = {0, 5, 20, 40, 60, 80, 90, 100};

    protected readonly ColorT strongSide;

    protected ColorT weakSide;

    protected Endgame(ColorT c)
    {
        strongSide = c;
        weakSide = Color.opposite(c);
    }

    internal ColorT strong_side()
    {
        return strongSide;
    }

    protected static bool verify_material(Position pos, ColorT c, Value npm, int pawnsCnt)
    {
        return pos.non_pawn_material(c) == npm && pos.count(PieceType.PAWN, c) == pawnsCnt;
    }

    // Map the square as if strongSide is white and strongSide's only pawn
    // is on the left half of the board.
    protected static Square normalize(Position pos, ColorT strongSide, Square sq)
    {
        Debug.Assert(pos.count(PieceType.PAWN, strongSide) == 1);

        if (Square.file_of(pos.square(PieceType.PAWN, strongSide)) >= File.FILE_E)
        {
            sq = new Square(sq ^ 7); // Mirror SQ_H1 -> SQ_A1
        }

        if (strongSide == Color.BLACK)
        {
            sq = ~sq;
        }

        return sq;
    }

    // Get the material key of Position out of the given endgame key code
    // like "KBPKN". The trick here is to first forge an ad-hoc FEN string
    // and then let a Position object do the work for us.
    internal static ulong key(string code, ColorT c)
    {
        Debug.Assert(code.Length > 0 && code.Length < 8);
        Debug.Assert(code[0] == 'K');

        string[] sides =
        {
            code.Substring(code.IndexOf('K', 1)), // Weak
            code.Substring(0, code.IndexOf('K', 1))
        }; // Strong
        sides[c] = sides[c].ToLower();

        var fen = sides[0] + (char) (8 - sides[0].Length + '0') + "/8/8/8/8/8/8/" + sides[1]
                  + (char) (8 - sides[1].Length + '0') + " w - - 0 10";

        return new Position(fen, false, null).material_key();
    }
}

internal abstract class EndgameValue : Endgame
{
    protected EndgameValue(ColorT c)
        : base(c)
    {
    }

    internal abstract Value GetValue(Position pos);
}

internal abstract class EndgameScaleFactor : Endgame
{
    protected EndgameScaleFactor(ColorT c)
        : base(c)
    {
    }

    internal abstract ScaleFactor GetScaleFactor(Position pos);
}

/// Mate with KX vs K. This function is used to evaluate positions with
/// king and plenty of material vs a lone king. It simply gives the
/// attacking side a bonus for driving the defending king towards the edge
/// of the board, and for keeping the distance between the two kings small.
internal class EndgameKXK : EndgameValue
{
    internal EndgameKXK(ColorT c)
        : base(c)
    {
    }

    internal override Value GetValue(Position pos)
    {
        Debug.Assert(verify_material(pos, weakSide, Value.VALUE_ZERO, 0));
        Debug.Assert(!pos.checkers()); // Eval is never called when in check

        // Stalemate detection with lone king
        if (pos.side_to_move() == weakSide && new MoveList(GenType.LEGAL, pos).size() == 0)
        {
            return Value.VALUE_DRAW;
        }

        var winnerKSq = pos.square(PieceType.KING, strongSide);
        var loserKSq = pos.square(PieceType.KING, weakSide);

        var result = pos.non_pawn_material(strongSide)
                     + pos.count(PieceType.PAWN, strongSide)*Value.PawnValueEg + PushToEdges[loserKSq]
                     + PushClose[Utils.distance_Square(winnerKSq, loserKSq)];

        if (pos.count(PieceType.QUEEN, strongSide) > 0 || pos.count(PieceType.ROOK, strongSide) > 0
            || (pos.count(PieceType.BISHOP, strongSide) > 0 && pos.count(PieceType.KNIGHT, strongSide) > 0)
            || (pos.count(PieceType.BISHOP, strongSide) > 1
                && Square.opposite_colors(
                    pos.square(PieceType.BISHOP, strongSide, 0),
                    pos.square(PieceType.BISHOP, strongSide, 1))))
        {
            result += Value.VALUE_KNOWN_WIN;
        }

        return strongSide == pos.side_to_move() ? result : -result;
    }
}

/// Mate with KBN vs K. This is similar to KX vs K, but we have to drive the
/// defending king towards a corner square of the right color.
internal class EndgameKBNK : EndgameValue
{
    internal EndgameKBNK(ColorT c)
        : base(c)
    {
    }

    internal override Value GetValue(Position pos)
    {
        Debug.Assert(verify_material(pos, strongSide, Value.KnightValueMg + Value.BishopValueMg, 0));
        Debug.Assert(verify_material(pos, weakSide, Value.VALUE_ZERO, 0));

        var winnerKSq = pos.square(PieceType.KING, strongSide);
        var loserKSq = pos.square(PieceType.KING, weakSide);
        var bishopSq = pos.square(PieceType.BISHOP, strongSide);

        // kbnk_mate_table() tries to drive toward corners A1 or H8. If we have a
        // bishop that cannot reach the above squares, we flip the kings in order
        // to drive the enemy toward corners A8 or H1.
        if (Square.opposite_colors(bishopSq, Square.SQ_A1))
        {
            winnerKSq = ~winnerKSq;
            loserKSq = ~loserKSq;
        }

        var result = Value.VALUE_KNOWN_WIN + PushClose[Utils.distance_Square(winnerKSq, loserKSq)]
                     + PushToCorners[loserKSq];

        return strongSide == pos.side_to_move() ? result : -result;
    }
}

/// KP vs K. This endgame is evaluated with the help of a bitbase.
internal class EndgameKPK : EndgameValue
{
    internal EndgameKPK(ColorT c)
        : base(c)
    {
    }

    internal override Value GetValue(Position pos)
    {
        Debug.Assert(verify_material(pos, strongSide, Value.VALUE_ZERO, 1));
        Debug.Assert(verify_material(pos, weakSide, Value.VALUE_ZERO, 0));

        // Assume strongSide is white and the pawn is on files A-D
        var wksq = normalize(pos, strongSide, pos.square(PieceType.KING, strongSide));
        var bksq = normalize(pos, strongSide, pos.square(PieceType.KING, weakSide));
        var psq = normalize(pos, strongSide, pos.square(PieceType.PAWN, strongSide));

        var us = strongSide == pos.side_to_move() ? Color.WHITE : Color.BLACK;

        if (!Bitbases.probe(wksq, psq, bksq, us))
        {
            return Value.VALUE_DRAW;
        }

        var result = Value.VALUE_KNOWN_WIN + Value.PawnValueEg + new Value(Square.rank_of(psq));

        return strongSide == pos.side_to_move() ? result : -result;
    }
}

/// KR vs KP. This is a somewhat tricky endgame to evaluate precisely without
/// a bitbase. The function below returns drawish scores when the pawn is
/// far advanced with support of the king, while the attacking king is far
/// away.
internal class EndgameKRKP : EndgameValue
{
    internal EndgameKRKP(ColorT c)
        : base(c)
    {
    }

    internal override Value GetValue(Position pos)
    {
        Debug.Assert(verify_material(pos, strongSide, Value.RookValueMg, 0));
        Debug.Assert(verify_material(pos, weakSide, Value.VALUE_ZERO, 1));

        var wksq = Square.relative_square(strongSide, pos.square(PieceType.KING, strongSide));
        var bksq = Square.relative_square(strongSide, pos.square(PieceType.KING, weakSide));
        var rsq = Square.relative_square(strongSide, pos.square(PieceType.ROOK, strongSide));
        var psq = Square.relative_square(strongSide, pos.square(PieceType.PAWN, weakSide));

        var queeningSq = Square.make_square(Square.file_of(psq), Rank.RANK_1);
        Value result;

        // If the stronger side's king is in front of the pawn, it's a win
        if (wksq < psq && Square.file_of(wksq) == Square.file_of(psq))
        {
            result = Value.RookValueEg - Utils.distance_Square(wksq, psq);
        }

        // If the weaker side's king is too far from the pawn and the rook,
        // it's a win.
        else if (Utils.distance_Square(bksq, psq) >= 3 + (pos.side_to_move() == weakSide ? 1 : 0)
                 && Utils.distance_Square(bksq, rsq) >= 3)
        {
            result = Value.RookValueEg - Utils.distance_Square(wksq, psq);
        }

        // If the pawn is far advanced and supported by the defending king,
        // the position is drawish
        else if (Square.rank_of(bksq) <= Rank.RANK_3 && Utils.distance_Square(bksq, psq) == 1
                 && Square.rank_of(wksq) >= Rank.RANK_4
                 && Utils.distance_Square(wksq, psq) > 2 + (pos.side_to_move() == strongSide ? 1 : 0))
        {
            result = new Value(80) - 8*Utils.distance_Square(wksq, psq);
        }

        else
        {
            result = new Value(200)
                     - 8
                     *(Utils.distance_Square(wksq, psq + Square.DELTA_S)
                       - Utils.distance_Square(bksq, psq + Square.DELTA_S)
                       - Utils.distance_Square(psq, queeningSq));
        }

        return strongSide == pos.side_to_move() ? result : -result;
    }
}

/// KR vs KB. This is very simple, and always returns drawish scores.  The
/// score is slightly bigger when the defending king is close to the edge.
internal class EndgameKRKB : EndgameValue
{
    internal EndgameKRKB(ColorT c)
        : base(c)
    {
    }

    internal override Value GetValue(Position pos)
    {
        Debug.Assert(verify_material(pos, strongSide, Value.RookValueMg, 0));
        Debug.Assert(verify_material(pos, weakSide, Value.BishopValueMg, 0));

        var result = new Value(PushToEdges[pos.square(PieceType.KING, weakSide)]);
        return strongSide == pos.side_to_move() ? result : -result;
    }
}

/// KR vs KN. The attacking side has slightly better winning chances than
/// in KR vs KB, particularly if the king and the knight are far apart.
internal class EndgameKRKN : EndgameValue
{
    internal EndgameKRKN(ColorT c)
        : base(c)
    {
    }

    internal override Value GetValue(Position pos)
    {
        Debug.Assert(verify_material(pos, strongSide, Value.RookValueMg, 0));
        Debug.Assert(verify_material(pos, weakSide, Value.KnightValueMg, 0));

        var bksq = pos.square(PieceType.KING, weakSide);
        var bnsq = pos.square(PieceType.KNIGHT, weakSide);
        var result = new Value(PushToEdges[bksq] + PushAway[Utils.distance_Square(bksq, bnsq)]);
        return strongSide == pos.side_to_move() ? result : -result;
    }
}

/// KQ vs KP. In general, this is a win for the stronger side, but there are a
/// few important exceptions. A pawn on 7th rank and on the A,C,F or H files
/// with a king positioned next to it can be a draw, so in that case, we only
/// use the distance between the kings.
internal class EndgameKQKP : EndgameValue
{
    internal EndgameKQKP(ColorT c)
        : base(c)
    {
    }

    internal override Value GetValue(Position pos)
    {
        Debug.Assert(verify_material(pos, strongSide, Value.QueenValueMg, 0));
        Debug.Assert(verify_material(pos, weakSide, Value.VALUE_ZERO, 1));

        var winnerKSq = pos.square(PieceType.KING, strongSide);
        var loserKSq = pos.square(PieceType.KING, weakSide);
        var pawnSq = pos.square(PieceType.PAWN, weakSide);

        var result = new Value(PushClose[Utils.distance_Square(winnerKSq, loserKSq)]);

        if (Rank.relative_rank(weakSide, pawnSq) != Rank.RANK_7 || Utils.distance_Square(loserKSq, pawnSq) != 1
            || !((Bitboard.FileABB | Bitboard.FileCBB | Bitboard.FileFBB | Bitboard.FileHBB) & pawnSq))
        {
            result += Value.QueenValueEg - Value.PawnValueEg;
        }

        return strongSide == pos.side_to_move() ? result : -result;
    }
}

/// KQ vs KR.  This is almost identical to KX vs K:  We give the attacking
/// king a bonus for having the kings close together, and for forcing the
/// defending king towards the edge. If we also take care to avoid null move for
/// the defending side in the search, this is usually sufficient to win KQ vs KR.
internal class EndgameKQKR : EndgameValue
{
    internal EndgameKQKR(ColorT c)
        : base(c)
    {
    }

    internal override Value GetValue(Position pos)
    {
        Debug.Assert(verify_material(pos, strongSide, Value.QueenValueMg, 0));
        Debug.Assert(verify_material(pos, weakSide, Value.RookValueMg, 0));

        var winnerKSq = pos.square(PieceType.KING, strongSide);
        var loserKSq = pos.square(PieceType.KING, weakSide);

        var result = Value.QueenValueEg - Value.RookValueEg + PushToEdges[loserKSq]
                     + PushClose[Utils.distance_Square(winnerKSq, loserKSq)];

        return strongSide == pos.side_to_move() ? result : -result;
    }
}

/// Some cases of trivial draws
internal class EndgameKNNK : EndgameValue
{
    internal EndgameKNNK(ColorT c)
        : base(c)
    {
    }

    internal override Value GetValue(Position pos)
    {
        return Value.VALUE_DRAW;
    }
}

/// KB and one or more pawns vs K. It checks for draws with rook pawns and
/// a bishop of the wrong color. If such a draw is detected, SCALE_FACTOR_DRAW
/// is returned. If not, the return value is SCALE_FACTOR_NONE, i.e. no scaling
/// will be used.
internal class EndgameKBPsK : EndgameScaleFactor
{
    internal EndgameKBPsK(ColorT c)
        : base(c)
    {
    }

    internal override ScaleFactor GetScaleFactor(Position pos)
    {
        Debug.Assert(pos.non_pawn_material(strongSide) == Value.BishopValueMg);
        Debug.Assert(pos.count(PieceType.PAWN, strongSide) >= 1);

        // No assertions about the material of weakSide, because we want draws to
        // be detected even when the weaker side has some pawns.

        var pawns = pos.pieces_CtPt(strongSide, PieceType.PAWN);
        var pawnsFile = Square.file_of(Utils.lsb(pawns));

        // All pawns are on a single rook file?
        if ((pawnsFile == File.FILE_A || pawnsFile == File.FILE_H) && !(pawns & ~Utils.file_bb(pawnsFile)))
        {
            var bishopSq = pos.square(PieceType.BISHOP, strongSide);
            var queeningSq = Square.relative_square(strongSide, Square.make_square(pawnsFile, Rank.RANK_8));
            var kingSq = pos.square(PieceType.KING, weakSide);

            if (Square.opposite_colors(queeningSq, bishopSq) && Utils.distance_Square(queeningSq, kingSq) <= 1)
            {
                return ScaleFactor.SCALE_FACTOR_DRAW;
            }
        }

        // If all the pawns are on the same B or G file, then it's potentially a draw
        if ((pawnsFile == File.FILE_B || pawnsFile == File.FILE_G)
            && !(pos.pieces_Pt(PieceType.PAWN) & ~Utils.file_bb(pawnsFile)) && pos.non_pawn_material(weakSide) == 0
            && pos.count(PieceType.PAWN, weakSide) >= 1)
        {
            // Get weakSide pawn that is closest to the home rank
            var weakPawnSq = Utils.backmost_sq(weakSide, pos.pieces_CtPt(weakSide, PieceType.PAWN));

            var strongKingSq = pos.square(PieceType.KING, strongSide);
            var weakKingSq = pos.square(PieceType.KING, weakSide);
            var bishopSq = pos.square(PieceType.BISHOP, strongSide);

            // There's potential for a draw if our pawn is blocked on the 7th rank,
            // the bishop cannot attack it or they only have one pawn left
            if (Rank.relative_rank(strongSide, weakPawnSq) == Rank.RANK_7
                && (pos.pieces_CtPt(strongSide, PieceType.PAWN) & (weakPawnSq + Square.pawn_push(weakSide)))
                && (Square.opposite_colors(bishopSq, weakPawnSq) || pos.count(PieceType.PAWN, strongSide) == 1))
            {
                var strongKingDist = Utils.distance_Square(weakPawnSq, strongKingSq);
                var weakKingDist = Utils.distance_Square(weakPawnSq, weakKingSq);

                // It's a draw if the weak king is on its back two ranks, within 2
                // squares of the blocking pawn and the strong king is not
                // closer. (I think this rule only fails in practically
                // unreachable positions such as 5k1K/6p1/6P1/8/8/3B4/8/8 w
                // and positions where qsearch will immediately correct the
                // problem such as 8/4k1p1/6P1/1K6/3B4/8/8/8 w)
                if (Rank.relative_rank(strongSide, weakKingSq) >= Rank.RANK_7 && weakKingDist <= 2
                    && weakKingDist <= strongKingDist)
                {
                    return ScaleFactor.SCALE_FACTOR_DRAW;
                }
            }
        }

        return ScaleFactor.SCALE_FACTOR_NONE;
    }
}

/// KQ vs KR and one or more pawns. It tests for fortress draws with a rook on
/// the third rank defended by a pawn.
internal class EndgameKQKRPs : EndgameScaleFactor
{
    internal EndgameKQKRPs(ColorT c)
        : base(c)
    {
    }

    internal override ScaleFactor GetScaleFactor(Position pos)
    {
        Debug.Assert(verify_material(pos, strongSide, Value.QueenValueMg, 0));
        Debug.Assert(pos.count(PieceType.ROOK, weakSide) == 1);
        Debug.Assert(pos.count(PieceType.PAWN, weakSide) >= 1);

        var kingSq = pos.square(PieceType.KING, weakSide);
        var rsq = pos.square(PieceType.ROOK, weakSide);

        if (Rank.relative_rank(weakSide, kingSq) <= Rank.RANK_2
            && Rank.relative_rank(weakSide, pos.square(PieceType.KING, strongSide)) >= Rank.RANK_4
            && Rank.relative_rank(weakSide, rsq) == Rank.RANK_3
            && (pos.pieces_CtPt(weakSide, PieceType.PAWN) & pos.attacks_from_PtS(PieceType.KING, kingSq)
                & pos.attacks_from_PS(PieceType.PAWN, rsq, strongSide)))
        {
            return ScaleFactor.SCALE_FACTOR_DRAW;
        }

        return ScaleFactor.SCALE_FACTOR_NONE;
    }
}

/// KRP vs KR. This function knows a handful of the most important classes of
/// drawn positions, but is far from perfect. It would probably be a good idea
/// to add more knowledge in the future.
/// 
/// It would also be nice to rewrite the actual code for this function,
/// which is mostly copied from Glaurung 1.x, and isn't very pretty.
internal class EndgameKRPKR : EndgameScaleFactor
{
    internal EndgameKRPKR(ColorT c)
        : base(c)
    {
    }

    internal override ScaleFactor GetScaleFactor(Position pos)
    {
        Debug.Assert(verify_material(pos, strongSide, Value.RookValueMg, 1));
        Debug.Assert(verify_material(pos, weakSide, Value.RookValueMg, 0));

        // Assume strongSide is white and the pawn is on files A-D
        var wksq = normalize(pos, strongSide, pos.square(PieceType.KING, strongSide));
        var bksq = normalize(pos, strongSide, pos.square(PieceType.KING, weakSide));
        var wrsq = normalize(pos, strongSide, pos.square(PieceType.ROOK, strongSide));
        var wpsq = normalize(pos, strongSide, pos.square(PieceType.PAWN, strongSide));
        var brsq = normalize(pos, strongSide, pos.square(PieceType.ROOK, weakSide));

        var f = Square.file_of(wpsq);
        var r = Square.rank_of(wpsq);
        var queeningSq = Square.make_square(f, Rank.RANK_8);
        var tempo = (pos.side_to_move() == strongSide) ? 1 : 0;

        // If the pawn is not too far advanced and the defending king defends the
        // queening square, use the third-rank defence.
        if (r <= Rank.RANK_5 && Utils.distance_Square(bksq, queeningSq) <= 1 && wksq <= Square.SQ_H5
            && (Square.rank_of(brsq) == Rank.RANK_6 || (r <= Rank.RANK_3 && Square.rank_of(wrsq) != Rank.RANK_6)))
        {
            return ScaleFactor.SCALE_FACTOR_DRAW;
        }

        // The defending side saves a draw by checking from behind in case the pawn
        // has advanced to the 6th rank with the king behind.
        if (r == Rank.RANK_6 && Utils.distance_Square(bksq, queeningSq) <= 1
            && (int)Square.rank_of(wksq) + tempo <= Rank.RANK_6
            && (Square.rank_of(brsq) == Rank.RANK_1 || (tempo == 0 && Utils.distance_File(brsq, wpsq) >= 3)))
        {
            return ScaleFactor.SCALE_FACTOR_DRAW;
        }

        if (r >= Rank.RANK_6 && bksq == queeningSq && Square.rank_of(brsq) == Rank.RANK_1
            && (tempo == 0 || Utils.distance_Square(wksq, wpsq) >= 2))
        {
            return ScaleFactor.SCALE_FACTOR_DRAW;
        }

        // White pawn on a7 and rook on a8 is a draw if black's king is on g7 or h7
        // and the black rook is behind the pawn.
        if (wpsq == Square.SQ_A7 && wrsq == Square.SQ_A8 && (bksq == Square.SQ_H7 || bksq == Square.SQ_G7)
            && Square.file_of(brsq) == File.FILE_A
            && (Square.rank_of(brsq) <= Rank.RANK_3 || Square.file_of(wksq) >= File.FILE_D
                || Square.rank_of(wksq) <= Rank.RANK_5))
        {
            return ScaleFactor.SCALE_FACTOR_DRAW;
        }

        // If the defending king blocks the pawn and the attacking king is too far
        // away, it's a draw.
        if (r <= Rank.RANK_5 && bksq == wpsq + Square.DELTA_N && Utils.distance_Square(wksq, wpsq) - tempo >= 2
            && Utils.distance_Square(wksq, brsq) - tempo >= 2)
        {
            return ScaleFactor.SCALE_FACTOR_DRAW;
        }

        // Pawn on the 7th rank supported by the rook from behind usually wins if the
        // attacking king is closer to the queening square than the defending king,
        // and the defending king cannot gain tempi by threatening the attacking rook.
        if (r == Rank.RANK_7 && f != File.FILE_A && Square.file_of(wrsq) == f && wrsq != queeningSq
            && (Utils.distance_Square(wksq, queeningSq) < Utils.distance_Square(bksq, queeningSq) - 2 + tempo)
            && (Utils.distance_Square(wksq, queeningSq) < Utils.distance_Square(bksq, wrsq) + tempo))
        {
            return ScaleFactor.SCALE_FACTOR_MAX - 2*Utils.distance_Square(wksq, queeningSq);
        }

        // Similar to the above, but with the pawn further back
        if (f != File.FILE_A && Square.file_of(wrsq) == f && wrsq < wpsq
            && (Utils.distance_Square(wksq, queeningSq) < Utils.distance_Square(bksq, queeningSq) - 2 + tempo)
            && (Utils.distance_Square(wksq, wpsq + Square.DELTA_N)
                < Utils.distance_Square(bksq, wpsq + Square.DELTA_N) - 2 + tempo)
            && (Utils.distance_Square(bksq, wrsq) + tempo >= 3
                || (Utils.distance_Square(wksq, queeningSq) < Utils.distance_Square(bksq, wrsq) + tempo
                    && (Utils.distance_Square(wksq, wpsq + Square.DELTA_N) < Utils.distance_Square(bksq, wrsq) + tempo))))
        {
            return ScaleFactor.SCALE_FACTOR_MAX - 8*Utils.distance_Square(wpsq, queeningSq)
                   - 2*Utils.distance_Square(wksq, queeningSq);
        }

        // If the pawn is not far advanced and the defending king is somewhere in
        // the pawn's path, it's probably a draw.
        if (r <= Rank.RANK_4 && bksq > wpsq)
        {
            if (Square.file_of(bksq) == Square.file_of(wpsq))
            {
                return (ScaleFactor) (10);
            }
            if (Utils.distance_File(bksq, wpsq) == 1 && Utils.distance_Square(wksq, bksq) > 2)
            {
                return (ScaleFactor) (24 - 2*Utils.distance_Square(wksq, bksq));
            }
        }
        return ScaleFactor.SCALE_FACTOR_NONE;
    }
}

internal class EndgameKRPKB : EndgameScaleFactor
{
    internal EndgameKRPKB(ColorT c)
        : base(c)
    {
    }

    internal override ScaleFactor GetScaleFactor(Position pos)
    {
        Debug.Assert(verify_material(pos, strongSide, Value.RookValueMg, 1));
        Debug.Assert(verify_material(pos, weakSide, Value.BishopValueMg, 0));

        // Test for a rook pawn
        if (pos.pieces_Pt(PieceType.PAWN) & (Bitboard.FileABB | Bitboard.FileHBB))
        {
            var ksq = pos.square(PieceType.KING, weakSide);
            var bsq = pos.square(PieceType.BISHOP, weakSide);
            var psq = pos.square(PieceType.PAWN, strongSide);
            var rk = Rank.relative_rank(strongSide, psq);
            var push = Square.pawn_push(strongSide);

            // If the pawn is on the 5th rank and the pawn (currently) is on
            // the same color square as the bishop then there is a chance of
            // a fortress. Depending on the king position give a moderate
            // reduction or a stronger one if the defending king is near the
            // corner but not trapped there.
            if (rk == Rank.RANK_5 && !Square.opposite_colors(bsq, psq))
            {
                var d = Utils.distance_Square(psq + 3*push, ksq);

                if (d <= 2 && !(d == 0 && ksq == pos.square(PieceType.KING, strongSide) + 2*push))
                {
                    return (ScaleFactor) (24);
                }
                return (ScaleFactor) (48);
            }

            // When the pawn has moved to the 6th rank we can be fairly sure
            // it's drawn if the bishop attacks the square in front of the
            // pawn from a reasonable distance and the defending king is near
            // the corner
            if (rk == Rank.RANK_6 && Utils.distance_Square(psq + 2*push, ksq) <= 1
                && (Utils.PseudoAttacks[PieceType.BISHOP, bsq] & (psq + push)) && Utils.distance_File(bsq, psq) >= 2)
            {
                return (ScaleFactor) (8);
            }
        }

        return ScaleFactor.SCALE_FACTOR_NONE;
    }
}

/// KRPP vs KRP. There is just a single rule: if the stronger side has no passed
/// pawns and the defending king is actively placed, the position is drawish.
internal class EndgameKRPPKRP : EndgameScaleFactor
{
    internal EndgameKRPPKRP(ColorT c)
        : base(c)
    {
    }

    internal override ScaleFactor GetScaleFactor(Position pos)
    {
        Debug.Assert(verify_material(pos, strongSide, Value.RookValueMg, 2));
        Debug.Assert(verify_material(pos, weakSide, Value.RookValueMg, 1));

        var wpsq1 = pos.square(PieceType.PAWN, strongSide, 0);
        var wpsq2 = pos.square(PieceType.PAWN, strongSide, 1);
        var bksq = pos.square(PieceType.KING, weakSide);

        // Does the stronger side have a passed pawn?
        if (pos.pawn_passed(strongSide, wpsq1) || pos.pawn_passed(strongSide, wpsq2))
        {
            return ScaleFactor.SCALE_FACTOR_NONE;
        }

        var r = Math.Max(Rank.relative_rank(strongSide, wpsq1), Rank.relative_rank(strongSide, wpsq2));

        if (Utils.distance_File(bksq, wpsq1) <= 1 && Utils.distance_File(bksq, wpsq2) <= 1
            && Rank.relative_rank(strongSide, bksq) > r)
        {
            switch (r)
            {
                case 1 /*Rank.RANK_2*/:
                    return (ScaleFactor) (9);
                case 2 /*Rank.RANK_3*/:
                    return (ScaleFactor) (10);
                case 3 /*Rank.RANK_4*/:
                    return (ScaleFactor) (14);
                case 4 /*Rank.RANK_5*/:
                    return (ScaleFactor) (21);
                case 5 /*Rank.RANK_6*/:
                    return (ScaleFactor) (44);
                default:
                    Debug.Assert(false);
                    break;
            }
        }
        return ScaleFactor.SCALE_FACTOR_NONE;
    }
}

/// K and two or more pawns vs K. There is just a single rule here: If all pawns
/// are on the same rook file and are blocked by the defending king, it's a draw.
internal class EndgameKPsK : EndgameScaleFactor
{
    internal EndgameKPsK(ColorT c)
        : base(c)
    {
    }

    internal override ScaleFactor GetScaleFactor(Position pos)
    {
        Debug.Assert(pos.non_pawn_material(strongSide) == Value.VALUE_ZERO);
        Debug.Assert(pos.count(PieceType.PAWN, strongSide) >= 2);
        Debug.Assert(verify_material(pos, weakSide, Value.VALUE_ZERO, 0));

        var ksq = pos.square(PieceType.KING, weakSide);
        var pawns = pos.pieces_CtPt(strongSide, PieceType.PAWN);

        // If all pawns are ahead of the king, on a single rook file and
        // the king is within one file of the pawns, it's a draw.
        if (!(pawns & ~Utils.in_front_bb(weakSide, Square.rank_of(ksq)))
            && !((bool) (pawns & ~Bitboard.FileABB) && (pawns & ~Bitboard.FileHBB))
            && Utils.distance_File(ksq, Utils.lsb(pawns)) <= 1)
        {
            return ScaleFactor.SCALE_FACTOR_DRAW;
        }

        return ScaleFactor.SCALE_FACTOR_NONE;
    }
}

/// KBP vs KB. There are two rules: if the defending king is somewhere along the
/// path of the pawn, and the square of the king is not of the same color as the
/// stronger side's bishop, it's a draw. If the two bishops have opposite color,
/// it's almost always a draw.
internal class EndgameKBPKB : EndgameScaleFactor
{
    internal EndgameKBPKB(ColorT c)
        : base(c)
    {
    }

    internal override ScaleFactor GetScaleFactor(Position pos)
    {
        Debug.Assert(verify_material(pos, strongSide, Value.BishopValueMg, 1));
        Debug.Assert(verify_material(pos, weakSide, Value.BishopValueMg, 0));

        var pawnSq = pos.square(PieceType.PAWN, strongSide);
        var strongBishopSq = pos.square(PieceType.BISHOP, strongSide);
        var weakBishopSq = pos.square(PieceType.BISHOP, weakSide);
        var weakKingSq = pos.square(PieceType.KING, weakSide);

        // Case 1: Defending king blocks the pawn, and cannot be driven away
        if (Square.file_of(weakKingSq) == Square.file_of(pawnSq)
            && Rank.relative_rank(strongSide, pawnSq) < Rank.relative_rank(strongSide, weakKingSq)
            && (Square.opposite_colors(weakKingSq, strongBishopSq)
                || Rank.relative_rank(strongSide, weakKingSq) <= Rank.RANK_6))
        {
            return ScaleFactor.SCALE_FACTOR_DRAW;
        }

        // Case 2: Opposite colored bishops
        if (Square.opposite_colors(strongBishopSq, weakBishopSq))
        {
            // We assume that the position is drawn in the following three situations:
            //
            //   a. The pawn is on rank 5 or further back.
            //   b. The defending king is somewhere in the pawn's path.
            //   c. The defending bishop attacks some square along the pawn's path,
            //      and is at least three squares away from the pawn.
            //
            // These rules are probably not perfect, but in practice they work
            // reasonably well.

            if (Rank.relative_rank(strongSide, pawnSq) <= Rank.RANK_5)
            {
                return ScaleFactor.SCALE_FACTOR_DRAW;
            }
            var path = Utils.forward_bb(strongSide, pawnSq);

            if (path & pos.pieces_CtPt(weakSide, PieceType.KING))
            {
                return ScaleFactor.SCALE_FACTOR_DRAW;
            }

            if ((pos.attacks_from_PtS(PieceType.BISHOP, weakBishopSq) & path)
                && Utils.distance_Square(weakBishopSq, pawnSq) >= 3)
            {
                return ScaleFactor.SCALE_FACTOR_DRAW;
            }
        }
        return ScaleFactor.SCALE_FACTOR_NONE;
    }
}

/// KBPP vs KB. It detects a few basic draws with opposite-colored bishops
internal class EndgameKBPPKB : EndgameScaleFactor
{
    internal EndgameKBPPKB(ColorT c)
        : base(c)
    {
    }

    internal override ScaleFactor GetScaleFactor(Position pos)
    {
        Debug.Assert(verify_material(pos, strongSide, Value.BishopValueMg, 2));
        Debug.Assert(verify_material(pos, weakSide, Value.BishopValueMg, 0));

        var wbsq = pos.square(PieceType.BISHOP, strongSide);
        var bbsq = pos.square(PieceType.BISHOP, weakSide);

        if (!Square.opposite_colors(wbsq, bbsq))
        {
            return ScaleFactor.SCALE_FACTOR_NONE;
        }

        var ksq = pos.square(PieceType.KING, weakSide);
        var psq1 = pos.square(PieceType.PAWN, strongSide, 0);
        var psq2 = pos.square(PieceType.PAWN, strongSide, 1);
        var r1 = Square.rank_of(psq1);
        var r2 = Square.rank_of(psq2);
        Square blockSq1, blockSq2;

        if (Rank.relative_rank(strongSide, psq1) > Rank.relative_rank(strongSide, psq2))
        {
            blockSq1 = psq1 + Square.pawn_push(strongSide);
            blockSq2 = Square.make_square(Square.file_of(psq2), Square.rank_of(psq1));
        }
        else
        {
            blockSq1 = psq2 + Square.pawn_push(strongSide);
            blockSq2 = Square.make_square(Square.file_of(psq1), Square.rank_of(psq2));
        }

        switch (Utils.distance_File(psq1, psq2))
        {
            case 0:
                // Both pawns are on the same file. It's an easy draw if the defender firmly
                // controls some square in the frontmost pawn's path.
                if (Square.file_of(ksq) == Square.file_of(blockSq1)
                    && Rank.relative_rank(strongSide, ksq) >= Rank.relative_rank(strongSide, blockSq1)
                    && Square.opposite_colors(ksq, wbsq))
                {
                    return ScaleFactor.SCALE_FACTOR_DRAW;
                }
                return ScaleFactor.SCALE_FACTOR_NONE;

            case 1:
                // Pawns on adjacent files. It's a draw if the defender firmly controls the
                // square in front of the frontmost pawn's path, and the square diagonally
                // behind this square on the file of the other pawn.
                if (ksq == blockSq1 && Square.opposite_colors(ksq, wbsq)
                    && (bbsq == blockSq2
                        || (pos.attacks_from_PtS(PieceType.BISHOP, blockSq2) & pos.pieces_CtPt(weakSide, PieceType.BISHOP))
                        || Utils.distance_Rank(r1, r2) >= 2))
                {
                    return ScaleFactor.SCALE_FACTOR_DRAW;
                }

                if (ksq == blockSq2 && Square.opposite_colors(ksq, wbsq)
                    && (bbsq == blockSq1
                        || (pos.attacks_from_PtS(PieceType.BISHOP, blockSq1) & pos.pieces_CtPt(weakSide, PieceType.BISHOP))))
                {
                    return ScaleFactor.SCALE_FACTOR_DRAW;
                }
                return ScaleFactor.SCALE_FACTOR_NONE;

            default:
                // The pawns are not on the same file or adjacent files. No scaling.
                return ScaleFactor.SCALE_FACTOR_NONE;
        }
    }
}

/// KBP vs KN. There is a single rule: If the defending king is somewhere along
/// the path of the pawn, and the square of the king is not of the same color as
/// the stronger side's bishop, it's a draw.
internal class EndgameKBPKN : EndgameScaleFactor
{
    internal EndgameKBPKN(ColorT c)
        : base(c)
    {
    }

    internal override ScaleFactor GetScaleFactor(Position pos)
    {
        Debug.Assert(verify_material(pos, strongSide, Value.BishopValueMg, 1));
        Debug.Assert(verify_material(pos, weakSide, Value.KnightValueMg, 0));

        var pawnSq = pos.square(PieceType.PAWN, strongSide);
        var strongBishopSq = pos.square(PieceType.BISHOP, strongSide);
        var weakKingSq = pos.square(PieceType.KING, weakSide);

        if (Square.file_of(weakKingSq) == Square.file_of(pawnSq)
            && Rank.relative_rank(strongSide, pawnSq) < Rank.relative_rank(strongSide, weakKingSq)
            && (Square.opposite_colors(weakKingSq, strongBishopSq)
                || Rank.relative_rank(strongSide, weakKingSq) <= Rank.RANK_6))
        {
            return ScaleFactor.SCALE_FACTOR_DRAW;
        }

        return ScaleFactor.SCALE_FACTOR_NONE;
    }
}

/// KNP vs K. There is a single rule: if the pawn is a rook pawn on the 7th rank
/// and the defending king prevents the pawn from advancing, the position is drawn.
internal class EndgameKNPK : EndgameScaleFactor
{
    internal EndgameKNPK(ColorT c)
        : base(c)
    {
    }

    internal override ScaleFactor GetScaleFactor(Position pos)
    {
        Debug.Assert(verify_material(pos, strongSide, Value.KnightValueMg, 1));
        Debug.Assert(verify_material(pos, weakSide, Value.VALUE_ZERO, 0));

        // Assume strongSide is white and the pawn is on files A-D
        var pawnSq = normalize(pos, strongSide, pos.square(PieceType.PAWN, strongSide));
        var weakKingSq = normalize(pos, strongSide, pos.square(PieceType.KING, weakSide));

        if (pawnSq == Square.SQ_A7 && Utils.distance_Square(Square.SQ_A8, weakKingSq) <= 1)
        {
            return ScaleFactor.SCALE_FACTOR_DRAW;
        }

        return ScaleFactor.SCALE_FACTOR_NONE;
    }
}

/// KNP vs KB. If knight can block bishop from taking pawn, it's a win.
/// Otherwise the position is drawn.
internal class EndgameKNPKB : EndgameScaleFactor
{
    internal EndgameKNPKB(ColorT c)
        : base(c)
    {
    }

    internal override ScaleFactor GetScaleFactor(Position pos)
    {
        var pawnSq = pos.square(PieceType.PAWN, strongSide);
        var bishopSq = pos.square(PieceType.BISHOP, weakSide);
        var weakKingSq = pos.square(PieceType.KING, weakSide);

        // King needs to get close to promoting pawn to prevent knight from blocking.
        // Rules for this are very tricky, so just approximate.
        if (Utils.forward_bb(strongSide, pawnSq) & pos.attacks_from_PtS(PieceType.BISHOP, bishopSq))
        {
            return (ScaleFactor) (Utils.distance_Square(weakKingSq, pawnSq));
        }

        return ScaleFactor.SCALE_FACTOR_NONE;
    }
}

/// KP vs KP. This is done by removing the weakest side's pawn and probing the
/// KP vs K bitbase: If the weakest side has a draw without the pawn, it probably
/// has at least a draw with the pawn as well. The exception is when the stronger
/// side's pawn is far advanced and not on a rook file; in this case it is often
/// possible to win (e.g. 8/4k3/3p4/3P4/6K1/8/8/8 w - - 0 1).
internal class EndgameKPKP : EndgameScaleFactor
{
    internal EndgameKPKP(ColorT c)
        : base(c)
    {
    }

    internal override ScaleFactor GetScaleFactor(Position pos)
    {
        Debug.Assert(verify_material(pos, strongSide, Value.VALUE_ZERO, 1));
        Debug.Assert(verify_material(pos, weakSide, Value.VALUE_ZERO, 1));

        // Assume strongSide is white and the pawn is on files A-D
        var wksq = normalize(pos, strongSide, pos.square(PieceType.KING, strongSide));
        var bksq = normalize(pos, strongSide, pos.square(PieceType.KING, weakSide));
        var psq = normalize(pos, strongSide, pos.square(PieceType.PAWN, strongSide));

        var us = strongSide == pos.side_to_move() ? Color.WHITE : Color.BLACK;

        // If the pawn has advanced to the fifth rank or further, and is not a
        // rook pawn, it's too dangerous to assume that it's at least a draw.
        if (Square.rank_of(psq) >= Rank.RANK_5 && Square.file_of(psq) != File.FILE_A)
        {
            return ScaleFactor.SCALE_FACTOR_NONE;
        }

        // Probe the KPK bitbase with the weakest side's pawn removed. If it's a draw,
        // it's probably at least a draw even with the pawn.
        return Bitbases.probe(wksq, psq, bksq, us) ? ScaleFactor.SCALE_FACTOR_NONE : ScaleFactor.SCALE_FACTOR_DRAW;
    }
}