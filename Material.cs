﻿using System.Diagnostics;

#if PRIMITIVE
using ColorT = System.Int32;
#endif
internal class Material
{
    // Polynomial material imbalance parameters

    //                      pair  pawn knight bishop rook queen
    private static readonly int[] Linear = {1756, -164, -1067, -160, 234, -137};

    private static readonly int[][] QuadraticOurs =
    {
        //            OUR PIECES
        // pair pawn knight bishop rook queen
        new[] {0}, // Bishop pair
        new[] {39, 2}, // Pawn
        new[] {35, 271, -4}, // Knight      OUR PIECES
        new[] {0, 105, 4, 0}, // Bishop
        new[] {-27, -2, 46, 100, -141}, // Rook
        new[] {-177, 25, 129, 142, -137, 0} // Queen
    };

    private static readonly int[][] QuadraticTheirs =
    {
        //           THEIR PIECES
        // pair pawn knight bishop rook queen
        new[] {0}, // Bishop pair
        new[] {37, 0}, // Pawn
        new[] {10, 62, 0}, // Knight      OUR PIECES
        new[] {57, 64, 39, 0}, // Bishop
        new[] {50, 40, 23, -22, 0}, // Rook
        new[] {98, 105, -39, 141, 274, 0} // Queen
    };

    // Endgame evaluation and scaling functions are accessed directly and not through
    // the function maps because they correspond to more than one material hash key.
    private static readonly EndgameValue[] EvaluateKXK = {new EndgameKXK(Color.WHITE), new EndgameKXK(Color.BLACK)};

    private static readonly EndgameScaleFactor[] ScaleKBPsK =
    {
        new EndgameKBPsK(Color.WHITE),
        new EndgameKBPsK(Color.BLACK)
    };

    private static readonly EndgameScaleFactor[] ScaleKPKP =
    {
        new EndgameKPKP(Color.WHITE), new EndgameKPKP(Color.BLACK)
    };

    private static readonly EndgameScaleFactor[] ScaleKPsK =
    {
        new EndgameKPsK(Color.WHITE), new EndgameKPsK(Color.BLACK)
    };

    private static readonly EndgameScaleFactor[] ScaleKQKRPs =
    {
        new EndgameKQKRPs(Color.WHITE),
        new EndgameKQKRPs(Color.BLACK)
    };

    // Helper used to detect a given material distribution
    private static bool is_KXK(Position pos, ColorT us)
    {
        return !Bitboard.more_than_one(pos.pieces_Ct(Color.opposite(us))) && pos.non_pawn_material(us) >= Value.RookValueMg;
    }

    private static bool is_KBPsKs(Position pos, ColorT us)
    {
        return pos.non_pawn_material(us) == Value.BishopValueMg && pos.count(PieceType.BISHOP, us) == 1
               && pos.count(PieceType.PAWN, us) >= 1;
    }

    private static bool is_KQKRPs(Position pos, ColorT us)
    {
        return pos.count(PieceType.PAWN, us) == 0 && pos.non_pawn_material(us) == Value.QueenValueMg
               && pos.count(PieceType.QUEEN, us) == 1 && pos.count(PieceType.ROOK, Color.opposite(us)) == 1
               && pos.count(PieceType.PAWN, Color.opposite(us)) >= 1;
    }

    /// imbalance() calculates the imbalance by comparing the piece count of each
    /// piece type for both colors.
    private static int imbalance(ColorT Us, int[][] pieceCount)
    {
        var Them = (Us == Color.WHITE ? Color.BLACK : Color.WHITE);

        var bonus = 0;

        // Second-degree polynomial material imbalance by Tord Romstad
        for (int pt1 = PieceType.NO_PIECE_TYPE; pt1 <= PieceType.QUEEN; ++pt1)
        {
            if (pieceCount[Us][pt1] == 0)
            {
                continue;
            }

            var v = Linear[pt1];

            for (int pt2 = PieceType.NO_PIECE_TYPE; pt2 <= pt1; ++pt2)
            {
                v += QuadraticOurs[pt1][pt2]*pieceCount[Us][pt2] + QuadraticTheirs[pt1][pt2]*pieceCount[Them][pt2];
            }

            bonus += pieceCount[Us][pt1]*v;
        }

        return bonus;
    }

