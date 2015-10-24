/// Stack struct keeps track of the information we need to remember from nodes
/// shallower and deeper in the tree during the search. Each search thread has
/// its own array of Stack objects, indexed by the current ply.
public class Stack
{
    public Move currentMove;
    private Move excludedMove;
    public Move killers0;
    public Move killers1;
    private int moveCount;
    private int ply;
    public SplitPoint splitPoint;
    private Move pv;
    private Depth reduction;
    private bool skipEarlyPruning;
    private Value staticEval;
    private Move ttMove;
};