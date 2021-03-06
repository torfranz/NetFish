﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

#if PRIMITIVE
using ColorT = System.Int32;
using PieceTypeT = System.Int32;
using ValueT = System.Int32;
using ScoreT = System.Int32;
using BitboardT = System.UInt64;
#endif
internal static class Eval
{
    internal static ValueT Tempo = Value.Create(17); // Must be visible to search

    // Evaluation weights, indexed by the corresponding evaluation term
    private static readonly int Mobility = 0;

    private static readonly int PawnStructure = 1;

    private static readonly int PassedPawns = 2;

    private static readonly int Space = 3;

    private static readonly int KingSafety = 4;

    internal static Weight[] Weights =
    {
        new Weight(289, 344), new Weight(233, 201), new Weight(221, 273),
        new Weight(46, 0), new Weight(322, 0)
    };

    // MobilityBonus[PieceType][attacked] contains bonuses for middle and end
    // game, indexed by piece type and number of attacked squares not occupied by
    // friendly pieces.
    internal static ScoreT[][] MobilityBonus =
    {
        new ScoreT[] {}, new ScoreT[] {},
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
    internal static ScoreT[][] Outpost =
    {
        new[] {Score.make_score(42, 11), Score.make_score(63, 17)}, // Knights
        new[] {Score.make_score(18, 5), Score.make_score(27, 8)} // Bishops
    };

    // Threat[defended/weak][minor/rook attacking][attacked PieceType] contains
    // bonuses according to which piece type attacks which one.
    internal static ScoreT[][][] Threat =
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
    internal static ScoreT[] ThreatenedByPawn =
    {
        Score.make_score(0, 0), Score.make_score(0, 0),
        Score.make_score(107, 138), Score.make_score(84, 122),
        Score.make_score(114, 203), Score.make_score(121, 217)
    };

    // Passed[mg/eg][rank] contains midgame and endgame bonuses for passed pawns.
    // We don't use a Score because we process the two components independently.
    internal static ValueT[][] Passed =
    {
        new[]
        {
            Value.Create(0), Value.Create(1), Value.Create(34), Value.Create(90), Value.Create(214),
            Value.Create(328)
        },
        new[]
        {
            Value.Create(7), Value.Create(14), Value.Create(37), Value.Create(63), Value.Create(134),
            Value.Create(189)
        }
    };

    // PassedFile[File] contains a bonus according to the file of a passed pawn.
    internal static ScoreT[] PassedFile =
    {
        Score.make_score(14, 13), Score.make_score(2, 5), Score.make_score(-3, -4),
        Score.make_score(-19, -14), Score.make_score(-19, -14),
        Score.make_score(-3, -4), Score.make_score(2, 5), Score.make_score(14, 13)
    };

    internal static ScoreT ThreatenedByHangingPawn = Score.make_score(40, 60);

    // Assorted bonuses and penalties used by evaluation
    internal static ScoreT KingOnOne = Score.make_score(2, 58);

    internal static ScoreT KingOnMany = Score.make_score(6, 125);

    internal static ScoreT RookOnPawn = Score.make_score(7, 27);

    internal static ScoreT RookOnOpenFile = Score.make_score(43, 21);

    internal static ScoreT RookOnSemiOpenFile = Score.make_score(19, 10);

    internal static ScoreT BishopPawns = Score.make_score(8, 12);

    internal static ScoreT MinorBehindPawn = Score.make_score(16, 0);

    internal static ScoreT TrappedRook = Score.make_score(92, 0);

    internal static ScoreT Unstoppable = Score.make_score(0, 20);

    internal static ScoreT Hanging = Score.make_score(31, 26);

    internal static ScoreT PawnAttackThreat = Score.make_score(20, 20);

    internal static ScoreT Checked = Score.make_score(20, 20);

    // Penalty for a bishop on a1/h1 (a8/h8 for black) which is trapped by
    // a friendly pawn on b2/g2 (b7/g7 for black). This can obviously only
    // happen in Chess960 games.
    internal static ScoreT TrappedBishopA1H1 = Score.make_score(50, 50);

    // SpaceMask[Color] contains the area of the board which is considered
    // by the space evaluation. In the middlegame, each side is given a bonus
    // based on how many squares inside this area are safe and available for
    // friendly minor pieces.
    internal static BitboardT[] SpaceMask =
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
    internal static ScoreT[] KingDanger = new ScoreT[512];

    // KingAttackWeights[PieceType] contains king attack weights by piece type
    internal static int[] KingAttackWeights = {0, 0, 7, 5, 4, 1};

    // Penalties for enemy's safe checks
    internal static int QueenContactCheck = 89;

    internal static int QueenCheck = 50;

    internal static int RookCheck = 45;

    internal static int BishopCheck = 6;

    internal static int KnightCheck = 14;

    private static double[,,] scores = new double[(int) Term.TERM_NB, Color.COLOR_NB, (int) Phase.PHASE_NB];

    // init_eval_info() initializes king bitboards for given color adding
    // pawn attacks. To be done at the beginning of the evaluation.

    private static void init_eval_info(ColorT Us, Position pos, EvalInfo ei)
    {
        var Them = (Us == Color.WHITE ? Color.BLACK : Color.WHITE);
        var Down = (Us == Color.WHITE ? Square.DELTA_S : Square.DELTA_N);

        ei.pinnedPieces[Us] = pos.pinned_pieces(Us);
        var b = ei.attackedBy[Them, PieceType.KING] = pos.attacks_from_PtS(PieceType.KING, pos.square(PieceType.KING, Them));
        ei.attackedBy[Them, PieceType.ALL_PIECES] |= b;
        ei.attackedBy[Us, PieceType.ALL_PIECES] |= ei.attackedBy[Us, PieceType.PAWN] = ei.pi.pawn_attacks(Us);

        // Init king safety tables only if we are going to use them
        if (pos.non_pawn_material(Us) >= Value.QueenValueMg)
        {
            ei.kingRing[Them] = b | Bitboard.shift_bb(Down, b);
            b &= ei.attackedBy[Us, PieceType.PAWN];
            ei.kingAttackersCount[Us] = b!=0 ? Bitcount.popcount_Max15(b) : 0;
            ei.kingAdjacentZoneAttacksCount[Us] = ei.kingAttackersWeight[Us] = 0;
        }
        else
        {
            ei.kingRing[Them] = Bitboard.Create(0);
            ei.kingAttackersCount[Us] = 0;
        }
    }

    // evaluate_pieces() assigns bonuses and penalties to the pieces of a given color

    private static ScoreT evaluate_pieces(
        PieceTypeT pieceType,
        ColorT Us,
        bool DoTrace,
        Position pos,
        EvalInfo ei,
        ScoreT[] mobility,
        BitboardT[] mobilityArea)
    {
        int Pt = pieceType;
        if (Pt == PieceType.KING)
        {
            return Score.SCORE_ZERO;
        }
        var score = Score.SCORE_ZERO;

        var NextPt = (Us == Color.WHITE ? pieceType : pieceType + 1);
        var Them = (Us == Color.WHITE ? Color.BLACK : Color.WHITE);
        
        ei.attackedBy[Us, Pt] = Bitboard.Create(0);

        for(var idx=0; idx<16;idx++)
        {
            var s = pos.square(pieceType, Us, idx);
            if (s == Square.SQ_NONE)
            {
                break;
            }
            // Find attacked squares, including x-ray attacks for bishops and rooks
            var b = Pt == PieceType.BISHOP
                ? Utils.attacks_bb_PtSBb(PieceType.BISHOP, s, pos.pieces() ^ pos.pieces_CtPt(Us, PieceType.QUEEN))
                : Pt == PieceType.ROOK
                    ? Utils.attacks_bb_PtSBb(
                        PieceType.ROOK,
                        s,
                        pos.pieces() ^ pos.pieces_CtPtPt(Us, PieceType.ROOK, PieceType.QUEEN))
                    : pos.attacks_from_PtS(pieceType, s);

            if (Bitboard.AndWithSquare(ei.pinnedPieces[Us], s)!=0)
            {
                b &= Utils.LineBB[pos.square(PieceType.KING, Us), s];
            }

            ei.attackedBy[Us, PieceType.ALL_PIECES] |= ei.attackedBy[Us, Pt] |= b;

            if ((b & ei.kingRing[Them])!=0)
            {
                ei.kingAttackersCount[Us]++;
                ei.kingAttackersWeight[Us] += KingAttackWeights[Pt];
                var bb = b & ei.attackedBy[Them, PieceType.KING];
                if (bb!=0)
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
                if (Rank.relative_rank_CtSt(Us, s) >= Rank.RANK_4 && Rank.relative_rank_CtSt(Us, s) <= Rank.RANK_6
                    && (pos.pieces_CtPt(Them, PieceType.PAWN) & Utils.pawn_attack_span(Us, s))==0)
                {
                    score +=
                        Outpost[Pt == PieceType.BISHOP ? 1 : 0][Bitboard.AndWithSquare(ei.attackedBy[Us, PieceType.PAWN], s)!=0 ? 1 : 0];
                }

                // Bonus when behind a pawn
                if (Rank.relative_rank_CtSt(Us, s) < Rank.RANK_5 && Bitboard.AndWithSquare(pos.pieces_Pt(PieceType.PAWN), (s + Square.pawn_push(Us)))!=0)
                {
                    score += MinorBehindPawn;
                }

                // Penalty for pawns on same color square of bishop
                if (Pt == PieceType.BISHOP)
                {
                    score -= BishopPawns*ei.pi.pawns_on_same_color_squares(Us, s);
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
                            ? TrappedBishopA1H1*4
                            : pos.piece_on(s + d + d) == Piece.make_piece(Us, PieceType.PAWN)
                                ? TrappedBishopA1H1*2
                                : TrappedBishopA1H1;
                    }
                }
            }

            if (Pt == PieceType.ROOK)
            {
                // Bonus for aligning with enemy pawns on the same rank/file
                if (Rank.relative_rank_CtSt(Us, s) >= Rank.RANK_5)
                {
                    var alignedPawns = pos.pieces_CtPt(Them, PieceType.PAWN) & Utils.PseudoAttacks[PieceType.ROOK, s];
                    if (alignedPawns!=0)
                    {
                        score += Bitcount.popcount_Max15(alignedPawns)*RookOnPawn;
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
                        && (Square.rank_of(ksq) == Square.rank_of(s) || Rank.relative_rank_CtSt(Us, ksq) == Rank.RANK_1)
                        && 0 == ei.pi.semiopen_side(Us, Square.file_of(ksq), Square.file_of(s) < Square.file_of(ksq)))
                    {
                        score -= (TrappedRook - Score.make_score(mob*22, 0))*(1 + (pos.can_castle(Us) == 0 ? 1 : 0));
                    }
                }
            }
        }

        if (DoTrace)
        {
            add_IdxCtSt(Pt, Us, score);
        }
        // Recursively call evaluate_pieces() of next piece type until KING excluded
        return score - evaluate_pieces(NextPt, Them, DoTrace, pos, ei, mobility, mobilityArea);
    }

