using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

public static class Eval
{
    public static Value Tempo = new Value(17); // Must be visible to search

    // Evaluation weights, indexed by the corresponding evaluation term
    private static readonly int Mobility = 0;

    private static readonly int PawnStructure = 1;

    private static readonly int PassedPawns = 2;

    private static readonly int Space = 3;

    private static readonly int KingSafety = 4;

    public static Weight[] Weights =
        {
            new Weight(289, 344), new Weight(233, 201), new Weight(221, 273),
            new Weight(46, 0), new Weight(322, 0)
        };

    // MobilityBonus[PieceType][attacked] contains bonuses for middle and end
    // game, indexed by piece type and number of attacked squares not occupied by
    // friendly pieces.
    public static Score[][] MobilityBonus =
        {
            new Score[] { }, new Score[] { },
            new[]
                {
                    Score.make_score(-68, -49), Score.make_score(-46, -33),
                    Score.make_score(-3, -12), Score.make_score(5, -4),
                    Score.make_score(9, 11), Score.make_score(15, 16), // Knights
                    Score.make_score(23, 27), Score.make_score(33, 28),
                    Score.make_score(37, 29)
                },
            new[]
                {
                    Score.make_score(-49, -44), Score.make_score(-23, -16),
                    Score.make_score(16, 1), Score.make_score(29, 16),
                    Score.make_score(40, 25), Score.make_score(51, 34), // Bishops
                    Score.make_score(55, 43), Score.make_score(61, 49),
                    Score.make_score(64, 51), Score.make_score(68, 52),
                    Score.make_score(73, 55), Score.make_score(75, 60),
                    Score.make_score(80, 65), Score.make_score(86, 66)
                },
            new[]
                {
                    Score.make_score(-50, -57), Score.make_score(-28, -22),
                    Score.make_score(-11, 7), Score.make_score(-1, 29),
                    Score.make_score(0, 39), Score.make_score(1, 46), // Rooks
                    Score.make_score(10, 66), Score.make_score(16, 79),
                    Score.make_score(22, 86), Score.make_score(23, 103),
                    Score.make_score(30, 109), Score.make_score(33, 111),
                    Score.make_score(37, 115), Score.make_score(38, 119),
                    Score.make_score(48, 124)
                },
            new[]
                {
                    Score.make_score(-43, -30), Score.make_score(-27, -15),
                    Score.make_score(1, -5), Score.make_score(2, -3),
                    Score.make_score(14, 10), Score.make_score(18, 24), // Queens
                    Score.make_score(20, 27), Score.make_score(33, 37),
                    Score.make_score(33, 38), Score.make_score(34, 43),
                    Score.make_score(40, 46), Score.make_score(43, 56),
                    Score.make_score(46, 61), Score.make_score(52, 63),
                    Score.make_score(52, 63), Score.make_score(57, 65),
                    Score.make_score(60, 70), Score.make_score(61, 74),
                    Score.make_score(67, 80), Score.make_score(76, 82),
                    Score.make_score(77, 88), Score.make_score(82, 94),
                    Score.make_score(86, 95), Score.make_score(90, 96),
                    Score.make_score(94, 99), Score.make_score(96, 100),
                    Score.make_score(99, 111), Score.make_score(99, 112)
                }
        };

    // Outpost[knight/bishop][supported by pawn] contains bonuses for knights and
    // bishops outposts, bigger if outpost piece is supported by a pawn.
    public static Score[][] Outpost =
        {
            new[] { Score.make_score(42, 11), Score.make_score(63, 17) }, // Knights
            new[] { Score.make_score(18, 5), Score.make_score(27, 8) } // Bishops
        };

    // Threat[defended/weak][minor/rook attacking][attacked PieceType] contains
    // bonuses according to which piece type attacks which one.
    public static Score[][][] Threat =
        {
            new[]
                {
                    new[]
                        {
                            Score.make_score(0, 0), Score.make_score(0, 0),
                            Score.make_score(19, 37), Score.make_score(24, 37),
                            Score.make_score(44, 97), Score.make_score(35, 106)
                        },
                    // Minor on Defended
                    new[]
                        {
                            Score.make_score(0, 0), Score.make_score(0, 0),
                            Score.make_score(9, 14), Score.make_score(9, 14),
                            Score.make_score(7, 14), Score.make_score(24, 48)
                        }
                },
            // Rook on Defended
            new[]
                {
                    new[]
                        {
                            Score.make_score(0, 0), Score.make_score(0, 32),
                            Score.make_score(33, 41), Score.make_score(31, 50),
                            Score.make_score(41, 100), Score.make_score(35, 104)
                        },
                    // Minor on Weak
                    new[]
                        {
                            Score.make_score(0, 0), Score.make_score(0, 27),
                            Score.make_score(26, 57), Score.make_score(26, 57),
                            Score.make_score(0, 43), Score.make_score(23, 51)
                        }
                }
            // Rook on Weak
        };

    // ThreatenedByPawn[PieceType] contains a penalty according to which piece
    // type is attacked by an enemy pawn.
    public static Score[] ThreatenedByPawn =
        {
            Score.make_score(0, 0), Score.make_score(0, 0),
            Score.make_score(107, 138), Score.make_score(84, 122),
            Score.make_score(114, 203), Score.make_score(121, 217)
        };

    // Passed[mg/eg][rank] contains midgame and endgame bonuses for passed pawns.
    // We don't use a Score because we process the two components independently.
    public static Value[][] Passed =
        {
            new[]
                {
                    new Value(0), new Value(1), new Value(34), new Value(90), new Value(214),
                    new Value(328)
                },
            new[]
                {
                    new Value(7), new Value(14), new Value(37), new Value(63), new Value(134),
                    new Value(189)
                }
        };

