using System;
using System.Diagnostics;
using System.Linq;

internal static class Pawns
{
    internal const int Size = 16384;
    // Doubled pawn penalty by file
    private static readonly Score[] Doubled =
    {
        Score.make_score(13, 43), Score.make_score(20, 48),
        Score.make_score(23, 48), Score.make_score(23, 48),
        Score.make_score(23, 48), Score.make_score(23, 48),
        Score.make_score(20, 48), Score.make_score(13, 43)
    };

    // Isolated pawn penalty by opposed flag and file
    private static readonly Score[][] Isolated =
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
    private static readonly Score[] Backward = {Score.make_score(67, 42), Score.make_score(49, 24)};

    // Connected pawn bonus by opposed, phalanx, twice supported and rank
    private static readonly Score[,,,] Connected = new Score[2, 2, 2, Rank.RANK_NB_C];

    // Levers bonus by rank
    private static readonly Score[] Lever =
    {
        Score.make_score(0, 0), Score.make_score(0, 0), Score.make_score(0, 0),
        Score.make_score(0, 0), Score.make_score(20, 20), Score.make_score(40, 40),
        Score.make_score(0, 0), Score.make_score(0, 0)
    };

    // Unsupported pawn penalty
    private static readonly Score UnsupportedPawnPenalty = Score.make_score(20, 10);

    // Center bind bonus: Two pawns controlling the same central square
    private static readonly Bitboard[] CenterBindMask =
    {
        (Bitboard.FileDBB | Bitboard.FileEBB)
        & (Bitboard.Rank5BB | Bitboard.Rank6BB | Bitboard.Rank7BB),
        (Bitboard.FileDBB | Bitboard.FileEBB)
        & (Bitboard.Rank4BB | Bitboard.Rank3BB | Bitboard.Rank2BB)
    };

    private static readonly Score CenterBind = Score.make_score(16, 0);

    // Weakness of our pawn shelter in front of the king by [distance from edge][rank]
    private static readonly Value[][] ShelterWeakness =
    {
        new[]
        {
            new Value(97), new Value(21), new Value(26),
            new Value(51), new Value(87), new Value(89),
            new Value(99)
        },
        new[]
        {
            new Value(120), new Value(0), new Value(28),
            new Value(76), new Value(88), new Value(103),
            new Value(104)
        },
        new[]
        {
            new Value(101), new Value(7), new Value(54),
            new Value(78), new Value(77), new Value(92),
            new Value(101)
        },
        new[]
        {
            new Value(80), new Value(11), new Value(44),
            new Value(68), new Value(87), new Value(90),
            new Value(119)
        }
    };