    // evaluate_king() assigns bonuses and penalties to a king of a given color

    private static ScoreT evaluate_king(ColorT Us, bool DoTrace, Position pos, EvalInfo ei)
    {
        var Them = (Us == Color.WHITE ? Color.BLACK : Color.WHITE);

        var ksq = pos.square(PieceType.KING, Us);

        // King shelter and enemy pawns storm
        var score = ei.pi.king_safety(Us, pos, ksq);

        // Main king safety evaluation
        if (ei.kingAttackersCount[Them] != 0)
        {
            // Find the attacked squares around the king which have no defenders
            // apart from the king itself
            var undefended = ei.attackedBy[Them, PieceType.ALL_PIECES] & ei.attackedBy[Us, PieceType.KING]
                                  & ~(ei.attackedBy[Us, PieceType.PAWN] | ei.attackedBy[Us, PieceType.KNIGHT]
                                      | ei.attackedBy[Us, PieceType.BISHOP] | ei.attackedBy[Us, PieceType.ROOK]
                                      | ei.attackedBy[Us, PieceType.QUEEN]);

            // Initialize the 'attackUnits' variable, which is used later on as an
            // index into the KingDanger[] array. The initial value is based on the
            // number and types of the enemy's attacking pieces, the number of
            // attacked and undefended squares around our king and the quality of
            // the pawn shelter (current 'score' value).
            var attackUnits = Math.Min(72, ei.kingAttackersCount[Them] *ei.kingAttackersWeight[Them])
                              + 9*ei.kingAdjacentZoneAttacksCount[Them] + 27*Bitcount.popcount_Max15(undefended)
                              + 11*((ulong)ei.pinnedPieces[Us] != 0 ? 1 : 0)
                              - 64*(pos.count(PieceType.QUEEN, Them) == 0 ? 1 : 0) - Score.mg_value(score)/8;

            // Analyse the enemy's safe queen contact checks. Firstly, find the
            // undefended squares around the king reachable by the enemy queen...
            var b = undefended & ei.attackedBy[Them, PieceType.QUEEN] & ~pos.pieces_Ct(Them);
            if (b!=0)
            {
                // ...and then remove squares not supported by another enemy piece
                b &= ei.attackedBy[Them, PieceType.PAWN] | ei.attackedBy[Them, PieceType.KNIGHT]
                     | ei.attackedBy[Them, PieceType.BISHOP] | ei.attackedBy[Them, PieceType.ROOK];

                if (b!=0)
                {
                    attackUnits += QueenContactCheck*Bitcount.popcount_Max15(b);
                }
            }

            // Analyse the enemy's safe distance checks for sliders and knights
            var safe = ~(ei.attackedBy[Us, PieceType.ALL_PIECES] | pos.pieces_Ct(Them));

            var b1 = pos.attacks_from_PtS(PieceType.ROOK, ksq) & safe;
            var b2 = pos.attacks_from_PtS(PieceType.BISHOP, ksq) & safe;

            // Enemy queen safe checks
            b = (b1 | b2) & ei.attackedBy[Them, PieceType.QUEEN];
            if (b!=0)
            {
                attackUnits += QueenCheck*Bitcount.popcount_Max15(b);
                score -= Checked;
            }

            // Enemy rooks safe checks
            b = b1 & ei.attackedBy[Them, PieceType.ROOK];
            if (b!=0)
            {
                attackUnits += RookCheck*Bitcount.popcount_Max15(b);
                score -= Checked;
            }

            // Enemy bishops safe checks
            b = b2 & ei.attackedBy[Them, PieceType.BISHOP];
            if (b!=0)
            {
                attackUnits += BishopCheck*Bitcount.popcount_Max15(b);
                score -= Checked;
            }

            // Enemy knights safe checks
            b = pos.attacks_from_PtS(PieceType.KNIGHT, ksq) & ei.attackedBy[Them, PieceType.KNIGHT] & safe;
            if (b!=0)
            {
                attackUnits += KnightCheck*Bitcount.popcount_Max15(b);
                score -= Checked;
            }

            // Finally, extract the king danger score from the KingDanger[]
            // array and subtract the score from evaluation.
            score -= KingDanger[Math.Max(Math.Min(attackUnits, 399), 0)];
        }

        if (DoTrace)
        {
            add_IdxCtSt(PieceType.KING, Us, score);
        }

        return score;
    }

