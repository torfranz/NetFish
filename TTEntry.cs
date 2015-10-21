using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public Move move() { return new Move(move16); }
    public Value value() { return new Value(value16); }
    public Value eval()  { return new Value(eval16); }
    Depth depth() { return new Depth(depth8); }
    public Bound bound() { return (Bound)((genBound8 & 0x3)); }

    public void save(ulong k, Value v, Bound b, Depth d, Move m, Value ev, byte g)
    {

        // Preserve any existing move for the same position
        if ((m != 0) || (k >> 48) != key16)
            move16 = (ushort)m;

        // Don't overwrite more valuable entries
        if ((k >> 48) != key16
            || d > depth8 - 2
            /* || g != (genBound8 & 0xFC) // Matching non-zero keys are already refreshed by probe() */
            || b == Bound.BOUND_EXACT)
        {
            key16 = (ushort)(k >> 48);
            value16 = (short)v;
            eval16 = (short)ev;
            genBound8 = (byte)(g | (int)b);
            depth8 = (byte)d;
        }
    }

    public ushort key16;
    ushort move16;
    short value16;
    short eval16;
    public byte genBound8;
    public byte depth8;
}

