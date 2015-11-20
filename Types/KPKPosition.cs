using System.Runtime.CompilerServices;

#if PRIMITIVE
using ColorT = System.Int32;
using SquareT = System.Int32;
#endif

internal class KPKPosition
{
    private readonly SquareT[] ksq = new SquareT[Color.COLOR_NB];

    private readonly SquareT psq;

    private readonly ColorT us;

    private Result result;

    internal KPKPosition(uint idx)
    {
        ksq[Color.WHITE] = Square.Create(((int)idx >> 0) & 0x3F);
        ksq[Color.BLACK] = Square.Create(((int)idx >> 6) & 0x3F);
        us = Color.Create(((int)idx >> 12) & 0x01);
        psq = Square.make_square(File.Create(((int)idx >> 13) & 0x3), Rank.RANK_7 - Rank.Create(((int)idx >> 15) & 0x7));

        // Check if two pieces are on the same square or if a king can be captured
        if (Utils.distance_Square(ksq[Color.WHITE], ksq[Color.BLACK]) <= 1
            || ksq[Color.WHITE] == psq || ksq[Color.BLACK] == psq
            || (us == Color.WHITE && Bitboard.AndWithSquare(Utils.StepAttacksBB[PieceType.PAWN, psq], ksq[Color.BLACK])!=0))
        {
            result = Result.INVALID;
        }

        // Immediate win if a pawn can be promoted without getting captured
        else if (us == Color.WHITE && Square.rank_of(psq) == Rank.RANK_7
                 && ksq[us] != psq + Square.DELTA_N
                 && (Utils.distance_Square(ksq[Color.opposite(us)], psq + Square.DELTA_N) > 1
                     || Bitboard.AndWithSquare(Utils.StepAttacksBB[PieceType.KING, ksq[us]], (psq + Square.DELTA_N))!=0))
        {
            result = Result.WIN;
        }

        // Immediate draw if it is a stalemate or a king captures undefended pawn
        else if (us == Color.BLACK
                 && (!(Utils.StepAttacksBB[PieceType.KING, ksq[us]]
                       & ~(Utils.StepAttacksBB[PieceType.KING, ksq[Color.opposite(us)]]
                           | Utils.StepAttacksBB[PieceType.PAWN, psq]))
                     || (Bitboard.AndWithSquare(Utils.StepAttacksBB[PieceType.KING, ksq[us]], psq)
                         & ~Utils.StepAttacksBB[PieceType.KING, ksq[Color.opposite(us)]])))
        {
            result = Result.DRAW;
        }

        // Position will be classified later
        else
        {
            result = Result.UNKNOWN;
        }
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static implicit operator Result(KPKPosition position)
    {
        return position.result;
    }
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Result classify(KPKPosition[] db)
    {
        return us == Color.WHITE ? classify(Color.WHITE, db) : classify(Color.BLACK, db);
    }

    internal Result classify(ColorT Us, KPKPosition[] db)
    {
        // White to move: If one move leads to a position classified as WIN, the result
        // of the current position is WIN. If all moves lead to positions classified
        // as DRAW, the current position is classified as DRAW, otherwise the current
        // position is classified as UNKNOWN.
        //
        // Black to move: If one move leads to a position classified as DRAW, the result
        // of the current position is DRAW. If all moves lead to positions classified
        // as WIN, the position is classified as WIN, otherwise the current position is
        // classified as UNKNOWN.

        var Them = (Us == Color.WHITE ? Color.BLACK : Color.WHITE);
        var Good = (Us == Color.WHITE ? Result.WIN : Result.DRAW);
        var Bad = (Us == Color.WHITE ? Result.DRAW : Result.WIN);

        var r = Result.INVALID;
        var b = Utils.StepAttacksBB[PieceType.KING, ksq[Us]];

        while (b)
        {
            r |= Us == Color.WHITE
                ? db[Bitbases.index(Them, ksq[Them], Utils.pop_lsb(ref b), psq)]
                : db[Bitbases.index(Them, Utils.pop_lsb(ref b), ksq[Them], psq)];
        }

        if (Us == Color.WHITE)
        {
            if (Square.rank_of(psq) < Rank.RANK_7) // Single push
            {
                r |= db[Bitbases.index(Them, ksq[Them], ksq[Us], psq + Square.DELTA_N)];
            }

            if (Square.rank_of(psq) == Rank.RANK_2 // Double push
                && psq + Square.DELTA_N != ksq[Us] && psq + Square.DELTA_N != ksq[Them])
            {
                r |= db[Bitbases.index(Them, ksq[Them], ksq[Us], psq + Square.DELTA_N + Square.DELTA_N)];
            }
        }

        return result = (r & Good) != 0 ? Good : (r & Result.UNKNOWN) != 0 ? Result.UNKNOWN : Bad;
    }
};