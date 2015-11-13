using System.Collections.Generic;

/// LimitsType struct stores information sent by GUI about available time to
/// search the current move, maximum depth/time, if we are in analysis mode or
/// if we have to ponder while it's our opponent's turn to move.
internal class LimitsType
{
    internal int[] inc = new int[Color.COLOR_NB_C];

    internal ulong nodes;

    internal int npmsec, movestogo, depth, movetime, mate, infinite;

    internal bool ponder;

    internal List<Move> searchmoves = new List<Move>();

    internal int[] time = new int[Color.COLOR_NB_C];

    internal bool use_time_management()
    {
        return (mate | movetime | depth | (int) nodes | infinite) == 0;
    }
};