using System.Collections.Generic;

#if PRIMITIVE
using ValueT = System.Int32;
using MoveT = System.Int32;
using DepthT = System.Int32;
#endif

/// Stack struct keeps track of the information we need to remember from nodes
/// shallower and deeper in the tree during the search. Each search thread has
/// its own array of Stack objects, indexed by the current ply.
internal class Stack
{
    internal MoveT currentMove;

    internal MoveT excludedMove;

    internal MoveT killers0;

    internal MoveT killers1;

    internal int moveCount;

    internal int ply;

    internal List<MoveT> pv = new List<MoveT>();

    internal DepthT reduction;

    internal bool skipEarlyPruning;

    internal SplitPoint splitPoint;

    internal ValueT staticEval;

    internal MoveT ttMove;

    internal Stack()
    {
    }

    internal Stack(Stack other)
    {
        currentMove = other.currentMove;
        excludedMove = other.excludedMove;
        killers0 = other.killers0;
        killers1 = other.killers1;
        moveCount = other.moveCount;
        ply = other.ply;
        pv = other.pv.ConvertAll(item => item);
        reduction = other.reduction;
        skipEarlyPruning = other.skipEarlyPruning;
        staticEval = other.staticEval;
        ttMove = other.ttMove;
    }
};