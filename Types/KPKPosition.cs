using System.Runtime.CompilerServices;

public class KPKPosition
{
    private readonly Square[] ksq = new Square[Color.COLOR_NB];

    private Square psq;

    private Result result;

    private readonly Color us;

    public KPKPosition(uint idx)
    {
        this.ksq[Color.WHITE] = new Square((idx >> 0) & 0x3F);
        this.ksq[Color.BLACK] = new Square((idx >> 6) & 0x3F);
        this.us = new Color((idx >> 12) & 0x01);
        this.psq = Square.make_square(new File((idx >> 13) & 0x3), Rank.RANK_7 - new Rank((idx >> 15) & 0x7));

        // Check if two pieces are on the same square or if a king can be captured
        if (Utils.distance_Square(this.ksq[Color.WHITE], this.ksq[Color.BLACK]) <= 1
            || this.ksq[Color.WHITE] == this.psq || this.ksq[Color.BLACK] == this.psq
            || (this.us == Color.WHITE && (Utils.StepAttacksBB[PieceType.PAWN, this.psq] & this.ksq[Color.BLACK])))
        {
            this.result = Result.INVALID;
        }

        // Immediate win if a pawn can be promoted without getting captured
        else if (this.us == Color.WHITE && this.psq.rank_of() == Rank.RANK_7
                 && this.ksq[this.us] != this.psq + Square.DELTA_N
                 && (Utils.distance_Square(this.ksq[~this.us], this.psq + Square.DELTA_N) > 1
                     || (Utils.StepAttacksBB[PieceType.KING, this.ksq[this.us]] & (this.psq + Square.DELTA_N))))
        {
            this.result = Result.WIN;
        }

        // Immediate draw if it is a stalemate or a king captures undefended pawn
        else if (this.us == Color.BLACK
                 && (!(Utils.StepAttacksBB[PieceType.KING, this.ksq[this.us]]
                       & ~(Utils.StepAttacksBB[PieceType.KING, this.ksq[~this.us]]
                           | Utils.StepAttacksBB[PieceType.PAWN, this.psq]))
                     || (Utils.StepAttacksBB[PieceType.KING, this.ksq[this.us]] & this.psq
                         & ~Utils.StepAttacksBB[PieceType.KING, this.ksq[~this.us]])))
        {
            this.result = Result.DRAW;
        }

        // Position will be classified later
        else
        {
            this.result = Result.UNKNOWN;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result(KPKPosition position)
    {
        return position.result;
    }

    public Result classify(KPKPosition[] db)
    {
        return this.us == Color.WHITE ? this.classify(Color.WHITE, db) : this.classify(Color.BLACK, db);
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

        var Them = (Us == Color.WHITE ? Color.BLACK : Color.WHITE);
        var Good = (Us == Color.WHITE ? Result.WIN : Result.DRAW);
        var Bad = (Us == Color.WHITE ? Result.DRAW : Result.WIN);

        var r = Result.INVALID;
        var b = Utils.StepAttacksBB[PieceType.KING, this.ksq[Us]];

        while (b)
        {
            r |= Us == Color.WHITE
                     ? db[Bitbases.index(Them, this.ksq[Them], Utils.pop_lsb(ref b), this.psq)]
                     : db[Bitbases.index(Them, Utils.pop_lsb(ref b), this.ksq[Them], this.psq)];
        }

        if (Us == Color.WHITE)
        {
            if (this.psq.rank_of() < Rank.RANK_7) // Single push
            {
                r |= db[Bitbases.index(Them, this.ksq[Them], this.ksq[Us], this.psq + Square.DELTA_N)];
            }

            if (this.psq.rank_of() == Rank.RANK_2 // Double push
                && this.psq + Square.DELTA_N != this.ksq[Us] && this.psq + Square.DELTA_N != this.ksq[Them])
            {
                r |= db[Bitbases.index(Them, this.ksq[Them], this.ksq[Us], this.psq + Square.DELTA_N + Square.DELTA_N)];
            }
        }

        return this.result = (r & Good) != 0 ? Good : (r & Result.UNKNOWN) != 0 ? Result.UNKNOWN : Bad;
    }
};