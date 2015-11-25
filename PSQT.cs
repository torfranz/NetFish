
#if PRIMITIVE
using ScoreT = System.Int32;
#endif

internal static class PSQT
{
    // Bonus[PieceType][Square / 2] contains Piece-Square scores. For each piece
    // type on a given square a (middlegame, endgame) score pair is assigned. Table
    // is defined for files A..D and white side: it is symmetric for black side and
    // second half of the files.
    internal static ScoreT[][][] Bonus =
        {
            new[] { new ScoreT[] { } }, new[]
                                            {
                                                // Pawn
                                                new[]
                                                    {
                                                        Score.make_score(0, 0),
                                                        Score.make_score(0, 0),
                                                        Score.make_score(0, 0),
                                                        Score.make_score(0, 0)
                                                    },
                                                new[]
                                                    {
                                                        Score.make_score(-19, 5),
                                                        Score.make_score(1, -4),
                                                        Score.make_score(7, 8),
                                                        Score.make_score(3, -2)
                                                    },
                                                new[]
                                                    {
                                                        Score.make_score(-26, -6),
                                                        Score.make_score(-7, -5),
                                                        Score.make_score(19, 5),
                                                        Score.make_score(24, 4)
                                                    },
                                                new[]
                                                    {
                                                        Score.make_score(-25, 1),
                                                        Score.make_score(-14, 3),
                                                        Score.make_score(16, -8),
                                                        Score.make_score(31, -3)
                                                    },
                                                new[]
                                                    {
                                                        Score.make_score(-14, 6),
                                                        Score.make_score(0, 9),
                                                        Score.make_score(-1, 7),
                                                        Score.make_score(17, -6)
                                                    },
                                                new[]
                                                    {
                                                        Score.make_score(-14, 6),
                                                        Score.make_score(-13, -5),
                                                        Score.make_score(-10, 2),
                                                        Score.make_score(-6, 4)
                                                    },
                                                new[]
                                                    {
                                                        Score.make_score(-12, 1),
                                                        Score.make_score(15, -9),
                                                        Score.make_score(-8, 1),
                                                        Score.make_score(-4, 18)
                                                    },
                                                new[]
                                                    {
                                                        Score.make_score(0, 0),
                                                        Score.make_score(0, 0),
                                                        Score.make_score(0, 0),
                                                        Score.make_score(0, 0)
                                                    }
                                            },
            new[]
                {
                    // Knight
                    new[]
                        {
                            Score.make_score(-143, -97), Score.make_score(-96, -82),
                            Score.make_score(-80, -46), Score.make_score(-73, -14)
                        },
                    new[]
                        {
                            Score.make_score(-83, -69), Score.make_score(-43, -55),
                            Score.make_score(-21, -17), Score.make_score(-10, 9)
                        },
                    new[]
                        {
                            Score.make_score(-71, -50), Score.make_score(-22, -39),
                            Score.make_score(0, -8), Score.make_score(9, 28)
                        },
                    new[]
                        {
                            Score.make_score(-25, -41), Score.make_score(18, -25),
                            Score.make_score(43, 7), Score.make_score(47, 38)
                        },
                    new[]
                        {
                            Score.make_score(-26, -46), Score.make_score(16, -25),
                            Score.make_score(38, 2), Score.make_score(50, 41)
                        },
                    new[]
                        {
                            Score.make_score(-11, -55), Score.make_score(37, -38),
                            Score.make_score(56, -8), Score.make_score(71, 27)
                        },
                    new[]
                        {
                            Score.make_score(-62, -64), Score.make_score(-17, -50),
                            Score.make_score(5, -24), Score.make_score(14, 13)
                        },
                    new[]
                        {
                            Score.make_score(-195, -110), Score.make_score(-66, -90),
                            Score.make_score(-42, -50), Score.make_score(-29, -13)
                        }
                },
            new[]
                {
                    // Bishop
                    new[]
                        {
                            Score.make_score(-54, -68), Score.make_score(-23, -40),
                            Score.make_score(-35, -46), Score.make_score(-44, -28)
                        },
                    new[]
                        {
                            Score.make_score(-30, -43), Score.make_score(10, -17),
                            Score.make_score(2, -23), Score.make_score(-9, -5)
                        },
                    new[]
                        {
                            Score.make_score(-19, -32), Score.make_score(17, -9),
                            Score.make_score(11, -13), Score.make_score(1, 8)
                        },
                    new[]
                        {
                            Score.make_score(-21, -36), Score.make_score(18, -13),
                            Score.make_score(11, -15), Score.make_score(0, 7)
                        },
                    new[]
                        {
                            Score.make_score(-21, -36), Score.make_score(14, -14),
                            Score.make_score(6, -17), Score.make_score(-1, 3)
                        },
                    new[]
                        {
                            Score.make_score(-27, -35), Score.make_score(6, -13),
                            Score.make_score(2, -10), Score.make_score(-8, 1)
                        },
                    new[]
                        {
                            Score.make_score(-33, -44), Score.make_score(7, -21),
                            Score.make_score(-4, -22), Score.make_score(-12, -4)
                        },
                    new[]
                        {
                            Score.make_score(-45, -65), Score.make_score(-21, -42),
                            Score.make_score(-29, -46), Score.make_score(-39, -27)
                        }
                },
            new[]
                {
                    // Rook
                    new[]
                        {
                            Score.make_score(-25, 0), Score.make_score(-16, 0),
                            Score.make_score(-16, 0), Score.make_score(-9, 0)
                        },
                    new[]
                        {
                            Score.make_score(-21, 0), Score.make_score(-8, 0),
                            Score.make_score(-3, 0), Score.make_score(0, 0)
                        },
                    new[]
                        {
                            Score.make_score(-21, 0), Score.make_score(-9, 0),
                            Score.make_score(-4, 0), Score.make_score(2, 0)
                        },
                    new[]
                        {
                            Score.make_score(-22, 0), Score.make_score(-6, 0),
                            Score.make_score(-1, 0), Score.make_score(2, 0)
                        },
                    new[]
                        {
                            Score.make_score(-22, 0), Score.make_score(-7, 0),
                            Score.make_score(0, 0), Score.make_score(1, 0)
                        },
                    new[]
                        {
                            Score.make_score(-21, 0), Score.make_score(-7, 0),
                            Score.make_score(0, 0), Score.make_score(2, 0)
                        },
                    new[]
                        {
                            Score.make_score(-12, 0), Score.make_score(4, 0),
                            Score.make_score(8, 0), Score.make_score(12, 0)
                        },
                    new[]
                        {
                            Score.make_score(-23, 0), Score.make_score(-15, 0),
                            Score.make_score(-11, 0), Score.make_score(-5, 0)
                        }
                },
            new[]
                {
                    // Queen
                    new[]
                        {
                            Score.make_score(0, -70), Score.make_score(-3, -57),
                            Score.make_score(-4, -41), Score.make_score(-1, -29)
                        },
                    new[]
                        {
                            Score.make_score(-4, -58), Score.make_score(6, -30),
                            Score.make_score(9, -21), Score.make_score(8, -4)
                        },
                    new[]
                        {
                            Score.make_score(-2, -39), Score.make_score(6, -17),
                            Score.make_score(9, -7), Score.make_score(9, 5)
                        },
                    new[]
                        {
                            Score.make_score(-1, -29), Score.make_score(8, -5),
                            Score.make_score(10, 9), Score.make_score(7, 17)
                        },
                    new[]
                        {
                            Score.make_score(-3, -27), Score.make_score(9, -5),
                            Score.make_score(8, 10), Score.make_score(7, 23)
                        },
                    new[]
                        {
                            Score.make_score(-2, -40), Score.make_score(6, -16),
                            Score.make_score(8, -11), Score.make_score(10, 3)
                        },
                    new[]
                        {
                            Score.make_score(-2, -54), Score.make_score(7, -30),
                            Score.make_score(7, -21), Score.make_score(6, -7)
                        },
                    new[]
                        {
                            Score.make_score(-1, -75), Score.make_score(-4, -54),
                            Score.make_score(-1, -44), Score.make_score(0, -30)
                        }
                },
            new[]
                {
                    // King
                    new[]
                        {
                            Score.make_score(291, 28), Score.make_score(344, 76),
                            Score.make_score(294, 103), Score.make_score(219, 112)
                        },
                    new[]
                        {
                            Score.make_score(289, 70), Score.make_score(329, 119),
                            Score.make_score(263, 170), Score.make_score(205, 159)
                        },
                    new[]
                        {
                            Score.make_score(226, 109), Score.make_score(271, 164),
                            Score.make_score(202, 195), Score.make_score(136, 191)
                        },
                    new[]
                        {
                            Score.make_score(204, 131), Score.make_score(212, 194),
                            Score.make_score(175, 194), Score.make_score(137, 204)
                        },
                    new[]
                        {
                            Score.make_score(177, 132), Score.make_score(205, 187),
                            Score.make_score(143, 224), Score.make_score(94, 227)
                        },
                    new[]
                        {
                            Score.make_score(147, 118), Score.make_score(188, 178),
                            Score.make_score(113, 199), Score.make_score(70, 197)
                        },
                    new[]
                        {
                            Score.make_score(116, 72), Score.make_score(158, 121),
                            Score.make_score(93, 142), Score.make_score(48, 161)
                        },
                    new[]
                        {
                            Score.make_score(94, 30), Score.make_score(120, 76),
                            Score.make_score(78, 101), Score.make_score(31, 111)
                        }
                }
        };

