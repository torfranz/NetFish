using System;
using System.Diagnostics;
using System.Linq;

#if PRIMITIVE
using FileT = System.Int32;
using ColorT = System.Int32;
using ValueT = System.Int32;
using ScoreT = System.Int32;
using SquareT = System.Int32;
using BitboardT = System.UInt64;
#endif

internal static class Pawns
{
    internal const int Size = 16384;
    // Doubled pawn penalty by file
    private static readonly ScoreT[] Doubled =
    {
        Score.make_score(13, 43), Score.make_score(20, 48),
        Score.make_score(23, 48), Score.make_score(23, 48),
        Score.make_score(23, 48), Score.make_score(23, 48),
        Score.make_score(20, 48), Score.make_score(13, 43)
    };

    // Isolated pawn penalty by opposed flag and file
    private static readonly ScoreT[][] Isolated =
    {
        new[]
        {
            Score.make_score(37, 45), Score.make_score(54, 52),
            Score.make_score(60, 52), Score.make_score(60, 52),
            Score.make_score(60, 52), Score.make_score(60, 52),
            Score.make_score(54, 52), Score.make_score(37, 45)
        },
        new[]
        {
            Score.make_score(25, 30), Score.make_score(36, 35),
            Score.make_score(40, 35), Score.make_score(40, 35),
            Score.make_score(40, 35), Score.make_score(40, 35),
            Score.make_score(36, 35), Score.make_score(25, 30)
        }
    };

    // Backward pawn penalty by opposed flag
    private static readonly ScoreT[] Backward = {Score.make_score(67, 42), Score.make_score(49, 24)};

    // Connected pawn bonus by opposed, phalanx, twice supported and rank
    private static readonly ScoreT[,,,] Connected = new ScoreT[2, 2, 2, Rank.RANK_NB];

    // Levers bonus by rank
    private static readonly ScoreT[] Lever =
    {
        Score.make_score(0, 0), Score.make_score(0, 0), Score.make_score(0, 0),
        Score.make_score(0, 0), Score.make_score(20, 20), Score.make_score(40, 40),
        Score.make_score(0, 0), Score.make_score(0, 0)
    };

    // Unsupported pawn penalty
    private static readonly ScoreT UnsupportedPawnPenalty = Score.make_score(20, 10);

    // Center bind bonus: Two pawns controlling the same central square
    private static readonly BitboardT[] CenterBindMask =
    {
        (Bitboard.FileDBB | Bitboard.FileEBB)
        & (Bitboard.Rank5BB | Bitboard.Rank6BB | Bitboard.Rank7BB),
        (Bitboard.FileDBB | Bitboard.FileEBB)
        & (Bitboard.Rank4BB | Bitboard.Rank3BB | Bitboard.Rank2BB)
    };

    private static readonly ScoreT CenterBind = Score.make_score(16, 0);

    // Weakness of our pawn shelter in front of the king by [distance from edge][rank]
    private static readonly ValueT[][] ShelterWeakness =
    {
        new[]
        {
            Value.Create(97), Value.Create(21), Value.Create(26),
            Value.Create(51), Value.Create(87), Value.Create(89),
            Value.Create(99)
        },
        new[]
        {
            Value.Create(120), Value.Create(0), Value.Create(28),
            Value.Create(76), Value.Create(88), Value.Create(103),
            Value.Create(104)
        },
        new[]
        {
            Value.Create(101), Value.Create(7), Value.Create(54),
            Value.Create(78), Value.Create(77), Value.Create(92),
            Value.Create(101)
        },
        new[]
        {
            Value.Create(80), Value.Create(11), Value.Create(44),
            Value.Create(68), Value.Create(87), Value.Create(90),
            Value.Create(119)
        }
    };

