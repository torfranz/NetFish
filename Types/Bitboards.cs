using System;

public static class Bitboards
{
    public static void init()
    {
        for (var s = Square.SQ_A1; s <= Square.SQ_H8; ++s)
        {
            Utils.SquareBB[s] = new Bitboard(1UL << s);
            Utils.BSFTable[Utils.bsf_index(Utils.SquareBB[s])] = s;
        }

        for (ulong b = 2; b < 256; ++b)
        {
            Utils.MSBTable[b] = Utils.MSBTable[b - 1] + (!(Bitboard.more_than_one(new Bitboard(b))) ? 1 : 0);
        }

        for (var f = File.FILE_A; f <= File.FILE_H; ++f)
        {
            Utils.FileBB[f] = f > File.FILE_A ? Utils.FileBB[f - 1] << 1 : Bitboard.FileABB;
        }

        for (var r = Rank.RANK_1; r <= Rank.RANK_8; ++r)
        {
            Utils.RankBB[r] = r > Rank.RANK_1 ? Utils.RankBB[r - 1] << 8 : Bitboard.Rank1BB;
        }

        for (var f = File.FILE_A; f <= File.FILE_H; ++f)
        {
            Utils.AdjacentFilesBB[f] = (f > File.FILE_A ? Utils.FileBB[f - 1] : new Bitboard(0))
                                       | (f < File.FILE_H ? Utils.FileBB[f + 1] : new Bitboard(0));
        }

        for (var r = Rank.RANK_1; r < Rank.RANK_8; ++r)
        {
            var value = (Utils.InFrontBB[Color.BLACK, r + 1] = Utils.InFrontBB[Color.BLACK, r] | Utils.RankBB[r]);
            Utils.InFrontBB[Color.WHITE, r] = ~value;
        }

        for (var c = Color.WHITE; c <= Color.BLACK; ++c)
        {
            for (var s = Square.SQ_A1; s <= Square.SQ_H8; ++s)
            {
                Utils.ForwardBB[c, s] = Utils.InFrontBB[c, Square.rank_of(s)] & Utils.FileBB[Square.file_of(s)];
                Utils.PawnAttackSpan[c, s] = Utils.InFrontBB[c, Square.rank_of(s)]
                                             & Utils.AdjacentFilesBB[Square.file_of(s)];
                Utils.PassedPawnMask[c, s] = Utils.ForwardBB[c, s] | Utils.PawnAttackSpan[c, s];
            }
        }

        for (var s1 = Square.SQ_A1; s1 <= Square.SQ_H8; ++s1)
        {
            for (var s2 = Square.SQ_A1; s2 <= Square.SQ_H8; ++s2)
            {
                if (s1 != s2)
                {
                    Utils.SquareDistance[s1, s2] = Math.Max(Utils.file_distance(s1, s2), Utils.rank_distance(s1, s2));
                    Utils.DistanceRingBB[s1, Utils.SquareDistance[s1, s2] - 1] |= s2;
                }
            }
        }

        int[][] steps =
        {
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 0}, new[] {7, 9, 0, 0, 0, 0, 0, 0, 0},
            new[] {17, 15, 10, 6, -6, -10, -15, -17, 0}, new[] {0, 0, 0, 0, 0, 0, 0, 0, 0},
            new[] {0, 0, 0, 0, 0, 0, 0, 0, 0}, new[] {0, 0, 0, 0, 0, 0, 0, 0, 0},
            new[] {9, 7, -7, -9, 8, 1, -1, -8, 0}
        };

        for (var c = Color.WHITE; c <= Color.BLACK; ++c)
        {
            for (var pt = PieceType.PAWN; pt <= PieceType.KING; ++pt)
            {
                for (var s = Square.SQ_A1; s <= Square.SQ_H8; ++s)
                {
                    for (var i = 0; steps[pt][i] != 0; ++i)
                    {
                        var to = s + new Square(c == Color.WHITE ? steps[pt][i] : -steps[pt][i]);

                        if (to.is_ok() && Utils.distance_Square(s, to) < 3)
                        {
                            Utils.StepAttacksBB[Piece.make_piece(c, pt), s] |= to;
                        }
                    }
                }
            }
        }

        Square[] RookDeltas = {Square.DELTA_N, Square.DELTA_E, Square.DELTA_S, Square.DELTA_W};
        Square[] BishopDeltas = {Square.DELTA_NE, Square.DELTA_SE, Square.DELTA_SW, Square.DELTA_NW};

        init_magics(
            Utils.RookAttacks,
            Utils.RookMagics,
            Utils.RookMasks,
            Utils.RookShifts,
            RookDeltas,
            Utils.magic_index_Rook);
        init_magics(
            Utils.BishopAttacks,
            Utils.BishopMagics,
            Utils.BishopMasks,
            Utils.BishopShifts,
            BishopDeltas,
            Utils.magic_index_Bishop);

