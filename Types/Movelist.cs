/// The MoveList struct is a simple wrapper around generate(). It sometimes comes
/// in handy to use this class instead of the low level generate() function.
#if PRIMITIVE
using MoveT = System.Int32;
#endif
internal class MoveList
{
    internal readonly ExtMoveArrayWrapper moveList = new ExtMoveArrayWrapper(new ExtMove[_.MAX_MOVES]);

    internal MoveList(Position pos)
    {
        Movegen.generate_LEGAL(pos, this.moveList);
    }

    internal int begin()
    {
        return 0;
    }

    internal int end()
    {
        return this.moveList.current;
    }

    internal int size()
    {
        return this.moveList.current;
    }

    internal bool contains(MoveT move)
    {
        for (var idx = 0; idx < this.moveList.current; idx++)
        {
            if (this.moveList[idx] == move)
            {
                return true;
            }
        }

        return false;
    }
};