    // Danger of enemy pawns moving toward our king by [type][distance from edge][rank]
    private static readonly Value[][][] StormDanger =
    {
        new[]
        {
            new[]
            {
                new Value(0), new Value(67), new Value(134),
                new Value(38), new Value(32), new Value(0),
                new Value(0), new Value(0)
            },
            new[]
            {
                new Value(0), new Value(57), new Value(139),
                new Value(37), new Value(22), new Value(0),
                new Value(0), new Value(0)
            },
            new[]
            {
                new Value(0), new Value(43), new Value(115),
                new Value(43), new Value(27), new Value(0),
                new Value(0), new Value(0)
            },
            new[]
            {
                new Value(0), new Value(68), new Value(124),
                new Value(57), new Value(32), new Value(0),
                new Value(0), new Value(0)
            }
        },
        new[]
        {
            new[]
            {
                new Value(20), new Value(43), new Value(100),
                new Value(56), new Value(20), new Value(0),
                new Value(0), new Value(0)
            },
            new[]
            {
                new Value(23), new Value(20), new Value(98),
                new Value(40), new Value(15), new Value(0),
                new Value(0), new Value(0)
            },
            new[]
            {
                new Value(23), new Value(39), new Value(103),
                new Value(36), new Value(18), new Value(0),
                new Value(0), new Value(0)
            },
            new[]
            {
                new Value(28), new Value(19), new Value(108),
                new Value(42), new Value(26), new Value(0),
                new Value(0), new Value(0)
            }
        },
        new[]
        {
            new[]
            {
                new Value(0), new Value(0), new Value(75),
                new Value(14), new Value(2), new Value(0),
                new Value(0), new Value(0)
            },
            new[]
            {
                new Value(0), new Value(0), new Value(150),
                new Value(30), new Value(4), new Value(0),
                new Value(0), new Value(0)
            },
            new[]
            {
                new Value(0), new Value(0), new Value(160),
                new Value(22), new Value(5), new Value(0),
                new Value(0), new Value(0)
            },
            new[]
            {
                new Value(0), new Value(0), new Value(166),
                new Value(24), new Value(13), new Value(0),
                new Value(0), new Value(0)
            }
        },
        new[]
        {
            new[]
            {
                new Value(0), new Value(-283), new Value(-281),
                new Value(57), new Value(31)
            },
            new[]
            {
                new Value(0), new Value(58), new Value(141),
                new Value(39), new Value(18)
            },
            new[]
            {
                new Value(0), new Value(65), new Value(142),
                new Value(48), new Value(32)
            },
            new[]
            {
                new Value(0), new Value(60), new Value(126),
                new Value(51), new Value(19)
            }
        }
    };

    // Max bonus for king safety. Corresponds to start position with all the pawns
    // in front of the king and no enemy pawn on the horizon.
    private static readonly Value MaxSafetyBonus = new Value(258);

    private static readonly int[] Seed = {0, 6, 15, 10, 57, 75, 135, 258};

    internal static Score evaluate(Color Us, Position pos, Entry e)
    {
        var Them = (Us == Color.WHITE ? Color.BLACK : Color.WHITE);
        var Up = (Us == Color.WHITE ? Square.DELTA_N : Square.DELTA_S);
        var Right = (Us == Color.WHITE ? Square.DELTA_NE : Square.DELTA_SW);
        var Left = (Us == Color.WHITE ? Square.DELTA_NW : Square.DELTA_SE);

        Bitboard b;

        var score = Score.SCORE_ZERO;
        
        var ourPawns = pos.pieces(Us, PieceType.PAWN);
        var theirPawns = pos.pieces(Them, PieceType.PAWN);

        e.passedPawns[Us.ValueMe] = new Bitboard(0);
        e.kingSquares[Us.ValueMe] = Square.SQ_NONE;
        e.semiopenFiles[Us.ValueMe] = 0xFF;
        e.pawnAttacks[Us.ValueMe] = Bitboard.shift_bb(Right, ourPawns) | Bitboard.shift_bb(Left, ourPawns);
        e.pawnsOnSquares[Us.ValueMe, Color.BLACK_C] = Bitcount.popcount_Max15(ourPawns & Bitboard.DarkSquares);
        e.pawnsOnSquares[Us.ValueMe, Color.WHITE_C] = pos.count(PieceType.PAWN, Us) - e.pawnsOnSquares[Us.ValueMe, Color.BLACK_C];

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
            e.semiopenFiles[Us.ValueMe] &= ~(1 << f);

            // Flag the pawn
            var neighbours = ourPawns & Utils.adjacent_files_bb(f);
            var doubled = ourPawns & Utils.forward_bb(Us, s);
            bool opposed = theirPawns & Utils.forward_bb(Us, s);
            var passed = !(theirPawns & Utils.passed_pawn_mask(Us, s));
            bool lever = theirPawns & Utils.StepAttacksBB[Piece.make_piece(Us, PieceType.PAWN), s];
            var phalanx = neighbours & Utils.rank_bb(s);
            var supported = neighbours & Utils.rank_bb(s - Up);
            bool connected = supported | phalanx;
            var isolated = !neighbours;

            // Test for backward pawn.
            // If the pawn is passed, isolated, lever or connected it cannot be
            // backward. If there are friendly pawns behind on adjacent files
            // or if it is sufficiently advanced, it cannot be backward either.
            bool backward;
            if ((passed | isolated | lever | connected) || (ourPawns & Utils.pawn_attack_span(Them, s))
                || (Rank.relative_rank(Us, s) >= Rank.RANK_5_C))
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
                b = Utils.pawn_attack_span(Us, s) & Utils.rank_bb(Utils.backmost_sq(Us, b));

                // If we have an enemy pawn in the same or next rank, the pawn is
                // backward because it cannot advance without being captured.
                backward = (b | Bitboard.shift_bb(Up, b)) & theirPawns;
            }