    // Danger of enemy pawns moving toward our king by [type][distance from edge][rank]
    private static readonly ValueT[][][] StormDanger =
    {
        new[]
        {
            new[]
            {
                Value.Create(0), Value.Create(67), Value.Create(134),
                Value.Create(38), Value.Create(32), Value.Create(0),
                Value.Create(0), Value.Create(0)
            },
            new[]
            {
                Value.Create(0), Value.Create(57), Value.Create(139),
                Value.Create(37), Value.Create(22), Value.Create(0),
                Value.Create(0), Value.Create(0)
            },
            new[]
            {
                Value.Create(0), Value.Create(43), Value.Create(115),
                Value.Create(43), Value.Create(27), Value.Create(0),
                Value.Create(0), Value.Create(0)
            },
            new[]
            {
                Value.Create(0), Value.Create(68), Value.Create(124),
                Value.Create(57), Value.Create(32), Value.Create(0),
                Value.Create(0), Value.Create(0)
            }
        },
        new[]
        {
            new[]
            {
                Value.Create(20), Value.Create(43), Value.Create(100),
                Value.Create(56), Value.Create(20), Value.Create(0),
                Value.Create(0), Value.Create(0)
            },
            new[]
            {
                Value.Create(23), Value.Create(20), Value.Create(98),
                Value.Create(40), Value.Create(15), Value.Create(0),
                Value.Create(0), Value.Create(0)
            },
            new[]
            {
                Value.Create(23), Value.Create(39), Value.Create(103),
                Value.Create(36), Value.Create(18), Value.Create(0),
                Value.Create(0), Value.Create(0)
            },
            new[]
            {
                Value.Create(28), Value.Create(19), Value.Create(108),
                Value.Create(42), Value.Create(26), Value.Create(0),
                Value.Create(0), Value.Create(0)
            }
        },
        new[]
        {
            new[]
            {
                Value.Create(0), Value.Create(0), Value.Create(75),
                Value.Create(14), Value.Create(2), Value.Create(0),
                Value.Create(0), Value.Create(0)
            },
            new[]
            {
                Value.Create(0), Value.Create(0), Value.Create(150),
                Value.Create(30), Value.Create(4), Value.Create(0),
                Value.Create(0), Value.Create(0)
            },
            new[]
            {
                Value.Create(0), Value.Create(0), Value.Create(160),
                Value.Create(22), Value.Create(5), Value.Create(0),
                Value.Create(0), Value.Create(0)
            },
            new[]
            {
                Value.Create(0), Value.Create(0), Value.Create(166),
                Value.Create(24), Value.Create(13), Value.Create(0),
                Value.Create(0), Value.Create(0)
            }
        },
        new[]
        {
            new[]
            {
                Value.Create(0), Value.Create(-283), Value.Create(-281),
                Value.Create(57), Value.Create(31)
            },
            new[]
            {
                Value.Create(0), Value.Create(58), Value.Create(141),
                Value.Create(39), Value.Create(18)
            },
            new[]
            {
                Value.Create(0), Value.Create(65), Value.Create(142),
                Value.Create(48), Value.Create(32)
            },
            new[]
            {
                Value.Create(0), Value.Create(60), Value.Create(126),
                Value.Create(51), Value.Create(19)
            }
        }
    };

    // Max bonus for king safety. Corresponds to start position with all the pawns
    // in front of the king and no enemy pawn on the horizon.
    private static readonly ValueT MaxSafetyBonus = Value.Create(258);

    private static readonly int[] Seed = {0, 6, 15, 10, 57, 75, 135, 258};

