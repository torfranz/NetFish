using System.Collections.Generic;

/// LimitsType struct stores information sent by GUI about available time to
/// search the current move, maximum depth/time, if we are in analysis mode or
/// if we have to ponder while it's our opponent's turn to move.
#if PRIMITIVE
using MoveT = System.Int32;
#endif
internal class LimitsType
{
    internal int[] inc = new int[Color.COLOR_NB];

    internal ulong nodes;

    internal int npmsec, movestogo, depth, movetime, mate, infinite;

    internal bool ponder;

    internal List<MoveT> searchmoves = new List<MoveT>();

    internal int[] time = new int[Color.COLOR_NB];

    internal bool use_time_management()
    {
        return (this.mate | this.movetime | this.depth | (int)this.nodes | this.infinite) == 0;
    }
};