            Debug.Assert(opposed | passed | (Utils.pawn_attack_span(Us, s) & theirPawns));

            // Passed pawns will be properly scored in evaluation because we need
            // full attack info to evaluate passed pawns. Only the frontmost passed
            // pawn on each file is considered a true passed pawn.
            if (passed && !doubled)
            {
                e.passedPawns[Us.ValueMe] |= s;
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

            else if (!supported)
            {
                score -= UnsupportedPawnPenalty;
            }

            if (connected)
            {
                score +=
                    Connected[
                        opposed ? 1 : 0,
                        phalanx ? 1 : 0,
                        Bitboard.more_than_one(supported) ? 1 : 0,
                        Rank.relative_rank(Us, s)];
            }

            if (doubled)
            {
                score -= Doubled[f]/Utils.distance_Rank(s, Utils.frontmost_sq(Us, doubled));
            }

            if (lever)
            {
                score += Lever[Rank.relative_rank(Us, s)];
            }
        }

        b = new Bitboard((uint) (e.semiopenFiles[Us.ValueMe] ^ 0xFF));
        e.pawnSpan[Us.ValueMe] = b ? Utils.msb(b) - (int)Utils.lsb(b) : 0;

        // Center binds: Two pawns controlling the same central square
        b = Bitboard.shift_bb(Right, ourPawns) & Bitboard.shift_bb(Left, ourPawns) & CenterBindMask[Us.ValueMe];
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
                    for (var r = Rank.RANK_2_C; r < Rank.RANK_8_C; ++r)
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
        internal int[] castlingRights = new int[Color.COLOR_NB_C];

        internal ulong key;

        internal Score[] kingSafety = new Score[Color.COLOR_NB_C];

        internal Square[] kingSquares = new Square[Color.COLOR_NB_C];

        internal Bitboard[] passedPawns = new Bitboard[Color.COLOR_NB_C];

        internal Bitboard[] pawnAttacks = new Bitboard[Color.COLOR_NB_C];

        internal int[,] pawnsOnSquares = new int[Color.COLOR_NB_C, Color.COLOR_NB_C]; // [color][light/dark squares]

        internal int[] pawnSpan = new int[Color.COLOR_NB_C];

        internal Score score;

        internal int[] semiopenFiles = new int[Color.COLOR_NB_C];

        internal Score pawns_score()
        {
            return score;
        }

        internal Bitboard pawn_attacks(Color c)
        {
            return pawnAttacks[c.ValueMe];
        }

        internal Bitboard passed_pawns(Color c)
        {
            return passedPawns[c.ValueMe];
        }

        internal int pawn_span(Color c)
        {
            return pawnSpan[c.ValueMe];
        }

        internal int semiopen_file(Color c, File f)
        {
            return semiopenFiles[c.ValueMe] & (1 << f);
        }

        internal int semiopen_side(Color c, File f, bool leftSide)
        {
            return semiopenFiles[c.ValueMe] & (leftSide ? (1 << f) - 1 : ~((1 << (f + 1)) - 1));
        }

        internal int pawns_on_same_color_squares(Color c, Square s)
        {
            return pawnsOnSquares[c.ValueMe, Bitboard.DarkSquares & s ? 1 : 0];
        }