    internal static ScoreT evaluate(ColorT Us, Position pos, Entry e)
    {
        var Them = (Us == Color.WHITE ? Color.BLACK : Color.WHITE);
        var Up = (Us == Color.WHITE ? Square.DELTA_N : Square.DELTA_S);
        var Right = (Us == Color.WHITE ? Square.DELTA_NE : Square.DELTA_SW);
        var Left = (Us == Color.WHITE ? Square.DELTA_NW : Square.DELTA_SE);

        BitboardT b;

        var score = Score.SCORE_ZERO;
        
        var ourPawns = pos.pieces_CtPt(Us, PieceType.PAWN);
        var theirPawns = pos.pieces_CtPt(Them, PieceType.PAWN);

        e.passedPawns[Us] = Bitboard.Create(0);
        e.kingSquares[Us] = Square.SQ_NONE;
        e.semiopenFiles[Us] = 0xFF;
        e.pawnAttacks[Us] = Bitboard.shift_bb(Right, ourPawns) | Bitboard.shift_bb(Left, ourPawns);
        e.pawnsOnSquares[Us, Color.BLACK] = Bitcount.popcount_Max15(ourPawns & Bitboard.DarkSquares);
        e.pawnsOnSquares[Us, Color.WHITE] = pos.count(PieceType.PAWN, Us) - e.pawnsOnSquares[Us, Color.BLACK];

        // Loop through all pawns of the current color and score each pawn
        for (var idx = 0; idx < 16; idx++)
        {
            var s = pos.square(PieceType.PAWN, Us, idx);
            if (s == Square.SQ_NONE)
            {
                break;
            }

            Debug.Assert(pos.piece_on(s) == Piece.make_piece(Us, PieceType.PAWN));

            var f = Square.file_of(s);

            // This file cannot be semi-open
            e.semiopenFiles[Us] &= ~(1 << f);

            // Flag the pawn
            var neighbours = ourPawns & Utils.adjacent_files_bb(f);
            var doubled = ourPawns & Utils.forward_bb(Us, s);
            bool opposed = (theirPawns & Utils.forward_bb(Us, s)) != 0;
            var passed = (theirPawns & Utils.passed_pawn_mask(Us, s)) == 0;
            bool lever = (theirPawns & Utils.StepAttacksBB[Piece.make_piece(Us, PieceType.PAWN), s]) != 0;
            var phalanx = neighbours & Utils.rank_bb_St(s);
            var supported = neighbours & Utils.rank_bb_St(s - Up);
            bool connected = (supported | phalanx) != 0;
            var isolated = neighbours == 0;

            // Test for backward pawn.
            // If the pawn is passed, isolated, lever or connected it cannot be
            // backward. If there are friendly pawns behind on adjacent files
            // or if it is sufficiently advanced, it cannot be backward either.
            bool backward;
            if ((passed | isolated | lever | connected) || (ourPawns & Utils.pawn_attack_span(Them, s)) != 0
                || (Rank.relative_rank_CtSt(Us, s) >= Rank.RANK_5))
            {
                backward = false;
            }
            else
            {
                // We now know there are no friendly pawns beside or behind this
                // pawn on adjacent files. We now check whether the pawn is
                // backward by looking in the forward direction on the adjacent
                // files, and picking the closest pawn there.
                b = Utils.pawn_attack_span(Us, s) & (ourPawns | theirPawns);
                b = Utils.pawn_attack_span(Us, s) & Utils.rank_bb_St(Utils.backmost_sq(Us, b));

                // If we have an enemy pawn in the same or next rank, the pawn is
                // backward because it cannot advance without being captured.
                backward = ((b | Bitboard.shift_bb(Up, b)) & theirPawns) != 0;
            }

            Debug.Assert(opposed | passed | (Utils.pawn_attack_span(Us, s) & theirPawns) != 0);

            // Passed pawns will be properly scored in evaluation because we need
            // full attack info to evaluate passed pawns. Only the frontmost passed
            // pawn on each file is considered a true passed pawn.
            if (passed && doubled == 0)
            {
                e.passedPawns[Us] = Bitboard.OrWithSquare(e.passedPawns[Us], s);
            }

            // Score this pawn
            if (isolated)
            {
                score -= Isolated[opposed ? 1 : 0][f];
            }

            else if (backward)
            {
                score -= Backward[opposed ? 1 : 0];
            }

            else if (supported == 0)
            {
                score -= UnsupportedPawnPenalty;
            }

            if (connected)
            {
                score +=
                    Connected[
                        opposed ? 1 : 0,
                        phalanx != 0 ? 1 : 0,
                        Bitboard.more_than_one(supported) ? 1 : 0,
                        Rank.relative_rank_CtSt(Us, s)];
            }

            if (doubled != 0)
            {
                score -= Score.Divide(Doubled[f], Utils.distance_Rank_StSt(s, Utils.frontmost_sq(Us, doubled)));
            }

            if (lever)
            {
                score += Lever[Rank.relative_rank_CtSt(Us, s)];
            }
        }

        b = Bitboard.Create((uint) (e.semiopenFiles[Us] ^ 0xFF));
        e.pawnSpan[Us] = b != 0 ? Utils.msb(b) - (int)Utils.lsb(b) : 0;

        // Center binds: Two pawns controlling the same central square
        b = Bitboard.shift_bb(Right, ourPawns) & Bitboard.shift_bb(Left, ourPawns) & CenterBindMask[Us];
        score += Bitcount.popcount_Max15(b)*CenterBind;

        return score;
    }

