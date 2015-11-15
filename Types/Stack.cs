using System.Collections.Generic;

#if PRIMITIVE
using ValueT = System.Int32;
#endif

/// Stack struct keeps track of the information we need to remember from nodes
/// shallower and deeper in the tree during the search. Each search thread has
/// its own array of Stack objects, indexed by the current ply.
internal class Stack
{
    internal Move currentMove;

    internal Move excludedMove;

    internal Move killers0;

    internal Move killers1;

    internal int moveCount;

    internal int ply;

    internal List<Move> pv = new List<Move>();

    internal Depth reduction;

    internal bool skipEarlyPruning;

    internal SplitPoint splitPoint;

    internal ValueT staticEval;

    internal Move ttMove;
};