        internal Score king_safety(Color Us, Position pos, Square ksq)
        {
            return kingSquares[Us.ValueMe] == ksq && castlingRights[Us.ValueMe] == pos.can_castle(Us)
                ? kingSafety[Us.ValueMe]
                : (kingSafety[Us.ValueMe] = do_king_safety(Us, pos, ksq));
        }

        /// Entry::do_king_safety() calculates a bonus for king safety. It is called only
        /// when king square changes, which is about 20% of total king_safety() calls.
        private Score do_king_safety(Color Us, Position pos, Square ksq)
        {
            kingSquares[Us.ValueMe] = ksq;
            castlingRights[Us.ValueMe] = pos.can_castle(Us);
            var minKingPawnDistance = 0;

            var pawns = pos.pieces(Us, PieceType.PAWN);
            if (pawns)
            {
                while (!(Utils.DistanceRingBB[ksq, minKingPawnDistance++] & pawns))
                {
                }
            }

            if (Rank.relative_rank(Us, ksq) > Rank.RANK_4_C)
            {
                return Score.make_score(0, -16*minKingPawnDistance);
            }

            var bonus = shelter_storm(Us, pos, ksq);

            // If we can castle use the bonus after the castling if it is bigger
            if (pos.can_castle(Movegen.MakeCastling(Us, CastlingSide.KING_SIDE)))
            {
                bonus = new Value(
                    Math.Max(bonus, shelter_storm(Us, pos, Square.relative_square(Us, Square.SQ_G1))));
            }

            if (pos.can_castle(Movegen.MakeCastling(Us, CastlingSide.QUEEN_SIDE)))
            {
                bonus = new Value(
                    Math.Max(bonus, shelter_storm(Us, pos, Square.relative_square(Us, Square.SQ_C1))));
            }

            return Score.make_score(bonus, -16*minKingPawnDistance);
        }

        /// Entry::shelter_storm() calculates shelter and storm penalties for the file
        /// the king is on, as well as the two adjacent files.
        private Value shelter_storm(Color Us, Position pos, Square ksq)
        {
            const int NoFriendlyPawn = 0;
            const int Unblocked = 1;
            const int BlockedByPawn = 2;
            const int BlockedByKing = 3;
            var Them = (Us == Color.WHITE ? Color.BLACK : Color.WHITE);

            var b = pos.pieces(PieceType.PAWN) & (Utils.in_front_bb(Us, Square.rank_of(ksq)) | Utils.rank_bb(ksq));
            var ourPawns = b & pos.pieces(Us);
            var theirPawns = b & pos.pieces(Them);
            var safety = MaxSafetyBonus;
            var center = File.Create(Math.Max(File.FILE_B_C, Math.Min(File.FILE_G_C, Square.file_of(ksq))));

            for (var f = center - 1; f <= (int)center + 1; ++f)
            {
                b = ourPawns & Utils.file_bb(File.Create(f));
                var rkUs = b ? Rank.relative_rank(Us, Utils.backmost_sq(Us, b)) : Rank.RANK_1;

                b = theirPawns & Utils.file_bb(File.Create(f));
                var rkThem = b ? Rank.relative_rank(Us, Utils.frontmost_sq(Them, b)) : Rank.RANK_1;

                safety -= ShelterWeakness[Math.Min(f, File.FILE_H_C - f)][rkUs]
                          + StormDanger[
                              f == (int)Square.file_of(ksq) && rkThem == Rank.relative_rank(Us, ksq) + 1
                                  ? BlockedByKing
                                  : (int)rkUs == Rank.RANK_1_C
                                      ? NoFriendlyPawn
                                      : rkThem == rkUs + 1 ? BlockedByPawn : Unblocked][Math.Min(f, File.FILE_H_C - f)][rkThem];
            }

            return safety;
        }
    }
}