    // PassedFile[File] contains a bonus according to the file of a passed pawn.
    public static Score[] PassedFile =
        {
            Score.make_score(14, 13), Score.make_score(2, 5), Score.make_score(-3, -4),
            Score.make_score(-19, -14), Score.make_score(-19, -14),
            Score.make_score(-3, -4), Score.make_score(2, 5), Score.make_score(14, 13)
        };

    public static Score ThreatenedByHangingPawn = Score.make_score(40, 60);

    // Assorted bonuses and penalties used by evaluation
    public static Score KingOnOne = Score.make_score(2, 58);

    public static Score KingOnMany = Score.make_score(6, 125);

    public static Score RookOnPawn = Score.make_score(7, 27);

    public static Score RookOnOpenFile = Score.make_score(43, 21);

    public static Score RookOnSemiOpenFile = Score.make_score(19, 10);

    public static Score BishopPawns = Score.make_score(8, 12);

    public static Score MinorBehindPawn = Score.make_score(16, 0);

    public static Score TrappedRook = Score.make_score(92, 0);

    public static Score Unstoppable = Score.make_score(0, 20);

    public static Score Hanging = Score.make_score(31, 26);

    public static Score PawnAttackThreat = Score.make_score(20, 20);

    public static Score Checked = Score.make_score(20, 20);

    // Penalty for a bishop on a1/h1 (a8/h8 for black) which is trapped by
    // a friendly pawn on b2/g2 (b7/g7 for black). This can obviously only
    // happen in Chess960 games.
    public static Score TrappedBishopA1H1 = Score.make_score(50, 50);

    // SpaceMask[Color] contains the area of the board which is considered
    // by the space evaluation. In the middlegame, each side is given a bonus
    // based on how many squares inside this area are safe and available for
    // friendly minor pieces.
    public static Bitboard[] SpaceMask =
        {
            (Bitboard.FileCBB | Bitboard.FileDBB | Bitboard.FileEBB | Bitboard.FileFBB)
            & (Bitboard.Rank2BB | Bitboard.Rank3BB | Bitboard.Rank4BB),
            (Bitboard.FileCBB | Bitboard.FileDBB | Bitboard.FileEBB | Bitboard.FileFBB)
            & (Bitboard.Rank7BB | Bitboard.Rank6BB | Bitboard.Rank5BB)
        };

    // King danger constants and variables. The king danger scores are looked-up
    // in KingDanger[]. Various little "meta-bonuses" measuring the strength
    // of the enemy attack are added up into an integer, which is used as an
    // index to KingDanger[].
    public static Score[] KingDanger = new Score[512];

    // KingAttackWeights[PieceType] contains king attack weights by piece type
    public static int[] KingAttackWeights = { 0, 0, 7, 5, 4, 1 };

    // Penalties for enemy's safe checks
    public static int QueenContactCheck = 89;

    public static int QueenCheck = 50;

    public static int RookCheck = 45;

    public static int BishopCheck = 6;

    public static int KnightCheck = 14;

    private static double[,,] scores = new double[(int)Term.TERM_NB, Color.COLOR_NB, (int)Phase.PHASE_NB];

    // init_eval_info() initializes king bitboards for given color adding
    // pawn attacks. To be done at the beginning of the evaluation.

    private static void init_eval_info(Color Us, Position pos, EvalInfo ei)
    {
        var Them = (Us == Color.WHITE ? Color.BLACK : Color.WHITE);
        var Down = (Us == Color.WHITE ? Square.DELTA_S : Square.DELTA_N);

        ei.pinnedPieces[Us] = pos.pinned_pieces(Us);
        var b = ei.attackedBy[Them, PieceType.KING] = pos.attacks_from(PieceType.KING, pos.square(PieceType.KING, Them));
        ei.attackedBy[Them, PieceType.ALL_PIECES] |= b;
        ei.attackedBy[Us, PieceType.ALL_PIECES] |= ei.attackedBy[Us, PieceType.PAWN] = ei.pi.pawn_attacks(Us);

        // Init king safety tables only if we are going to use them
        if (pos.non_pawn_material(Us) >= Value.QueenValueMg)
        {
            ei.kingRing[Them] = b | Bitboard.shift_bb(Down, b);
            b &= ei.attackedBy[Us, PieceType.PAWN];
            ei.kingAttackersCount[Us] = b ? Bitcount.popcount_Max15(b) : 0;
            ei.kingAdjacentZoneAttacksCount[Us] = ei.kingAttackersWeight[Us] = 0;
        }
        else
        {
            ei.kingRing[Them] = new Bitboard(0);
            ei.kingAttackersCount[Us] = 0;
        }
        
    }

    // evaluate_pieces() assigns bonuses and penalties to the pieces of a given color