    /// Pawns::init() initializes some tables needed by evaluation. Instead of using
    /// hard-coded tables, when makes sense, we prefer to calculate them with a formula
    /// to reduce independent parameters and to allow easier tuning and better insight.
    internal static void init()
    {
        for (var opposed = 0; opposed <= 1; ++opposed)
        {
            for (var phalanx = 0; phalanx <= 1; ++phalanx)
            {
                for (var apex = 0; apex <= 1; ++apex)
                {
                    for (var r = (int)Rank.RANK_2; r < Rank.RANK_8; ++r)
                    {
                        var v = (Seed[r] + (phalanx != 0 ? (Seed[r + 1] - Seed[r])/2 : 0)) >> opposed;
                        v += (apex != 0 ? v/2 : 0);
                        Connected[opposed, phalanx, apex, r] = Score.make_score(3*v/2, v);
                    }
                }
            }
        }
    }

    /// Pawns::probe() looks up the current position's pawns configuration in
    /// the pawns hash table. It returns a pointer to the Entry if the position
    /// is found. Otherwise a new Entry is computed and stored there, so we don't
    /// have to recompute all when the same pawns configuration occurs again.
    internal static Entry probe(Position pos)
    {
        var key = pos.pawn_key();
        var hashKey = (uint) key & (Size - 1);
        Entry e;
        if ((e = pos.this_thread().pawnsTable[hashKey]) == null)
        {
            e = new Entry();
            pos.this_thread().pawnsTable[hashKey] = e;
        }
        else if (e.key == key)
        {
            return e;
        }

        e.key = key;
        e.score = evaluate(Color.WHITE, pos, e) - evaluate(Color.BLACK, pos, e);
        return e;
    }

    /// Pawns::Entry contains various information about a pawn structure. A lookup
    /// to the pawn hash table (performed by calling the probe function) returns a
    /// pointer to an Entry object.
    internal class Entry
    {
        internal int[] castlingRights = new int[Color.COLOR_NB];

        internal ulong key;

        internal ScoreT[] kingSafety = new ScoreT[Color.COLOR_NB];

        internal SquareT[] kingSquares = new SquareT[Color.COLOR_NB];

        internal BitboardT[] passedPawns = new BitboardT[Color.COLOR_NB];

        internal BitboardT[] pawnAttacks = new BitboardT[Color.COLOR_NB];

        internal int[,] pawnsOnSquares = new int[Color.COLOR_NB, Color.COLOR_NB]; // [color][light/dark squares]

        internal int[] pawnSpan = new int[Color.COLOR_NB];

        internal ScoreT score;

        internal int[] semiopenFiles = new int[Color.COLOR_NB];

        internal ScoreT pawns_score()
        {
            return score;
        }

        internal BitboardT pawn_attacks(ColorT c)
        {
            return pawnAttacks[c];
        }

        internal BitboardT passed_pawns(ColorT c)
        {
            return passedPawns[c];
        }

        internal int pawn_span(ColorT c)
        {
            return pawnSpan[c];
        }

        internal int semiopen_file(ColorT c, FileT f)
        {
            return semiopenFiles[c] & (1 << f);
        }

        internal int semiopen_side(ColorT c, FileT f, bool leftSide)
        {
            return semiopenFiles[c] & (leftSide ? (1 << f) - 1 : ~((1 << (f + 1)) - 1));
        }

