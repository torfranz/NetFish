using System.Collections.Generic;

/// LimitsType struct stores information sent by GUI about available time to
/// search the current move, maximum depth/time, if we are in analysis mode or
/// if we have to ponder while it's our opponent's turn to move.
public class LimitsType
{
    public int[] inc = new int[Color.COLOR_NB];
    public ulong nodes;
    public bool ponder;
    public int npmsec, movestogo, depth, movetime, mate, infinite;
    public List<Move> searchmoves = new List<Move>();
    public int[] time = new int[Color.COLOR_NB];
};