    private static Score evaluate_pieces(
        PieceType Pt,
        Color Us,
        bool DoTrace,
        Position pos,
        EvalInfo ei,
        Score[] mobility,
        Bitboard[] mobilityArea)
    {
        if (Pt == PieceType.KING)
        {
            return Score.SCORE_ZERO;
        }
        Bitboard b;
        var score = Score.SCORE_ZERO;

        var NextPt = (Us == Color.WHITE ? Pt : Pt + 1);
        var Them = (Us == Color.WHITE ? Color.BLACK : Color.WHITE);
        var pl = pos.squares(Pt, Us);

        ei.attackedBy[Us, Pt] = new Bitboard(0);
        foreach (var s in pl.TakeWhile(s1 => s1 != Square.SQ_NONE))
        {
            // Find attacked squares, including x-ray attacks for bishops and rooks
            b = Pt == PieceType.BISHOP
                    ? Utils.attacks_bb(PieceType.BISHOP, s, pos.pieces() ^ pos.pieces(Us, PieceType.QUEEN))
                    : Pt == PieceType.ROOK
                          ? Utils.attacks_bb(
                              PieceType.ROOK,
                              s,
                              pos.pieces() ^ pos.pieces(Us, PieceType.ROOK, PieceType.QUEEN))
                          : pos.attacks_from(Pt, s);

            if (ei.pinnedPieces[Us] & s)
            {
                b &= Utils.LineBB[pos.square(PieceType.KING, Us), s];
            }

            ei.attackedBy[Us, PieceType.ALL_PIECES] |= ei.attackedBy[Us, Pt] |= b;

            if (b & ei.kingRing[Them])
            {
                ei.kingAttackersCount[Us]++;
                ei.kingAttackersWeight[Us] += KingAttackWeights[Pt];
                var bb = b & ei.attackedBy[Them, PieceType.KING];
                if (bb)
                {
                    ei.kingAdjacentZoneAttacksCount[Us] += Bitcount.popcount_Max15(bb);
                }
            }

            if (Pt == PieceType.QUEEN)
            {
                b &=
                    ~(ei.attackedBy[Them, PieceType.KNIGHT] | ei.attackedBy[Them, PieceType.BISHOP]
                      | ei.attackedBy[Them, PieceType.ROOK]);
            }

            var mob = Pt == PieceType.QUEEN
                          ? Bitcount.popcount_Full(b & mobilityArea[Us])
                          : Bitcount.popcount_Max15(b & mobilityArea[Us]);

            mobility[Us] += MobilityBonus[Pt][mob];

            if (Pt == PieceType.BISHOP || Pt == PieceType.KNIGHT)
            {
                // Bonus for outpost square
                if (Rank.relative_rank(Us, s) >= Rank.RANK_4 && Rank.relative_rank(Us, s) <= Rank.RANK_6
                    && !(pos.pieces(Them, PieceType.PAWN) & Utils.pawn_attack_span(Us, s)))
                {
                    score +=
                        Outpost[Pt == PieceType.BISHOP ? 1 : 0][(ei.attackedBy[Us, PieceType.PAWN] & s) != 0 ? 1 : 0];
                }

                // Bonus when behind a pawn
                if (Rank.relative_rank(Us, s) < Rank.RANK_5 && (pos.pieces(PieceType.PAWN) & (s + Square.pawn_push(Us))))
                {
                    score += MinorBehindPawn;
                }

                // Penalty for pawns on same color square of bishop
                if (Pt == PieceType.BISHOP)
                {
                    score -= BishopPawns * ei.pi.pawns_on_same_color_squares(Us, s);
                }

                // An important Chess960 pattern: A cornered bishop blocked by a friendly
                // pawn diagonally in front of it is a very serious problem, especially
                // when that pawn is also blocked.
                if (Pt == PieceType.BISHOP && pos.is_chess960()
                    && (s == Square.relative_square(Us, Square.SQ_A1) || s == Square.relative_square(Us, Square.SQ_H1)))
                {
                    var d = Square.pawn_push(Us) + (Square.file_of(s) == File.FILE_A ? Square.DELTA_E : Square.DELTA_W);
                    if (pos.piece_on(s + d) == Piece.make_piece(Us, PieceType.PAWN))
                    {
                        score -= !pos.empty(s + d + Square.pawn_push(Us))
                                     ? TrappedBishopA1H1 * 4
                                     : pos.piece_on(s + d + d) == Piece.make_piece(Us, PieceType.PAWN)
                                           ? TrappedBishopA1H1 * 2
                                           : TrappedBishopA1H1;
                    }
                }
            }

            if (Pt == PieceType.ROOK)
            {
                // Bonus for aligning with enemy pawns on the same rank/file
                if (Rank.relative_rank(Us, s) >= Rank.RANK_5)
                {
                    var alignedPawns = pos.pieces(Them, PieceType.PAWN) & Utils.PseudoAttacks[PieceType.ROOK, s];
                    if (alignedPawns)
                    {
                        score += Bitcount.popcount_Max15(alignedPawns) * RookOnPawn;
                    }
                }

                // Bonus when on an open or semi-open file
                if (ei.pi.semiopen_file(Us, Square.file_of(s)) != 0)
                {
                    score += ei.pi.semiopen_file(Them, Square.file_of(s)) != 0 ? RookOnOpenFile : RookOnSemiOpenFile;
                }

                // Penalize when trapped by the king, even more if king cannot castle
                if (mob <= 3 && 0 == ei.pi.semiopen_file(Us, Square.file_of(s)))
                {
                    var ksq = pos.square(PieceType.KING, Us);

                    if (((Square.file_of(ksq) < File.FILE_E) == (Square.file_of(s) < Square.file_of(ksq)))
                        && (Square.rank_of(ksq) == Square.rank_of(s) || Rank.relative_rank(Us, ksq) == Rank.RANK_1)
                        && 0 == ei.pi.semiopen_side(Us, Square.file_of(ksq), Square.file_of(s) < Square.file_of(ksq)))
                    {
                        score -= (TrappedRook - Score.make_score(mob * 22, 0)) * (1 + (pos.can_castle(Us) == 0 ? 1 : 0));
                    }
                }
            }
        }

        if (DoTrace)
        {
            add(Pt, Us, score);
        }
        // Recursively call evaluate_pieces() of next piece type until KING excluded
        return score - evaluate_pieces(NextPt, Them, DoTrace, pos, ei, mobility, mobilityArea);
    }