        internal int pawns_on_same_color_squares(ColorT c, SquareT s)
        {
            return pawnsOnSquares[c, Bitboard.AndWithSquare(Bitboard.DarkSquares, s)!=0 ? 1 : 0];
        }

        internal ScoreT king_safety(ColorT Us, Position pos, SquareT ksq)
        {
            return kingSquares[Us] == ksq && castlingRights[Us] == pos.can_castle(Us)
                ? kingSafety[Us]
                : (kingSafety[Us] = do_king_safety(Us, pos, ksq));
        }

        /// Entry::do_king_safety() calculates a bonus for king safety. It is called only
        /// when king square changes, which is about 20% of total king_safety() calls.
        private ScoreT do_king_safety(ColorT Us, Position pos, SquareT ksq)
        {
            kingSquares[Us] = ksq;
            castlingRights[Us] = pos.can_castle(Us);
            var minKingPawnDistance = 0;

            var pawns = pos.pieces_CtPt(Us, PieceType.PAWN);
            if (pawns != 0)
            {
                while ((Utils.DistanceRingBB[ksq, minKingPawnDistance++] & pawns) == 0)
                {
                }
            }

            if (Rank.relative_rank_CtSt(Us, ksq) > Rank.RANK_4)
            {
                return Score.make_score(0, -16*minKingPawnDistance);
            }

            var bonus = shelter_storm(Us, pos, ksq);

            // If we can castle use the bonus after the castling if it is bigger
            if (pos.can_castle(Movegen.MakeCastling(Us, CastlingSide.KING_SIDE)))
            {
                bonus = Value.Create(
                    Math.Max(bonus, shelter_storm(Us, pos, Square.relative_square(Us, Square.SQ_G1))));
            }

            if (pos.can_castle(Movegen.MakeCastling(Us, CastlingSide.QUEEN_SIDE)))
            {
                bonus = Value.Create(
                    Math.Max(bonus, shelter_storm(Us, pos, Square.relative_square(Us, Square.SQ_C1))));
            }

            return Score.make_score(bonus, -16*minKingPawnDistance);
        }

        /// Entry::shelter_storm() calculates shelter and storm penalties for the file
        /// the king is on, as well as the two adjacent files.
        private ValueT shelter_storm(ColorT Us, Position pos, SquareT ksq)
        {
            const int NoFriendlyPawn = 0;
            const int Unblocked = 1;
            const int BlockedByPawn = 2;
            const int BlockedByKing = 3;
            var Them = (Us == Color.WHITE ? Color.BLACK : Color.WHITE);

            var b = pos.pieces_Pt(PieceType.PAWN) & (Utils.in_front_bb(Us, Square.rank_of(ksq)) | Utils.rank_bb_St(ksq));
            var ourPawns = b & pos.pieces_Ct(Us);
            var theirPawns = b & pos.pieces_Ct(Them);
            var safety = MaxSafetyBonus;
            var center = File.Create(Math.Max(File.FILE_B, Math.Min(File.FILE_G, Square.file_of(ksq))));

            for (var f = center - 1; f <= (int)center + 1; ++f)
            {
                b = ourPawns & Utils.file_bb_Ft(File.Create(f));
                var rkUs = b != 0 ? Rank.relative_rank_CtSt(Us, Utils.backmost_sq(Us, b)) : Rank.RANK_1;

                b = theirPawns & Utils.file_bb_Ft(File.Create(f));
                var rkThem = b != 0 ? Rank.relative_rank_CtSt(Us, Utils.frontmost_sq(Them, b)) : Rank.RANK_1;

                safety -= ShelterWeakness[Math.Min(f, File.FILE_H - f)][rkUs]
                          + StormDanger[
                              f == (int)Square.file_of(ksq) && rkThem == Rank.relative_rank_CtSt(Us, ksq) + 1
                                  ? BlockedByKing
                                  : (int)rkUs == Rank.RANK_1
                                      ? NoFriendlyPawn
                                      : rkThem == rkUs + 1 ? BlockedByPawn : Unblocked][Math.Min(f, File.FILE_H - f)][rkThem];
            }

            return safety;
        }
    }
}