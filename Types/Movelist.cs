/// The MoveList struct is a simple wrapper around generate(). It sometimes comes
/// in handy to use this class instead of the low level generate() function.
public class MoveList
{
    private readonly int last;

    private readonly ExtMove[] moveList = new ExtMove[_.MAX_MOVES];

    public MoveList(GenType Type, Position pos)
    {
        Movegen.generate(Type, pos, this.moveList, ref this.last);
    }

    public int begin()
    {
        return 0;
    }

    public int end()
    {
        return this.last;
    }

    public int size()
    {
        return this.last;
    }

    public bool contains(Move move)
    {
        for (var idx = 0; idx < this.last; idx++)
        {
            if (this.moveList[idx] == move)
            {
                return true;
            }
        }

        return false;
    }
};