    // evaluate_king() assigns bonuses and penalties to a king of a given color

    private static Score evaluate_king(Color Us, bool DoTrace, Position pos, EvalInfo ei)
    {
        var Them = (Us == Color.WHITE ? Color.BLACK : Color.WHITE);

        Bitboard undefended, b, b1, b2, safe;
        int attackUnits;
        var ksq = pos.square(PieceType.KING, Us);

        // King shelter and enemy pawns storm
        var score = ei.pi.king_safety(Us, pos, ksq);

        // Main king safety evaluation
        if (ei.kingAttackersCount[Them] != 0)
        {
            // Find the attacked squares around the king which have no defenders
            // apart from the king itself
            undefended = ei.attackedBy[Them, PieceType.ALL_PIECES] & ei.attackedBy[Us, PieceType.KING]
                         & ~(ei.attackedBy[Us, PieceType.PAWN] | ei.attackedBy[Us, PieceType.KNIGHT]
                             | ei.attackedBy[Us, PieceType.BISHOP] | ei.attackedBy[Us, PieceType.ROOK]
                             | ei.attackedBy[Us, PieceType.QUEEN]);

            // Initialize the 'attackUnits' variable, which is used later on as an
            // index into the KingDanger[] array. The initial value is based on the
            // number and types of the enemy's attacking pieces, the number of
            // attacked and undefended squares around our king and the quality of
            // the pawn shelter (current 'score' value).
            attackUnits = Math.Min(72, ei.kingAttackersCount[Them] * ei.kingAttackersWeight[Them])
                          + 9 * ei.kingAdjacentZoneAttacksCount[Them] + 27 * Bitcount.popcount_Max15(undefended)
                          + 11 * (ei.pinnedPieces[Us] != 0 ? 1 : 0)
                          - 64 * (pos.count(PieceType.QUEEN, Them) == 0 ? 1 : 0) - Score.mg_value(score) / 8;

            // Analyse the enemy's safe queen contact checks. Firstly, find the
            // undefended squares around the king reachable by the enemy queen...
            b = undefended & ei.attackedBy[Them, PieceType.QUEEN] & ~pos.pieces(Them);
            if (b)
            {
                // ...and then remove squares not supported by another enemy piece
                b &= ei.attackedBy[Them, PieceType.PAWN] | ei.attackedBy[Them, PieceType.KNIGHT]
                     | ei.attackedBy[Them, PieceType.BISHOP] | ei.attackedBy[Them, PieceType.ROOK];

                if (b)
                {
                    attackUnits += QueenContactCheck * Bitcount.popcount_Max15(b);
                }
            }

            // Analyse the enemy's safe distance checks for sliders and knights
            safe = ~(ei.attackedBy[Us, PieceType.ALL_PIECES] | pos.pieces(Them));

            b1 = pos.attacks_from(PieceType.ROOK, ksq) & safe;
            b2 = pos.attacks_from(PieceType.BISHOP, ksq) & safe;

            // Enemy queen safe checks
            b = (b1 | b2) & ei.attackedBy[Them, PieceType.QUEEN];
            if (b)
            {
                attackUnits += QueenCheck * Bitcount.popcount_Max15(b);
                score -= Checked;
            }

            // Enemy rooks safe checks
            b = b1 & ei.attackedBy[Them, PieceType.ROOK];
            if (b)
            {
                attackUnits += RookCheck * Bitcount.popcount_Max15(b);
                score -= Checked;
            }

            // Enemy bishops safe checks
            b = b2 & ei.attackedBy[Them, PieceType.BISHOP];
            if (b)
            {
                attackUnits += BishopCheck * Bitcount.popcount_Max15(b);
                score -= Checked;
            }

            // Enemy knights safe checks
            b = pos.attacks_from(PieceType.KNIGHT, ksq) & ei.attackedBy[Them, PieceType.KNIGHT] & safe;
            if (b)
            {
                attackUnits += KnightCheck * Bitcount.popcount_Max15(b);
                score -= Checked;
            }

            // Finally, extract the king danger score from the KingDanger[]
            // array and subtract the score from evaluation.
            score -= KingDanger[Math.Max(Math.Min(attackUnits, 399), 0)];
        }

        if (DoTrace)
        {
            add(PieceType.KING, Us, score);
        }

        return score;
    }

    // evaluate_threats() assigns bonuses according to the type of attacking piece
    // and the type of attacked one.

