using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

public class KPKPosition
{
    public KPKPosition(uint idx)
    {
        ksq[Color.WHITE] = new Square((idx >> 0) & 0x3F);
        ksq[Color.BLACK] = new Square((idx >> 6) & 0x3F);
        us = new Color((idx >> 12) & 0x01);
        psq = Square.make_square(new File((idx >> 13) & 0x3), new Rank(Rank.RANK_7) - new Rank((idx >> 15) & 0x7));

        // Check if two pieces are on the same square or if a king can be captured
        if (Utils.distance_Square(ksq[Color.WHITE], ksq[Color.BLACK]) <= 1
            || ksq[Color.WHITE] == psq
            || ksq[Color.BLACK] == psq
            || (us == Color.WHITE && (Utils.StepAttacksBB[PieceType.PAWN, psq] & ksq[Color.BLACK])))
            result = Result.INVALID;

        // Immediate win if a pawn can be promoted without getting captured
        else if (us == Color.WHITE
                 && psq.rank_of() == Rank.RANK_7
                 && ksq[us] != psq + Square.DELTA_N
                 && (Utils.distance_Square(ksq[~us], psq + Square.DELTA_N) > 1
                     || (Utils.StepAttacksBB[PieceType.KING, ksq[us]] & (psq + Square.DELTA_N))))
            result = Result.WIN;

        // Immediate draw if it is a stalemate or a king captures undefended pawn
        else if (us == Color.BLACK
                 && (!(Utils.StepAttacksBB[PieceType.KING, ksq[us]] & ~(Utils.StepAttacksBB[PieceType.KING, ksq[~us]] | Utils.StepAttacksBB[PieceType.PAWN, psq]))
                     || (Utils.StepAttacksBB[PieceType.KING, ksq[us]] & psq & ~Utils.StepAttacksBB[PieceType.KING, ksq[~us]])))
            result = Result.DRAW;

        // Position will be classified later
        else
            result = Result.UNKNOWN;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result(KPKPosition position)
    {
        return position.result;
    }

    public Result classify(KPKPosition[] db)
    {
        return us == Color.WHITE ? classify(new Color(Color.WHITE), db) : classify(new Color(Color.BLACK), db);
    }

    public Result classify(Color Us, KPKPosition[] db)
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

        Color Them = (Us == Color.WHITE ? new Color(Color.BLACK) : new Color(Color.WHITE));
        Result Good = (Us == Color.WHITE ? Result.WIN : Result.DRAW);
        Result Bad = (Us == Color.WHITE ? Result.DRAW : Result.WIN);

        Result r = Result.INVALID;
        Bitboard b = Utils.StepAttacksBB[PieceType.KING, ksq[Us]];

        while (b)
            r |= Us == Color.WHITE ? db[Bitbases.index(Them, ksq[Them], Utils.pop_lsb(ref b), psq)]
                             : db[Bitbases.index(Them, Utils.pop_lsb(ref b), ksq[Them], psq)];

        if (Us == Color.WHITE)
        {
            if (psq.rank_of() < Rank.RANK_7)      // Single push
                r |= db[Bitbases.index(Them, ksq[Them], ksq[Us], psq + Square.DELTA_N)];

            if (psq.rank_of() == Rank.RANK_2   // Double push
                && psq + Square.DELTA_N != ksq[Us]
                && psq + Square.DELTA_N != ksq[Them])
                r |= db[Bitbases.index(Them, ksq[Them], ksq[Us], psq + Square.DELTA_N + Square.DELTA_N)];
        }

        return result = (r & Good) != 0 ? Good : (r & Result.UNKNOWN) != 0 ? Result.UNKNOWN : Bad;
    }

        private Color us;
        private Square[] ksq = new Square[Color.COLOR_NB];
        private Square psq;
        private Result result;
  };