/// TTEntry struct is the 10 bytes transposition table entry, defined as below:
/// 
/// key        16 bit
/// move       16 bit
/// value      16 bit
/// eval value 16 bit
/// generation  6 bit
/// bound type  2 bit
/// depth       8 bit
public struct TTEntry
{
    public Move move()
    {
        return new Move(this.move16);
    }

    public Value value()
    {
        return new Value(this.value16);
    }

    public Value eval()
    {
        return new Value(this.eval16);
    }

    public Depth depth()
    {
        return new Depth(this.depth8);
    }

    public Bound bound()
    {
        return (Bound)((this.genBound8 & 0x3));
    }

    public void save(ulong k, Value v, Bound b, Depth d, Move m, Value ev, byte g)
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
            this.depth8 = (byte)d;
        }
    }

    public ushort key16;

    private ushort move16;

    private short value16;

    private short eval16;

    public byte genBound8;

    public byte depth8;
}