/// The MoveList struct is a simple wrapper around generate(). It sometimes comes
/// in handy to use this class instead of the low level generate() function.
public class MoveList
{
    private readonly ExtMoveArrayWrapper moveList = new ExtMoveArrayWrapper(new ExtMove[_.MAX_MOVES]);

    public MoveList(GenType Type, Position pos)
    {
        Movegen.generate(Type, pos, moveList);
    }

    public int begin()
    {
        return 0;
    }

    public int end()
    {
        return moveList.current;
    }

    public int size()
    {
        return moveList.current;
    }

    public bool contains(Move move)
    {
        for (var idx = 0; idx < moveList.current; idx++)
        {
            if (moveList[idx] == move)
            {
                return true;
            }
        }

        return false;
    }
};