    internal static ScoreT[,,] psq = new ScoreT[Color.COLOR_NB, PieceType.PIECE_TYPE_NB, Square.SQUARE_NB];

    // init() initializes piece square tables: the white halves of the tables are
    // copied from Bonus[] adding the piece value, then the black halves of the
    // tables are initialized by flipping and changing the sign of the white scores.
    internal static void init()
    {
        foreach (var pt in PieceType.AllPieceTypes)
        {
            var piece = Piece.make_piece(Color.BLACK, pt);
            Value.PieceValue[(int)Phase.MG][piece] = Value.PieceValue[(int)Phase.MG][pt];
            Value.PieceValue[(int)Phase.EG][piece] = Value.PieceValue[(int)Phase.EG][pt];

            var v = Score.make_score(Value.PieceValue[(int)Phase.MG][pt], Value.PieceValue[(int)Phase.EG][pt]);

            for (var s = Square.SQ_A1; s <= Square.SQ_H8; ++s)
            {
                var edgeDistance = Square.file_of(s) < File.FILE_E ? Square.file_of(s) : File.FILE_H - Square.file_of(s);
                psq[Color.BLACK, pt, Square.opposite(s)] =
                    -(psq[Color.WHITE, pt, s] = v + Bonus[pt][Square.rank_of(s)][edgeDistance]);
            }
        }
    }
}