    // evaluate_threats() assigns bonuses according to the type of attacking piece
    // and the type of attacked one.

    private static ScoreT evaluate_threats(ColorT Us, bool DoTrace, Position pos, EvalInfo ei)
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

        BitboardT b;
        var score = Score.SCORE_ZERO;

        // Non-pawn enemies attacked by a pawn
        var weak = (pos.pieces_Ct(Them) ^ pos.pieces_CtPt(Them, PieceType.PAWN)) & ei.attackedBy[Us, PieceType.PAWN];

        if (weak!=0)
        {
            b = pos.pieces_CtPt(Us, PieceType.PAWN)
                & (~ei.attackedBy[Them, PieceType.ALL_PIECES] | ei.attackedBy[Us, PieceType.ALL_PIECES]);

            var safeThreats = (Bitboard.shift_bb(Right, b) | Bitboard.shift_bb(Left, b)) & weak;

            if ((weak ^ safeThreats)!=0)
            {
                score += ThreatenedByHangingPawn;
            }

            while (safeThreats!=0)
            {
                score += ThreatenedByPawn[Piece.type_of(pos.piece_on(Utils.pop_lsb(ref safeThreats)))];
            }
        }

        // Non-pawn enemies defended by a pawn
        var defended = (pos.pieces_Ct(Them) ^ pos.pieces_CtPt(Them, PieceType.PAWN)) & ei.attackedBy[Them, PieceType.PAWN];