    /// Material::probe() looks up the current position's material configuration in
    /// the material hash table. It returns a pointer to the Entry if the position
    /// is found. Otherwise a new Entry is computed and stored there, so we don't
    /// have to recompute all when the same material configuration occurs again.
    internal static MaterialEntry probe(Position pos)
    {
        var key = pos.material_key();
        MaterialEntry e;
        if (!pos.this_thread().materialTable.TryGetValue(key, out e))
        {
            e = new MaterialEntry();
            pos.this_thread().materialTable.Add(key, e);
        }
        else if (e.key == key)
        {
            return e;
        }

        e.reset();

        e.key = key;
        e.factor[Color.WHITE] = e.factor[Color.BLACK] = (ushort) ScaleFactor.SCALE_FACTOR_NORMAL;
        e.gamePhase = pos.game_phase();

        // Let's look if we have a specialized evaluation function for this particular
        // material configuration. Firstly we look for a fixed configuration one, then
        // for a generic one if the previous search failed.
        if ((e.evaluationFunction = pos.this_thread().endgames.probeEndgameValue(key)) != null)
        {
            return e;
        }

        foreach (var c in Color.AllColors)
        {
            if (is_KXK(pos, c))
            {
                e.evaluationFunction = EvaluateKXK[c];
                return e;
            }
        }

        // OK, we didn't find any special evaluation function for the current material
        // configuration. Is there a suitable specialized scaling function?
        EndgameScaleFactor sf;

        if ((sf = pos.this_thread().endgames.probeEndgameScaleFactor(key)) != null)
        {
            e.scalingFunction[sf.strong_side()] = sf; // Only strong color assigned
            return e;
        }

        // We didn't find any specialized scaling function, so fall back on generic
        // ones that refer to more than one material distribution. Note that in this
        // case we don't return after setting the function.
        foreach (var c in Color.AllColors)
        {
            if (is_KBPsKs(pos, c))
            {
                e.scalingFunction[c] = ScaleKBPsK[c];
            }
            else if (is_KQKRPs(pos, c))
            {
                e.scalingFunction[c] = ScaleKQKRPs[c];
            }
        }

        var npm_w = pos.non_pawn_material(Color.WHITE);
        var npm_b = pos.non_pawn_material(Color.BLACK);

        if (npm_w + npm_b == Value.VALUE_ZERO && (pos.pieces_Pt(PieceType.PAWN) != 0)) // Only pawns on the board
        {
            if (pos.count(PieceType.PAWN, Color.BLACK) == 0)
            {
                Debug.Assert(pos.count(PieceType.PAWN, Color.WHITE) >= 2);

                e.scalingFunction[Color.WHITE] = ScaleKPsK[Color.WHITE];
            }
            else if (pos.count(PieceType.PAWN, Color.WHITE) == 0)
            {
                Debug.Assert(pos.count(PieceType.PAWN, Color.BLACK) >= 2);

                e.scalingFunction[Color.BLACK] = ScaleKPsK[Color.BLACK];
            }
            else if (pos.count(PieceType.PAWN, Color.WHITE) == 1 && pos.count(PieceType.PAWN, Color.BLACK) == 1)
            {
                // This is a special case because we set scaling functions
                // for both colors instead of only one.
                e.scalingFunction[Color.WHITE] = ScaleKPKP[Color.WHITE];
                e.scalingFunction[Color.BLACK] = ScaleKPKP[Color.BLACK];
            }
        }

        // Zero or just one pawn makes it difficult to win, even with a small material
        // advantage. This catches some trivial draws like KK, KBK and KNK and gives a
        // drawish scale factor for cases such as KRKBP and KmmKm (except for KBBKN).
        if (pos.count(PieceType.PAWN, Color.WHITE) == 0 && npm_w - npm_b <= Value.BishopValueMg)
        {
            e.factor[Color.WHITE] =
                (ushort)
                    (npm_w < Value.RookValueMg
                        ? (ushort) ScaleFactor.SCALE_FACTOR_DRAW
                        : npm_b <= Value.BishopValueMg ? 4 : 14);
        }

        if (pos.count(PieceType.PAWN, Color.BLACK) == 0 && npm_b - npm_w <= Value.BishopValueMg)
        {
            e.factor[Color.BLACK] =
                (ushort)
                    (npm_b < Value.RookValueMg
                        ? (ushort) ScaleFactor.SCALE_FACTOR_DRAW
                        : npm_w <= Value.BishopValueMg ? 4 : 14);
        }

        if (pos.count(PieceType.PAWN, Color.WHITE) == 1 && npm_w - npm_b <= Value.BishopValueMg)
        {
            e.factor[Color.WHITE] = (ushort) ScaleFactor.SCALE_FACTOR_ONEPAWN;
        }

        if (pos.count(PieceType.PAWN, Color.BLACK) == 1 && npm_b - npm_w <= Value.BishopValueMg)
        {
            e.factor[Color.BLACK] = (ushort) ScaleFactor.SCALE_FACTOR_ONEPAWN;
        }

        // Evaluate the material imbalance. We use PIECE_TYPE_NONE as a place holder
        // for the bishop pair "extended piece", which allows us to be more flexible
        // in defining bishop pair bonuses.
        int[][] PieceCount =
        {
            new[]
            {
                pos.count(PieceType.BISHOP, Color.WHITE) > 1 ? 1 : 0,
                pos.count(PieceType.PAWN, Color.WHITE), pos.count(PieceType.KNIGHT, Color.WHITE),
                pos.count(PieceType.BISHOP, Color.WHITE), pos.count(PieceType.ROOK, Color.WHITE),
                pos.count(PieceType.QUEEN, Color.WHITE)
            },
            new[]
            {
                pos.count(PieceType.BISHOP, Color.BLACK) > 1 ? 1 : 0,
                pos.count(PieceType.PAWN, Color.BLACK), pos.count(PieceType.KNIGHT, Color.BLACK),
                pos.count(PieceType.BISHOP, Color.BLACK), pos.count(PieceType.ROOK, Color.BLACK),
                pos.count(PieceType.QUEEN, Color.BLACK)
            }
        };

        e.value = (short) ((imbalance(Color.WHITE, PieceCount) - imbalance(Color.BLACK, PieceCount))/16);
        return e;
    }
}