
#if PRIMITIVE
using ValueT = System.Int32;
using MoveT = System.Int32;
using DepthT = System.Int32;
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

    internal MoveT move()
    {
        return Move.Create(this.move16);
    }

    internal ValueT value()
    {
        return Value.Create(this.value16);
    }

    internal ValueT eval()
    {
        return Value.Create(this.eval16);
    }

    internal DepthT depth()
    {
        return Depth.Create(this.depth8);
    }

    internal Bound bound()
    {
        return (Bound)((this.genBound8 & 0x3));
    }

    internal void save(ulong k, ValueT v, Bound b, DepthT d, MoveT m, ValueT ev, byte g)
    {
        // Preserve any existing move for the same position
        if ((m != 0) || (k >> 48) != this.key16)
        {
            this.move16 = (ushort)m;
        }

        // Don't overwrite more valuable entries
        if ((k >> 48) != this.key16 || d > this.depth8 - 2
            /* || g != (genBound8 & 0xFC) // Matching non-zero keys are already refreshed by probe() */
            || b == Bound.BOUND_EXACT)
        {
            this.key16 = (ushort)(k >> 48);
            this.value16 = (short)v;
            this.eval16 = (short)ev;
            this.genBound8 = (byte)(g | (int)b);
            this.depth8 = d;
        }
    }
}