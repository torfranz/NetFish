#if PRIMITIVE
using ValueT = System.Int32;
#endif

/// TTEntry struct is the 10 bytes transposition table entry, defined as below:
/// 
/// key        16 bit
/// move       16 bit
/// value      16 bit
/// eval value 16 bit
/// generation  6 bit
/// bound type  2 bit
/// depth       8 bit
internal class TTEntry
{
    internal int depth8;

    private short eval16;

    internal byte genBound8;

    internal ushort key16;

    private ushort move16;

    private short value16;

    internal Move move()
    {
        return new Move(move16);
    }

    internal ValueT value()
    {
        return Value.Create(value16);
    }

    internal ValueT eval()
    {
        return Value.Create(eval16);
    }

    internal Depth depth()
    {
        return new Depth(depth8);
    }

    internal Bound bound()
    {
        return (Bound) ((genBound8 & 0x3));
    }

    internal void save(ulong k, ValueT v, Bound b, Depth d, Move m, ValueT ev, byte g)
    {
        // Preserve any existing move for the same position
        if ((m != 0) || (k >> 48) != key16)
        {
            move16 = (ushort) m;
        }

        // Don't overwrite more valuable entries
        if ((k >> 48) != key16 || d > depth8 - 2
            /* || g != (genBound8 & 0xFC) // Matching non-zero keys are already refreshed by probe() */
            || b == Bound.BOUND_EXACT)
        {
            key16 = (ushort) (k >> 48);
            value16 = (short) v;
            eval16 = (short) ev;
            genBound8 = (byte) (g | (int) b);
            depth8 = d;
        }
    }
}