/// The MoveList struct is a simple wrapper around generate(). It sometimes comes
/// in handy to use this class instead of the low level generate() function.

#if PRIMITIVE
using MoveT = System.Int32;
#endif
internal class MoveList
{
    internal readonly ExtMoveArrayWrapper moveList = new ExtMoveArrayWrapper(new ExtMove[_.MAX_MOVES]);

    internal MoveList(GenType Type, Position pos)
    {
        Movegen.generate(Type, pos, moveList);
    }

    internal int begin()
    {
        return 0;
    }

    internal int end()
    {
        return moveList.current;
    }

    internal int size()
    {
        return moveList.current;
    }

    internal bool contains(MoveT move)
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