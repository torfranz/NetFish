using System.Collections.Generic;

/// LimitsType struct stores information sent by GUI about available time to
/// search the current move, maximum depth/time, if we are in analysis mode or
/// if we have to ponder while it's our opponent's turn to move.
public class LimitsType
{
    public int[] inc = new int[Color.COLOR_NB];

    public ulong nodes;

    public int npmsec, movestogo, depth, movetime, mate, infinite;

    public bool ponder;

    public List<Move> searchmoves = new List<Move>();

    public int[] time = new int[Color.COLOR_NB];

    public bool use_time_management()
    {
        return (this.mate | this.movetime | this.depth | (int)this.nodes | this.infinite) != 0;
    }
};