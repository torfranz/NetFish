using System;

internal static class Bitboards
{
    internal static void init()
    {
        for (var s = Square.SQ_A1_C; s <= Square.SQ_H8_C; ++s)
        {
            Utils.SquareBB[s] = new Bitboard(1UL << s);
            Utils.BSFTable[Utils.bsf_index(Utils.SquareBB[s])] = new Square(s);
        }

        for (ulong b = 2; b < 256; ++b)
        {
            Utils.MSBTable[b] = Utils.MSBTable[b - 1] + (!(Bitboard.more_than_one(new Bitboard(b))) ? 1 : 0);
        }

        foreach (var f in File.AllFiles)
        {
            Utils.FileBB[(int)f] = (int)f > File.FILE_A_C ? Utils.FileBB[(int)f - 1] << 1 : Bitboard.FileABB;
        }

        for (var r = Rank.RANK_1_C; r <= Rank.RANK_8_C; ++r)
        {
            Utils.RankBB[r] = r > Rank.RANK_1_C ? Utils.RankBB[r - 1] << 8 : Bitboard.Rank1BB;
        }

        foreach (var f in File.AllFiles)
        {
            Utils.AdjacentFilesBB[(int)f] = ((int)f > File.FILE_A_C ? Utils.FileBB[(int)f - 1] : new Bitboard(0))
                                       | ((int)f < File.FILE_H_C ? Utils.FileBB[(int)f + 1] : new Bitboard(0));
        }

        for (var r = Rank.RANK_1_C; r < Rank.RANK_8_C; ++r)
        {
            var value = (Utils.InFrontBB[Color.BLACK_C, r + 1] = Utils.InFrontBB[Color.BLACK_C, r] | Utils.RankBB[r]);
            Utils.InFrontBB[Color.WHITE_C, r] = ~value;
        }

        for (var c = Color.WHITE_C; c <= Color.BLACK_C; ++c)
        {
            for (var s = Square.SQ_A1; s <= Square.SQ_H8; ++s)
            {
                Utils.ForwardBB[c, (int)s] = Utils.InFrontBB[c, (int)Square.rank_of(s)] & Utils.FileBB[(int)Square.file_of(s)];
                Utils.PawnAttackSpan[c, (int)s] = Utils.InFrontBB[c, (int)Square.rank_of(s)]
                                             & Utils.AdjacentFilesBB[(int)Square.file_of(s)];
                Utils.PassedPawnMask[c, (int)s] = Utils.ForwardBB[c, (int)s] | Utils.PawnAttackSpan[c, (int)s];
            }
        }

        for (var s1 = Square.SQ_A1; s1 <= Square.SQ_H8; ++s1)
        {
            for (var s2 = Square.SQ_A1; s2 <= Square.SQ_H8; ++s2)
            {
                if (s1 != s2)
                {
                    Utils.SquareDistance[(int)s1, (int)s2] = Math.Max(Utils.distance_File(s1, s2), Utils.distance_Rank(s1, s2));
                    Utils.DistanceRingBB[(int)s1, Utils.SquareDistance[(int)s1, (int)s2] - 1] |= s2;
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

        for (var c = Color.WHITE_C; c <= Color.BLACK_C; ++c)
        {
            for (var pt = PieceType.PAWN_C; pt <= PieceType.KING_C; ++pt)
            {
                for (var s = Square.SQ_A1; s <= Square.SQ_H8; ++s)
                {
                    for (var i = 0; steps[pt][i] != 0; ++i)
                    {
                        var to = s + new Square(c == Color.WHITE_C ? steps[pt][i] : -steps[pt][i]);

                        if (to.is_ok() && Utils.distance_Square(s, to) < 3)
                        {
                            Utils.StepAttacksBB[(int)Piece.make_piece(c, PieceType.Create(pt)), (int)s] |= to;
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

        for (var s1 = Square.SQ_A1_C; s1 <= Square.SQ_H8_C; ++s1)
        {
            var s1Square = new Square(s1);
            
            Utils.PseudoAttacks[PieceType.QUEEN_C, s1] =
                Utils.PseudoAttacks[PieceType.BISHOP_C, s1] = Utils.attacks_bb(PieceType.BISHOP, s1Square, new Bitboard(0));
            var bb = Utils.PseudoAttacks[PieceType.ROOK_C, s1] = Utils.attacks_bb(PieceType.ROOK, s1Square, new Bitboard(0));
            Utils.PseudoAttacks[PieceType.QUEEN_C, s1] = Utils.PseudoAttacks[PieceType.QUEEN_C, s1] | bb;

            for (var pc = Piece.W_BISHOP_C; pc <= Piece.W_ROOK_C; ++pc)
            {
                for (var s2 = Square.SQ_A1_C; s2 <= Square.SQ_H8_C; ++s2)
                {
                    var s2Square = new Square(s2);
                    if (!(Utils.PseudoAttacks[pc, s1] & s2Square))
                    {
                        continue;
                    }

                    var piece = Piece.Create(pc);
                    Utils.LineBB[s1, s2] = (Utils.attacks_bb(piece, s1Square, new Bitboard(0))
                                            & Utils.attacks_bb(piece, s2Square, new Bitboard(0))) | s1Square | s2Square;
                    Utils.BetweenBB[s1, s2] = Utils.attacks_bb(piece, s1Square, Utils.SquareBB[s2])
                                              & Utils.attacks_bb(piece, s2Square, Utils.SquareBB[s1]);
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
        int current = 0;

        for (var s = Square.SQ_A1; s <= Square.SQ_H8; ++s)
        {
            // Board edges are not considered in the relevant occupancies
            var edges = ((Bitboard.Rank1BB | Bitboard.Rank8BB) & ~Utils.rank_bb(s)
                              | ((Bitboard.FileABB | Bitboard.FileHBB) & ~Utils.file_bb(s)));

            // Given a square 's', the mask is the bitboard of sliding attacks from
            // 's' computed on an empty board. The index must be big enough to contain
            // all the attacks for each possible subset of the mask and so is 2 power
            // the number of 1s of the mask. Hence we deduce the size of the shift to
            // apply to the 64 or 32 bits word to get the index.
            masks[(int)s] = sliding_attack(deltas, s, new Bitboard(0)) & ~edges;

#if X64
            shifts[(int)s] = (uint) (64 - Bitcount.popcount_Max15(masks[(int)s]));
#else
            shifts[(int)s] = (uint)(32 - Bitcount.popcount_Max15(masks[(int)s]));
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
                b = (b - masks[(int)s]) & masks[(int)s];
            } while (b);

            // Set the offset for the table of the next square. We have individual
            // table sizes for each square with "Fancy Magic Bitboards".
            attacks[(int)s] = new Bitboard[size];

            // if (HasPext)
            //  continue;

#if X64
            var rng = new PRNG((ulong) seeds[1][(int)Square.rank_of(s)]);
#else
            var rng = new PRNG((ulong)seeds[0][(int)Square.rank_of(s)]);
#endif

            // Find a magic for square 's' picking up an (almost) random number
            // until we find the one that passes the verification test.
            int i;
            do
            {
                do
                {
                    magics[(int) s] = new Bitboard(rng.sparse_rand());
                }
                while (Bitcount.popcount_Max15((magics[(int) s]*masks[(int) s]) >> 56) < 6);
            
                Array.Clear(attacks[(int)s], 0, size);

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
                        attacks[(int)s][idx] = reference[i];
                    }
                    else if (attacks[(int)s][idx] != reference[i])
                    {
                        break;
                    }
                }
            } while (i < size);
        }
    }
}