    private static Score evaluate_threats(Color Us, bool DoTrace, Position pos, EvalInfo ei)
    {
        var Them = (Us == Color.WHITE ? Color.BLACK : Color.WHITE);
        var Up = (Us == Color.WHITE ? Square.DELTA_N : Square.DELTA_S);
        var Left = (Us == Color.WHITE ? Square.DELTA_NW : Square.DELTA_SE);
        var Right = (Us == Color.WHITE ? Square.DELTA_NE : Square.DELTA_SW);
        var TRank2BB = (Us == Color.WHITE ? Bitboard.Rank2BB : Bitboard.Rank7BB);
        var TRank7BB = (Us == Color.WHITE ? Bitboard.Rank7BB : Bitboard.Rank2BB);

        const int Defended = 0;
        const int Weak = 1;
        const int Minor = 0;
        const int Rook = 1;

        Bitboard b, weak, defended, safeThreats;
        var score = Score.SCORE_ZERO;

        // Non-pawn enemies attacked by a pawn
        weak = (pos.pieces(Them) ^ pos.pieces(Them, PieceType.PAWN)) & ei.attackedBy[Us, PieceType.PAWN];

        if (weak)
        {
            b = pos.pieces(Us, PieceType.PAWN)
                & (~ei.attackedBy[Them, PieceType.ALL_PIECES] | ei.attackedBy[Us, PieceType.ALL_PIECES]);

            safeThreats = (Bitboard.shift_bb(Right, b) | Bitboard.shift_bb(Left, b)) & weak;

            if (weak ^ safeThreats)
            {
                score += ThreatenedByHangingPawn;
            }

            while (safeThreats)
            {
                score += ThreatenedByPawn[Piece.type_of(pos.piece_on(Utils.pop_lsb(ref safeThreats)))];
            }
        }

        // Non-pawn enemies defended by a pawn
        defended = (pos.pieces(Them) ^ pos.pieces(Them, PieceType.PAWN)) & ei.attackedBy[Them, PieceType.PAWN];

        // Add a bonus according to the kind of attacking pieces
        if (defended)
        {
            b = defended & (ei.attackedBy[Us, PieceType.KNIGHT] | ei.attackedBy[Us, PieceType.BISHOP]);
            while (b)
            {
                score += Threat[Defended][Minor][Piece.type_of(pos.piece_on(Utils.pop_lsb(ref b)))];
            }

            b = defended & ei.attackedBy[Us, PieceType.ROOK];
            while (b)
            {
                score += Threat[Defended][Rook][Piece.type_of(pos.piece_on(Utils.pop_lsb(ref b)))];
            }
        }

        // Enemies not defended by a pawn and under our attack
        weak = pos.pieces(Them) & ~ei.attackedBy[Them, PieceType.PAWN] & ei.attackedBy[Us, PieceType.ALL_PIECES];

        // Add a bonus according to the kind of attacking pieces
        if (weak)
        {
            b = weak & (ei.attackedBy[Us, PieceType.KNIGHT] | ei.attackedBy[Us, PieceType.BISHOP]);
            while (b)
            {
                score += Threat[Weak][Minor][Piece.type_of(pos.piece_on(Utils.pop_lsb(ref b)))];
            }

            b = weak & ei.attackedBy[Us, PieceType.ROOK];
            while (b)
            {
                score += Threat[Weak][Rook][Piece.type_of(pos.piece_on(Utils.pop_lsb(ref b)))];
            }

            b = weak & ~ei.attackedBy[Them, PieceType.ALL_PIECES];
            if (b)
            {
                score += Hanging * Bitcount.popcount_Max15(b);
            }

            b = weak & ei.attackedBy[Us, PieceType.KING];
            if (b)
            {
                score += Bitboard.more_than_one(b) ? KingOnMany : KingOnOne;
            }
        }

        // Bonus if some pawns can safely push and attack an enemy piece
        b = pos.pieces(Us, PieceType.PAWN) & ~TRank7BB;
        b = Bitboard.shift_bb(Up, b | (Bitboard.shift_bb(Up, b & TRank2BB) & ~pos.pieces()));

        b &= ~pos.pieces() & ~ei.attackedBy[Them, PieceType.PAWN]
             & (ei.attackedBy[Us, PieceType.ALL_PIECES] | ~ei.attackedBy[Them, PieceType.ALL_PIECES]);

        b = (Bitboard.shift_bb(Left, b) | Bitboard.shift_bb(Right, b)) & pos.pieces(Them)
            & ~ei.attackedBy[Us, PieceType.PAWN];

        if (b)
        {
            score += Bitcount.popcount_Max15(b) * PawnAttackThreat;
        }

        if (DoTrace)
        {
            add((int)Term.THREAT, Us, score);
        }

        return score;
    }