        // Add a bonus according to the kind of attacking pieces
        if (defended!=0)
        {
            b = defended & (ei.attackedBy[Us, PieceType.KNIGHT] | ei.attackedBy[Us, PieceType.BISHOP]);
            while (b!=0)
            {
                score += Threat[Defended][Minor][Piece.type_of(pos.piece_on(Utils.pop_lsb(ref b)))];
            }

            b = defended & ei.attackedBy[Us, PieceType.ROOK];
            while (b!=0)
            {
                score += Threat[Defended][Rook][Piece.type_of(pos.piece_on(Utils.pop_lsb(ref b)))];
            }
        }

        // Enemies not defended by a pawn and under our attack
        weak = pos.pieces_Ct(Them) & ~ei.attackedBy[Them, PieceType.PAWN] & ei.attackedBy[Us, PieceType.ALL_PIECES];

        // Add a bonus according to the kind of attacking pieces
        if (weak!=0)
        {
            b = weak & (ei.attackedBy[Us, PieceType.KNIGHT] | ei.attackedBy[Us, PieceType.BISHOP]);
            while (b!=0)
            {
                score += Threat[Weak][Minor][Piece.type_of(pos.piece_on(Utils.pop_lsb(ref b)))];
            }

            b = weak & ei.attackedBy[Us, PieceType.ROOK];
            while (b!=0)
            {
                score += Threat[Weak][Rook][Piece.type_of(pos.piece_on(Utils.pop_lsb(ref b)))];
            }

            b = weak & ~ei.attackedBy[Them, PieceType.ALL_PIECES];
            if (b!=0)
            {
                score += Hanging*Bitcount.popcount_Max15(b);
            }

            b = weak & ei.attackedBy[Us, PieceType.KING];
            if (b!=0)
            {
                score += Bitboard.more_than_one(b) ? KingOnMany : KingOnOne;
            }
        }

