// Skill struct is used to implement strength limiting

using System;

public struct Skill
{
    public Skill(int l)
    {
        level = l;
        best = Move.MOVE_NONE;
    }

    public bool enabled()
    {
        return level < 20;
    }

    // PRNG sequence should be non-deterministic, so we seed it with the time at init
    private static readonly PRNG rng = new PRNG((ulong) DateTime.Now.Millisecond);

    public bool time_to_pick(Depth depth)
    {
        return depth/Depth.ONE_PLY == 1 + level;
    }

    public Move best_move(uint multiPV)
    {
        return best ? best : pick_best(multiPV);
    }

    // When playing with strength handicap, choose best move among a set of RootMoves
    // using a statistical rule dependent on 'level'. Idea by Heinz van Saanen.

    public Move pick_best(uint multiPV)
    {
        // RootMoves are already sorted by score in descending order
        var variance = Math.Min(Search.RootMoves[0].score - Search.RootMoves[(int) multiPV - 1].score, Value.PawnValueMg);
        var weakness = 120 - 2*level;
        int maxScore = -Value.VALUE_INFINITE;

        // Choose best move. For each move score we add two terms both dependent on
        // weakness. One deterministic and bigger for weaker levels, and one random,
        // then we choose the move with the resulting highest score.
        for (var i = 0; i < multiPV; ++i)
        {
            // This is our magic formula
            var push = (weakness*(int) (Search.RootMoves[0].score - Search.RootMoves[i].score)
                        + variance*((int) rng.rand()%weakness))/128;

            if (Search.RootMoves[i].score + push > maxScore)
            {
                maxScore = Search.RootMoves[i].score + push;
                best = Search.RootMoves[i].pv[0];
            }
        }
        return best;
    }

    private readonly int level;

    private Move best;
};