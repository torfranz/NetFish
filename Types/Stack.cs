/// Stack struct keeps track of the information we need to remember from nodes
/// shallower and deeper in the tree during the search. Each search thread has
/// its own array of Stack objects, indexed by the current ply.
public class Stack
{
    public Move currentMove;

    public Move excludedMove;

    public Move killers0;

    public Move killers1;

    public int moveCount;

    public int ply;

    public Move pv;

    public Depth reduction;

    public bool skipEarlyPruning;

    public SplitPoint splitPoint;

    public Value staticEval;

    public Move ttMove;
};