    // evaluate_passed_pawns() evaluates the passed pawns of the given color
    private static Score evaluate_passed_pawns(Color Us, bool DoTrace, Position pos, EvalInfo ei)
    {
        var Them = (Us == Color.WHITE ? Color.BLACK : Color.WHITE);

        Bitboard b, squaresToQueen, defendedSquares, unsafeSquares;
        var score = Score.SCORE_ZERO;

        b = ei.pi.passed_pawns(Us);

        while (b)
        {
            var s = Utils.pop_lsb(ref b);

            Debug.Assert(pos.pawn_passed(Us, s));

            int r = Rank.relative_rank(Us, s) - Rank.RANK_2;
            var rr = r * (r - 1);

            Value mbonus = Passed[(int)Phase.MG][r], ebonus = Passed[(int)Phase.EG][r];

            if (rr != 0)
            {
                var blockSq = s + Square.pawn_push(Us);

                // Adjust bonus based on the king's proximity
                ebonus += Utils.distance_Square(pos.square(PieceType.KING, Them), blockSq) * 5 * rr
                          - Utils.distance_Square(pos.square(PieceType.KING, Us), blockSq) * 2 * rr;

                // If blockSq is not the queening square then consider also a second push
                if (Rank.relative_rank(Us, blockSq) != Rank.RANK_8)
                {
                    ebonus -= Utils.distance_Square(pos.square(PieceType.KING, Us), blockSq + Square.pawn_push(Us)) * rr;
                }

                // If the pawn is free to advance, then increase the bonus
                if (pos.empty(blockSq))
                {
                    // If there is a rook or queen attacking/defending the pawn from behind,
                    // consider all the squaresToQueen. Otherwise consider only the squares
                    // in the pawn's path attacked or occupied by the enemy.
                    defendedSquares = unsafeSquares = squaresToQueen = Utils.forward_bb(Us, s);

                    var bb = Utils.forward_bb(Them, s) & pos.pieces(PieceType.ROOK, PieceType.QUEEN)
                             & pos.attacks_from(PieceType.ROOK, s);

                    if (!(pos.pieces(Us) & bb))
                    {
                        defendedSquares &= ei.attackedBy[Us, PieceType.ALL_PIECES];
                    }

                    if (!(pos.pieces(Them) & bb))
                    {
                        unsafeSquares &= ei.attackedBy[Them, PieceType.ALL_PIECES] | pos.pieces(Them);
                    }

                    // If there aren't any enemy attacks, assign a big bonus. Otherwise
                    // assign a smaller bonus if the block square isn't attacked.
                    var k = !unsafeSquares ? 18 : !(unsafeSquares & blockSq) ? 8 : 0;

                    // If the path to queen is fully defended, assign a big bonus.
                    // Otherwise assign a smaller bonus if the block square is defended.
                    if (defendedSquares == squaresToQueen)
                    {
                        k += 6;
                    }

                    else if (defendedSquares & blockSq)
                    {
                        k += 4;
                    }

                    mbonus += k * rr;
                    ebonus += k * rr;
                }
                else if (pos.pieces(Us) & blockSq)
                {
                    mbonus += rr * 3 + r * 2 + 3;
                    ebonus += rr + r * 2;
                }
            } // rr != 0

            if (pos.count(PieceType.PAWN, Us) < pos.count(PieceType.PAWN, Them))
            {
                ebonus += ebonus / 4;
            }

            score += Score.make_score(mbonus, ebonus) + PassedFile[Square.file_of(s)];
        }

        if (DoTrace)
        {
            add((int)Term.PASSED, Us, score * Weights[PassedPawns]);
        }

        // Add the scores to the middlegame and endgame eval
        return score * Weights[PassedPawns];
    }

    // evaluate_space() computes the space evaluation for a given side. The
    // space evaluation is a simple bonus based on the number of safe squares
    // available for minor pieces on the central four files on ranks 2--4. Safe
    // squares one, two or three squares behind a friendly pawn are counted
    // twice. Finally, the space bonus is multiplied by a weight. The aim is to
    // improve play on game opening.
    private static Score evaluate_space(Color Us, Position pos, EvalInfo ei)
    {
        var Them = (Us == Color.WHITE ? Color.BLACK : Color.WHITE);

        // Find the safe squares for our pieces inside the area defined by
        // SpaceMask[]. A square is unsafe if it is attacked by an enemy
        // pawn, or if it is undefended and attacked by an enemy piece.
        var safe = SpaceMask[Us] & ~pos.pieces(Us, PieceType.PAWN) & ~ei.attackedBy[Them, PieceType.PAWN]
                   & (ei.attackedBy[Us, PieceType.ALL_PIECES] | ~ei.attackedBy[Them, PieceType.ALL_PIECES]);

        // Find all squares which are at most three squares behind some friendly pawn
        var behind = pos.pieces(Us, PieceType.PAWN);
        behind |= (Us == Color.WHITE ? behind >> 8 : behind << 8);
        behind |= (Us == Color.WHITE ? behind >> 16 : behind << 16);

        // Since SpaceMask[Us] is fully on our half of the board...
        Debug.Assert((uint)(safe >> (Us == Color.WHITE ? 32 : 0)) == 0);

        // ...count safe + (behind & safe) with a single popcount
        var bonus = Bitcount.popcount_Full((Us == Color.WHITE ? safe << 32 : safe >> 32) | (behind & safe));
        var weight = pos.count(PieceType.KNIGHT, Us) + pos.count(PieceType.BISHOP, Us)
                     + pos.count(PieceType.KNIGHT, Them) + pos.count(PieceType.BISHOP, Them);

        return Score.make_score(bonus * weight * weight, 0);
    }