        for (var s1 = Square.SQ_A1; s1 <= Square.SQ_H8; ++s1)
        {
            Utils.PseudoAttacks[PieceType.QUEEN, s1] =
                Utils.PseudoAttacks[PieceType.BISHOP, s1] = Utils.attacks_bb(PieceType.BISHOP, s1, new Bitboard(0));
            var bb = Utils.PseudoAttacks[PieceType.ROOK, s1] = Utils.attacks_bb(PieceType.ROOK, s1, new Bitboard(0));
            Utils.PseudoAttacks[PieceType.QUEEN, s1] = Utils.PseudoAttacks[PieceType.QUEEN, s1] | bb;

            for (var pc = Piece.W_BISHOP; pc <= Piece.W_ROOK; ++pc)
            {
                for (var s2 = Square.SQ_A1; s2 <= Square.SQ_H8; ++s2)
                {
                    if (!(Utils.PseudoAttacks[pc, s1] & s2))
                    {
                        continue;
                    }

                    Utils.LineBB[s1, s2] = (Utils.attacks_bb(pc, s1, new Bitboard(0))
                                            & Utils.attacks_bb(pc, s2, new Bitboard(0))) | s1 | s2;
                    Utils.BetweenBB[s1, s2] = Utils.attacks_bb(pc, s1, Utils.SquareBB[s2])
                                              & Utils.attacks_bb(pc, s2, Utils.SquareBB[s1]);
                }
            }
        }
    }

    private static Bitboard sliding_attack(Square[] deltas, Square sq, Bitboard occupied)
    {
        var attack = new Bitboard(0);

        for (var i = 0; i < 4; ++i)
        {
            for (var s = sq + deltas[i]; s.is_ok() && Utils.distance_Square(s, s - deltas[i]) == 1; s += deltas[i])
            {
                attack |= s;

                if (occupied & s)
                {
                    break;
                }
            }
        }

        return attack;
    }

    // init_magics() computes all rook and bishop attacks at startup. Magic
    // bitboards are used to look up attacks of sliding pieces. As a reference see
    // chessprogramming.wikispaces.com/Magic+Bitboards. In particular, here we
    // use the so called "fancy" approach.

    private static void init_magics(
        Bitboard[][] attacks,
        Bitboard[] magics,
        Bitboard[] masks,
        uint[] shifts,
        Square[] deltas,
        Utils.Fn index)
    {
        int[][] seeds =
        {
            new[] {8977, 44560, 54343, 38998, 5731, 95205, 104912, 17020},
            new[] {728, 10316, 55013, 32803, 12281, 15100, 16645, 255}
        };

        var occupancy = new Bitboard[4096];
        var reference = new Bitboard[4096];

        var age = new int[4096];
        int current = 0, i;
        Bitboard edges;

        for (var s = Square.SQ_A1; s <= Square.SQ_H8; ++s)
        {
            // Board edges are not considered in the relevant occupancies
            edges = ((Bitboard.Rank1BB | Bitboard.Rank8BB) & ~Utils.rank_bb(s)
                     | ((Bitboard.FileABB | Bitboard.FileHBB) & ~Utils.file_bb(s)));

            // Given a square 's', the mask is the bitboard of sliding attacks from
            // 's' computed on an empty board. The index must be big enough to contain
            // all the attacks for each possible subset of the mask and so is 2 power
            // the number of 1s of the mask. Hence we deduce the size of the shift to
            // apply to the 64 or 32 bits word to get the index.
            masks[s] = sliding_attack(deltas, s, new Bitboard(0)) & ~edges;

#if X64
            shifts[s] = (uint)(64 - Bitcount.popcount_Max15(masks[s]));
#else
            shifts[s] = (uint) (32 - Bitcount.popcount_Max15(masks[s]));
#endif

            // Use Carry-Rippler trick to enumerate all subsets of masks[s] and
            // store the corresponding sliding attack bitboard in reference[].
            var b = new Bitboard(0);
            var size = 0;
            do
            {
                occupancy[size] = b;
                reference[size] = sliding_attack(deltas, s, b);

                // if (HasPext)
                // attacks[s][pext(b, masks[s])] = reference[size];

                size++;
                b = (b - masks[s]) & masks[s];
            } while (b);

            // Set the offset for the table of the next square. We have individual
            // table sizes for each square with "Fancy Magic Bitboards".
            attacks[s] = new Bitboard[size];

            // if (HasPext)
            //  continue;

#if X64
            var rng = new PRNG((ulong)seeds[1][s.rank_of()]);
#else
            var rng = new PRNG((ulong) seeds[0][Square.rank_of(s)]);
#endif

            // Find a magic for square 's' picking up an (almost) random number
            // until we find the one that passes the verification test.
            do
            {
                do magics[s] = new Bitboard(rng.sparse_rand()); while (
                    Bitcount.popcount_Max15((magics[s]*masks[s]) >> 56) < 6);

                Array.Clear(attacks[s], 0, size);

                // A good magic must map every possible occupancy to an index that
                // looks up the correct sliding attack in the attacks[s] database.
                // Note that we build up the database for square 's' as a side
                // effect of verifying the magic.
                for (++current, i = 0; i < size; ++i)
                {
                    var idx = index(s, occupancy[i]);

                    if (age[idx] < current)
                    {
                        age[idx] = current;
                        attacks[s][idx] = reference[i];
                    }
                    else if (attacks[s][idx] != reference[i])
                    {
                        break;
                    }
                }
            } while (i < size);
        }
    }
}