        // Bonus if some pawns can safely push and attack an enemy piece
        b = pos.pieces_CtPt(Us, PieceType.PAWN) & ~TRank7BB;
        b = Bitboard.shift_bb(Up, b | (Bitboard.shift_bb(Up, b & TRank2BB) & ~pos.pieces()));

        b &= ~pos.pieces() & ~ei.attackedBy[Them, PieceType.PAWN]
             & (ei.attackedBy[Us, PieceType.ALL_PIECES] | ~ei.attackedBy[Them, PieceType.ALL_PIECES]);

        b = (Bitboard.shift_bb(Left, b) | Bitboard.shift_bb(Right, b)) & pos.pieces_Ct(Them)
            & ~ei.attackedBy[Us, PieceType.PAWN];

        if (b!=0)
        {
            score += Bitcount.popcount_Max15(b)*PawnAttackThreat;
        }

        if (DoTrace)
        {
            add_IdxCtSt((int) Term.THREAT, Us, score);
        }

        return score;
    }

    // evaluate_passed_pawns() evaluates the passed pawns of the given color
    private static ScoreT evaluate_passed_pawns(ColorT Us, bool DoTrace, Position pos, EvalInfo ei)
    {
        var Them = (Us == Color.WHITE ? Color.BLACK : Color.WHITE);

        var score = Score.SCORE_ZERO;

        var b = ei.pi.passed_pawns(Us);

        while (b!=0)
        {
            var s = Utils.pop_lsb(ref b);

            Debug.Assert(pos.pawn_passed(Us, s));

            int r = Rank.relative_rank_CtSt(Us, s) - Rank.RANK_2;
            var rr = r*(r - 1);

            ValueT mbonus = Passed[(int) Phase.MG][r], ebonus = Passed[(int) Phase.EG][r];

            if (rr != 0)
            {
                var blockSq = s + Square.pawn_push(Us);

                // Adjust bonus based on the king's proximity
                ebonus += Utils.distance_Square(pos.square(PieceType.KING, Them), blockSq)*5*rr
                          - Utils.distance_Square(pos.square(PieceType.KING, Us), blockSq)*2*rr;

                // If blockSq is not the queening square then consider also a second push
                if (Rank.relative_rank_CtSt(Us, blockSq) != Rank.RANK_8)
                {
                    ebonus -= Utils.distance_Square(pos.square(PieceType.KING, Us), blockSq + Square.pawn_push(Us))*rr;
                }

                // If the pawn is free to advance, then increase the bonus
                if (pos.empty(blockSq))
                {
                    // If there is a rook or queen attacking/defending the pawn from behind,
                    // consider all the squaresToQueen. Otherwise consider only the squares
                    // in the pawn's path attacked or occupied by the enemy.
                    BitboardT squaresToQueen;
                    BitboardT unsafeSquares;
                    var defendedSquares = unsafeSquares = squaresToQueen = Utils.forward_bb(Us, s);

                    var bb = Utils.forward_bb(Them, s) & pos.pieces_PtPt(PieceType.ROOK, PieceType.QUEEN)
                             & pos.attacks_from_PtS(PieceType.ROOK, s);

                    if ((pos.pieces_Ct(Us) & bb)==0)
                    {
                        defendedSquares &= ei.attackedBy[Us, PieceType.ALL_PIECES];
                    }

                    if ((pos.pieces_Ct(Them) & bb) == 0)
                    {
                        unsafeSquares &= ei.attackedBy[Them, PieceType.ALL_PIECES] | pos.pieces_Ct(Them);
                    }

                    // If there aren't any enemy attacks, assign a big bonus. Otherwise
                    // assign a smaller bonus if the block square isn't attacked.
                    var k = unsafeSquares == 0 ? 18 : Bitboard.AndWithSquare(unsafeSquares, blockSq)==0 ? 8 : 0;

                    // If the path to queen is fully defended, assign a big bonus.
                    // Otherwise assign a smaller bonus if the block square is defended.
                    if (defendedSquares == squaresToQueen)
                    {
                        k += 6;
                    }

                    else if (Bitboard.AndWithSquare(defendedSquares, blockSq)!=0)
                    {
                        k += 4;
                    }

                    mbonus += k*rr;
                    ebonus += k*rr;
                }
                else if (Bitboard.AndWithSquare(pos.pieces_Ct(Us), blockSq)!=0)
                {
                    mbonus += rr*3 + r*2 + 3;
                    ebonus += rr + r*2;
                }
            } // rr != 0

            if (pos.count(PieceType.PAWN, Us) < pos.count(PieceType.PAWN, Them))
            {
                ebonus += ebonus/4;
            }

            score += Score.make_score(mbonus, ebonus) + PassedFile[Square.file_of(s)];
        }

        if (DoTrace)
        {
            add_IdxCtSt((int) Term.PASSED, Us, Score.Multiply(score, Weights[PassedPawns]));
        }

        // Add the scores to the middlegame and endgame eval
        return Score.Multiply(score, Weights[PassedPawns]);
    }

    // evaluate_space() computes the space evaluation for a given side. The
    // space evaluation is a simple bonus based on the number of safe squares
    // available for minor pieces on the central four files on ranks 2--4. Safe
    // squares one, two or three squares behind a friendly pawn are counted
    // twice. Finally, the space bonus is multiplied by a weight. The aim is to
    // improve play on game opening.
    private static ScoreT evaluate_space(ColorT Us, Position pos, EvalInfo ei)
    {
        var Them = (Us == Color.WHITE ? Color.BLACK : Color.WHITE);

        // Find the safe squares for our pieces inside the area defined by
        // SpaceMask[]. A square is unsafe if it is attacked by an enemy
        // pawn, or if it is undefended and attacked by an enemy piece.
        var safe = SpaceMask[Us] & ~pos.pieces_CtPt(Us, PieceType.PAWN) & ~ei.attackedBy[Them, PieceType.PAWN]
                   & (ei.attackedBy[Us, PieceType.ALL_PIECES] | ~ei.attackedBy[Them, PieceType.ALL_PIECES]);

        // Find all squares which are at most three squares behind some friendly pawn
        var behind = pos.pieces_CtPt(Us, PieceType.PAWN);
        behind |= (Us == Color.WHITE ? behind >> 8 : behind << 8);
        behind |= (Us == Color.WHITE ? behind >> 16 : behind << 16);

        // Since SpaceMask[Us.Value] is fully on our half of the board...
        Debug.Assert((uint) (safe >> (Us == Color.WHITE ? 32 : 0)) == 0);

        // ...count safe + (behind & safe) with a single popcount
        var bonus = Bitcount.popcount_Full((Us == Color.WHITE ? safe << 32 : safe >> 32) | (behind & safe));
        var weight = pos.count(PieceType.KNIGHT, Us) + pos.count(PieceType.BISHOP, Us)
                     + pos.count(PieceType.KNIGHT, Them) + pos.count(PieceType.BISHOP, Them);

        return Score.make_score(bonus*weight*weight, 0);
    }

    /// evaluate() is the main evaluation function. It returns a static evaluation
    /// of the position always from the point of view of the side to move.
    internal static ValueT evaluate(bool DoTrace, Position pos)
    {
        Debug.Assert(pos.checkers() == 0);

        var ei = new EvalInfo();
        ScoreT[] mobility = {Score.SCORE_ZERO, Score.SCORE_ZERO};

        // Initialize score by reading the incrementally updated scores included
        // in the position object (material + piece square tables).
        // Score is computed from the point of view of white.
        var score = pos.psq_score();

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
        score += Score.Multiply(ei.pi.pawns_score(), Weights[PawnStructure]);

        // Initialize attack and king safety bitboards
        ei.attackedBy[Color.WHITE, PieceType.ALL_PIECES] =
            ei.attackedBy[Color.BLACK, PieceType.ALL_PIECES] = Bitboard.Create(0);
        init_eval_info(Color.WHITE, pos, ei);
        init_eval_info(Color.BLACK, pos, ei);

        // Pawns blocked or on ranks 2 and 3. Will be excluded from the mobility area
        BitboardT[] blockedPawns =
        {
            pos.pieces_CtPt(Color.WHITE, PieceType.PAWN)
            & (Bitboard.shift_bb(Square.DELTA_S, pos.pieces()) | Bitboard.Rank2BB
               | Bitboard.Rank3BB),
            pos.pieces_CtPt(Color.BLACK, PieceType.PAWN)
            & (Bitboard.shift_bb(Square.DELTA_N, pos.pieces()) | Bitboard.Rank7BB
               | Bitboard.Rank6BB)
        };

        // Do not include in mobility squares protected by enemy pawns, or occupied
        // by our blocked pawns or king.
        BitboardT[] mobilityArea =
        {
            ~(Bitboard.OrWithSquare(ei.attackedBy[Color.BLACK, PieceType.PAWN] | blockedPawns[Color.WHITE]
              , pos.square(PieceType.KING, Color.WHITE))),
            ~(Bitboard.OrWithSquare(ei.attackedBy[Color.WHITE, PieceType.PAWN] | blockedPawns[Color.BLACK]
              , pos.square(PieceType.KING, Color.BLACK)))
        };

        // Evaluate pieces and mobility
        score += evaluate_pieces(PieceType.KNIGHT, Color.WHITE, DoTrace, pos, ei, mobility, mobilityArea);
        score += Score.Multiply(mobility[Color.WHITE] - mobility[Color.BLACK], Weights[Mobility]);

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
            BitboardT b;
            if ((b = ei.pi.passed_pawns(Color.WHITE)) != 0)
            {
                score += Rank.relative_rank_CtSt(Color.WHITE, Utils.frontmost_sq(Color.WHITE, b)) * Unstoppable;
            }

            if ((b = ei.pi.passed_pawns(Color.BLACK)) != 0)
            {
                score -= Rank.relative_rank_CtSt(Color.BLACK, Utils.frontmost_sq(Color.BLACK, b)) * Unstoppable;
            }
        }

        // Evaluate space for both sides, only during opening
        if (pos.non_pawn_material(Color.WHITE) + pos.non_pawn_material(Color.BLACK) >= 12222)
        {
            score += Score.Multiply(evaluate_space(Color.WHITE, pos, ei) - evaluate_space(Color.BLACK, pos, ei), Weights[Space]);
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
                    sf = Bitboard.more_than_one(pos.pieces_Pt(PieceType.PAWN)) ? (ScaleFactor) (31) : (ScaleFactor) (9);
                }

                // Endgame with opposite-colored bishops, but also other pieces. Still
                // a bit drawish, but not as drawish as with only the two bishops.
                else
                {
                    sf = (ScaleFactor) (46*(int) sf/(int) ScaleFactor.SCALE_FACTOR_NORMAL);
                }
            }
            // Endings where weaker side can place his king in front of the opponent's
            // pawns are drawish.
            else if (Math.Abs(Score.eg_value(score)) <= Value.BishopValueEg && ei.pi.pawn_span(strongSide) <= 1
                     && !pos.pawn_passed(Color.opposite(strongSide), pos.square(PieceType.KING, Color.opposite(strongSide))))
            {
                sf = ei.pi.pawn_span(strongSide) != 0 ? (ScaleFactor) (51) : (ScaleFactor) (37);
            }
        }

        // Scale endgame by number of pawns
        var p = pos.count(PieceType.PAWN, Color.WHITE) + pos.count(PieceType.PAWN, Color.BLACK);
        var vEg = 1 + Math.Abs(Score.eg_value(score));
        sf = (ScaleFactor) (Math.Max((int) sf/2, (int) sf - 8*(int) ScaleFactor.SCALE_FACTOR_NORMAL*(12 - p)/vEg));

        // Interpolate between a middlegame and a (scaled by 'sf') endgame score
        var v = Score.mg_value(score)*(int) (me.game_phase())
                + Score.eg_value(score)*(Phase.PHASE_MIDGAME - me.game_phase())*(int) sf
                /(int) ScaleFactor.SCALE_FACTOR_NORMAL;

        v /= (int) (Phase.PHASE_MIDGAME);

        // In case of tracing add all single evaluation terms
        if (DoTrace)
        {
            add_IdxSt((int) Term.MATERIAL, pos.psq_score());
            add_IdxSt((int) Term.IMBALANCE, me.imbalance());
            add_IdxSt(PieceType.PAWN, ei.pi.pawns_score());
            add_IdxStSt(
                (int) Term.MOBILITY,
                Score.Multiply(mobility[Color.WHITE], Weights[Mobility]),
                Score.Multiply(mobility[Color.BLACK], Weights[Mobility]));
            add_IdxStSt(
                (int) Term.SPACE,
                Score.Multiply(evaluate_space(Color.WHITE, pos, ei), Weights[Space]),
                Score.Multiply(evaluate_space(Color.BLACK, pos, ei), Weights[Space]));
            add_IdxSt((int) Term.TOTAL, score);
        }

        return (pos.side_to_move() == Color.WHITE ? v : -v) + Tempo; // Side to move point of view
    }

    /// init() computes evaluation weights, usually at startup
    internal static void init()
    {
        const int MaxSlope = 8700;
        const int Peak = 1280000;
        var t = 0;

        for (var i = 0; i < 400; ++i)
        {
            t = Math.Min(Peak, Math.Min(i*i*27, t + MaxSlope));
            KingDanger[i] = Score.Multiply(Score.make_score(t/1000, 0), Weights[KingSafety]);
        }
    }

    private static double to_cp(ValueT v)
    {
        return (double) v/Value.PawnValueEg;
    }

    private static void add_IdxCtSt(int idx, ColorT c, ScoreT s)
    {
        scores[idx, c, (int) Phase.MG] = to_cp(Score.mg_value(s));
        scores[idx, c, (int) Phase.EG] = to_cp(Score.eg_value(s));
    }

    private static void add_IdxStSt(int idx, ScoreT w, ScoreT b)
    {
        add_IdxCtSt(idx, Color.WHITE, w);
        add_IdxCtSt(idx, Color.BLACK, b);
    }

    private static void add_IdxSt(int idx, ScoreT w)
    {
        add_IdxCtSt(idx, Color.WHITE, w);
        add_IdxCtSt(idx, Color.BLACK, Score.SCORE_ZERO);
    }

    private static string termString(Term t)
    {
        var os = new StringBuilder();
        if (t == Term.MATERIAL || t == Term.IMBALANCE || t == (Term) ((int)PieceType.PAWN) || t == Term.TOTAL)
        {
            os.Append("  ---   --- |   ---   --- | ");
        }
        else
        {
            os.Append($"{scores[(int) t, Color.WHITE, (int) Phase.MG],5:N2} ");
            os.Append($"{scores[(int) t, Color.WHITE, (int) Phase.EG],5:N2} | ");
            os.Append($"{scores[(int) t, Color.BLACK, (int) Phase.MG],5:N2} ");
            os.Append($"{scores[(int) t, Color.BLACK, (int) Phase.EG],5:N2} | ");
        }

        os.Append($"{scores[(int) t, Color.WHITE, (int) Phase.MG] - scores[(int) t, Color.BLACK, (int) Phase.MG],5:N2} ");
        os.Append($"{scores[(int) t, Color.WHITE, (int) Phase.EG] - scores[(int) t, Color.BLACK, (int) Phase.EG],5:N2} ");

        return os.ToString();
    }

    /// trace() is like evaluate(), but instead of returning a value, it returns
    /// a string (suitable for outputting to stdout) that contains the detailed
    /// descriptions and values of each evaluation term. Useful for debugging.
    internal static string trace(Position pos)
    {
        scores = new double[(int) Term.TERM_NB, Color.COLOR_NB, (int) Phase.PHASE_NB];

        var v = evaluate(true, pos);
        v = pos.side_to_move() == Color.WHITE ? v : -v; // White's point of view

        var ss = new StringBuilder();

        ss.AppendLine("      Eval term |    White    |    Black    |    Total    ");
        ss.AppendLine("                |   MG    EG  |   MG    EG  |   MG    EG  ");
        ss.AppendLine("----------------+-------------+-------------+-------------");
        ss.AppendLine($"       Material | {termString(Term.MATERIAL)}");
        ss.AppendLine($"      Imbalance | {termString(Term.IMBALANCE)}");
        ss.AppendLine($"          Pawns | {termString((Term) (int)PieceType.PAWN)}");
        ss.AppendLine($"        Knights | {termString((Term) (int)PieceType.KNIGHT)}");
        ss.AppendLine($"         Bishop | {termString((Term) (int)PieceType.BISHOP)}");
        ss.AppendLine($"          Rooks | {termString((Term) (int)PieceType.ROOK)}");
        ss.AppendLine($"         Queens | {termString((Term) (int)PieceType.QUEEN)}");
        ss.AppendLine($"       Mobility | {termString(Term.MOBILITY)}");
        ss.AppendLine($"    King safety | {termString((Term)(int) PieceType.KING)}");
        ss.AppendLine($"        Threats | {termString(Term.THREAT)}");
        ss.AppendLine($"   Passed pawns | {termString(Term.PASSED)}");
        ss.AppendLine($"          Space | {termString(Term.SPACE)}");
        ss.AppendLine("----------------+-------------+-------------+-------------");
        ss.AppendLine($"          Total | {termString(Term.TOTAL)}");

        ss.AppendLine();
        ss.AppendLine($"Total Evaluation: {to_cp(v),5:N2} (white side)");

        return ss.ToString();
    }

    internal struct Weight
    {
        internal Weight(int mg, int eg)
        {
            this.mg = mg;
            this.eg = eg;
        }

        internal int mg;

        internal int eg;
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