    /// evaluate() is the main evaluation function. It returns a static evaluation
    /// of the position always from the point of view of the side to move.
    public static Value evaluate(bool DoTrace, Position pos)
    {
        Debug.Assert(!pos.checkers());

        var ei = new EvalInfo();
        Score score;
        Score[] mobility = { Score.SCORE_ZERO, Score.SCORE_ZERO };

        // Initialize score by reading the incrementally updated scores included
        // in the position object (material + piece square tables).
        // Score is computed from the point of view of white.
        score = pos.psq_score();

        // Probe the material hash table
        var me = Material.probe(pos);
        score += me.imbalance();

        // If we have a specialized evaluation function for the current material
        // configuration, call it and return.
        if (me.specialized_eval_exists())
        {
            return me.evaluate(pos);
        }

        // Probe the pawn hash table
        ei.pi = Pawns.probe(pos);
        score += ei.pi.pawns_score() * Weights[PawnStructure];

        // Initialize attack and king safety bitboards
        ei.attackedBy[Color.WHITE, PieceType.ALL_PIECES] =
            ei.attackedBy[Color.BLACK, PieceType.ALL_PIECES] = new Bitboard(0);
        init_eval_info(Color.WHITE, pos, ei);
        init_eval_info(Color.BLACK, pos, ei);

        // Pawns blocked or on ranks 2 and 3. Will be excluded from the mobility area
        Bitboard[] blockedPawns =
            {
                pos.pieces(Color.WHITE, PieceType.PAWN)
                & (Bitboard.shift_bb(Square.DELTA_S, pos.pieces()) | Bitboard.Rank2BB
                   | Bitboard.Rank3BB),
                pos.pieces(Color.BLACK, PieceType.PAWN)
                & (Bitboard.shift_bb(Square.DELTA_N, pos.pieces()) | Bitboard.Rank7BB
                   | Bitboard.Rank6BB)
            };

        // Do not include in mobility squares protected by enemy pawns, or occupied
        // by our blocked pawns or king.
        Bitboard[] mobilityArea =
            {
                ~(ei.attackedBy[Color.BLACK, PieceType.PAWN] | blockedPawns[Color.WHITE]
                  | pos.square(PieceType.KING, Color.WHITE)),
                ~(ei.attackedBy[Color.WHITE, PieceType.PAWN] | blockedPawns[Color.BLACK]
                  | pos.square(PieceType.KING, Color.BLACK))
            };

        // Evaluate pieces and mobility
        score += evaluate_pieces(PieceType.KNIGHT, Color.WHITE, DoTrace, pos, ei, mobility, mobilityArea);
        score += (mobility[Color.WHITE] - mobility[Color.BLACK]) * Weights[Mobility];

        // Evaluate kings after all other pieces because we need complete attack
        // information when computing the king safety evaluation.
        score += evaluate_king(Color.WHITE, DoTrace, pos, ei) - evaluate_king(Color.BLACK, DoTrace, pos, ei);

        // Evaluate tactical threats, we need full attack information including king
        score += evaluate_threats(Color.WHITE, DoTrace, pos, ei) - evaluate_threats(Color.BLACK, DoTrace, pos, ei);

        // Evaluate passed pawns, we need full attack information including king
        score += evaluate_passed_pawns(Color.WHITE, DoTrace, pos, ei)
                 - evaluate_passed_pawns(Color.BLACK, DoTrace, pos, ei);

        // If both sides have only pawns, score for potential unstoppable pawns
        if (pos.non_pawn_material(Color.WHITE) == 0 && pos.non_pawn_material(Color.BLACK) == 0)
        {
            Bitboard b;
            if ((b = ei.pi.passed_pawns(Color.WHITE)) != 0)
            {
                score += (int)(Rank.relative_rank(Color.WHITE, Utils.frontmost_sq(Color.WHITE, b))) * Unstoppable;
            }

            if ((b = ei.pi.passed_pawns(Color.BLACK)) != 0)
            {
                score -= (int)(Rank.relative_rank(Color.BLACK, Utils.frontmost_sq(Color.BLACK, b))) * Unstoppable;
            }
        }

        // Evaluate space for both sides, only during opening
        if (pos.non_pawn_material(Color.WHITE) + pos.non_pawn_material(Color.BLACK) >= 12222)
        {
            score += (evaluate_space(Color.WHITE, pos, ei) - evaluate_space(Color.BLACK, pos, ei)) * Weights[Space];
        }

        // Scale winning side if position is more drawish than it appears
        var strongSide = Score.eg_value(score) > Value.VALUE_DRAW ? Color.WHITE : Color.BLACK;
        var sf = me.scale_factor(pos, strongSide);

        // If we don't already have an unusual scale factor, check for certain
        // types of endgames, and use a lower scale for those.
        if (me.game_phase() < Phase.PHASE_MIDGAME
            && (sf == ScaleFactor.SCALE_FACTOR_NORMAL || sf == ScaleFactor.SCALE_FACTOR_ONEPAWN))
        {
            if (pos.opposite_bishops())
            {
                // Endgame with opposite-colored bishops and no other pieces (ignoring pawns)
                // is almost a draw, in case of KBP vs KB is even more a draw.
                if (pos.non_pawn_material(Color.WHITE) == Value.BishopValueMg
                    && pos.non_pawn_material(Color.BLACK) == Value.BishopValueMg)
                {
                    sf = Bitboard.more_than_one(pos.pieces(PieceType.PAWN)) ? (ScaleFactor)(31) : (ScaleFactor)(9);
                }

                // Endgame with opposite-colored bishops, but also other pieces. Still
                // a bit drawish, but not as drawish as with only the two bishops.
                else
                {
                    sf = (ScaleFactor)(46 * (int)sf / (int)ScaleFactor.SCALE_FACTOR_NORMAL);
                }
            }
            // Endings where weaker side can place his king in front of the opponent's
            // pawns are drawish.
            else if (Math.Abs(Score.eg_value(score)) <= Value.BishopValueEg && ei.pi.pawn_span(strongSide) <= 1
                     && !pos.pawn_passed(~strongSide, pos.square(PieceType.KING, ~strongSide)))
            {
                sf = ei.pi.pawn_span(strongSide) != 0 ? (ScaleFactor)(51) : (ScaleFactor)(37);
            }
        }

        // Scale endgame by number of pawns
        var p = pos.count(PieceType.PAWN, Color.WHITE) + pos.count(PieceType.PAWN, Color.BLACK);
        var v_eg = 1 + Math.Abs(Score.eg_value(score));
        sf = (ScaleFactor)(Math.Max((int)sf / 2, (int)sf - 8 * (int)ScaleFactor.SCALE_FACTOR_NORMAL * (12 - p) / v_eg));

        // Interpolate between a middlegame and a (scaled by 'sf') endgame score
        var v = Score.mg_value(score) * (int)(me.game_phase())
                + Score.eg_value(score) * (Phase.PHASE_MIDGAME - me.game_phase()) * (int)sf
                / (int)ScaleFactor.SCALE_FACTOR_NORMAL;

        v /= (int)(Phase.PHASE_MIDGAME);

        // In case of tracing add all single evaluation terms
        if (DoTrace)
        {
            add((int)Term.MATERIAL, pos.psq_score());
            add((int)Term.IMBALANCE, me.imbalance());
            add(PieceType.PAWN, ei.pi.pawns_score());
            add(
                (int)Term.MOBILITY,
                mobility[Color.WHITE] * Weights[Mobility],
                mobility[Color.BLACK] * Weights[Mobility]);
            add(
                (int)Term.SPACE,
                evaluate_space(Color.WHITE, pos, ei) * Weights[Space],
                evaluate_space(Color.BLACK, pos, ei) * Weights[Space]);
            add((int)Term.TOTAL, score);
        }

        return (pos.side_to_move() == Color.WHITE ? v : -v) + Tempo; // Side to move point of view
    }

