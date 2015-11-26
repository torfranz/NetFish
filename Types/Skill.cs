// Skill struct is used to implement strength limiting

using System;

#if PRIMITIVE
using MoveT = System.Int32;
#endif

internal struct Skill
{
    internal Skill(int l)
    {
        this.level = l;
        this.best = Move.MOVE_NONE;
    }

    internal bool enabled()
    {
        return this.level < 20;
    }

    // PRNG sequence should be non-deterministic, so we seed it with the time at init
    private static readonly PRNG rng = new PRNG((ulong)DateTime.Now.Millisecond);

    internal bool time_to_pick(Depth depth)
    {
        return depth / Depth.ONE_PLY == 1 + this.level;
    }

    internal MoveT best_move(int multiPV)
    {
        return this.best != 0 ? this.best : this.pick_best(multiPV);
    }

    // When playing with strength handicap, choose best move among a set of RootMoves
    // using a statistical rule dependent on 'level'. Idea by Heinz van Saanen.

    internal MoveT pick_best(int multiPV)
    {
        // RootMoves are already sorted by score in descending order
        var variance = Math.Min(Search.RootMoves[0].score - Search.RootMoves[multiPV - 1].score, Value.PawnValueMg);
        var weakness = 120 - 2 * this.level;
        int maxScore = -Value.VALUE_INFINITE;

        // Choose best move. For each move score we add two terms both dependent on
        // weakness. One deterministic and bigger for weaker levels, and one random,
        // then we choose the move with the resulting highest score.
        for (var i = 0; i < multiPV; ++i)
        {
            // This is our magic formula
            var push = (weakness * (Search.RootMoves[0].score - Search.RootMoves[i].score)
                        + variance * ((int)rng.rand() % weakness)) / 128;

            if (Search.RootMoves[i].score + push > maxScore)
            {
                maxScore = Search.RootMoves[i].score + push;
                this.best = Search.RootMoves[i].pv[0];
            }
        }
        return this.best;
    }

    private readonly int level;

    private MoveT best;
};