    /// init() computes evaluation weights, usually at startup
    public static void init()
    {
        const int MaxSlope = 8700;
        const int Peak = 1280000;
        var t = 0;

        for (var i = 0; i < 400; ++i)
        {
            t = Math.Min(Peak, Math.Min(i * i * 27, t + MaxSlope));
            KingDanger[i] = Score.make_score(t / 1000, 0) * Weights[KingSafety];
        }
    }

    private static double to_cp(Value v)
    {
        return (double)v / Value.PawnValueEg;
    }

    private static void add(int idx, Color c, Score s)
    {
        scores[idx, c, (int)Phase.MG] = to_cp(Score.mg_value(s));
        scores[idx, c, (int)Phase.EG] = to_cp(Score.eg_value(s));
    }

    private static void add(int idx, Score w, Score b)
    {
        add(idx, Color.WHITE, w);
        add(idx, Color.BLACK, b);
    }

    private static void add(int idx, Score w)
    {
        add(idx, Color.WHITE, w);
        add(idx, Color.BLACK, Score.SCORE_ZERO);
    }

    private static string termString(Term t)
    {
        var os = new StringBuilder();
        if (t == Term.MATERIAL || t == Term.IMBALANCE || t == (Term)((int)PieceType.PAWN) || t == Term.TOTAL)
        {
            os.Append("  ---   --- |   ---   --- | ");
        }
        else
        {
            os.Append($"{scores[(int)t, Color.WHITE, (int)Phase.MG],5:N2} ");
            os.Append($"{scores[(int)t, Color.WHITE, (int)Phase.EG],5:N2} | ");
            os.Append($"{scores[(int)t, Color.BLACK, (int)Phase.MG],5:N2} ");
            os.Append($"{scores[(int)t, Color.BLACK, (int)Phase.EG],5:N2} | ");
        }

        os.Append($"{scores[(int)t, Color.WHITE, (int)Phase.MG] - scores[(int)t, Color.BLACK, (int)Phase.MG],5:N2} ");
        os.Append($"{scores[(int)t, Color.WHITE, (int)Phase.EG] - scores[(int)t, Color.BLACK, (int)Phase.EG],5:N2} ");

        return os.ToString();
    }

    /// trace() is like evaluate(), but instead of returning a value, it returns
    /// a string (suitable for outputting to stdout) that contains the detailed
    /// descriptions and values of each evaluation term. Useful for debugging.
    public static string trace(Position pos)
    {
        scores = new double[(int)Term.TERM_NB, Color.COLOR_NB, (int)Phase.PHASE_NB];

        var v = evaluate(true, pos);
        v = pos.side_to_move() == Color.WHITE ? v : -v; // White's point of view

        var ss = new StringBuilder();

        ss.AppendLine("      Eval term |    White    |    Black    |    Total    ");
        ss.AppendLine("                |   MG    EG  |   MG    EG  |   MG    EG  ");
        ss.AppendLine("----------------+-------------+-------------+-------------");
        ss.AppendLine($"       Material | {termString(Term.MATERIAL)}");
        ss.AppendLine($"      Imbalance | {termString(Term.IMBALANCE)}");
        ss.AppendLine($"          Pawns | {termString((Term)(int)PieceType.PAWN)}");
        ss.AppendLine($"        Knights | {termString((Term)(int)PieceType.KNIGHT)}");
        ss.AppendLine($"         Bishop | {termString((Term)(int)PieceType.BISHOP)}");
        ss.AppendLine($"          Rooks | {termString((Term)(int)PieceType.ROOK)}");
        ss.AppendLine($"         Queens | {termString((Term)(int)PieceType.QUEEN)}");
        ss.AppendLine($"       Mobility | {termString(Term.MOBILITY)}");
        ss.AppendLine($"    King safety | {termString((Term)(int)PieceType.KING)}");
        ss.AppendLine($"        Threats | {termString(Term.THREAT)}");
        ss.AppendLine($"   Passed pawns | {termString(Term.PASSED)}");
        ss.AppendLine($"          Space | {termString(Term.SPACE)}");
        ss.AppendLine("----------------+-------------+-------------+-------------");
        ss.AppendLine($"          Total | {termString(Term.TOTAL)}");

        ss.AppendLine();
        ss.AppendLine($"Total Evaluation: {to_cp(v),5:N2} (white side)");

        return ss.ToString();
    }

    public struct Weight
    {
        public Weight(int mg, int eg)
        {
            this.mg = mg;
            this.eg = eg;
        }

        public int mg;

        public int eg;
    }

    private enum Term
    {
        // First 8 entries are for PieceType
        MATERIAL = 8,

        IMBALANCE,

        MOBILITY,

        THREAT,

        PASSED,

        SPACE,

        TOTAL,

        TERM_NB
    };
}