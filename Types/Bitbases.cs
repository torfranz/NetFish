using System.Diagnostics;

#if PRIMITIVE
using ColorT = System.Int32;
using SquareT = System.Int32;
#endif
internal static class Bitbases
{
    // There are 24 possible pawn squares: the first 4 files and ranks from 2 to 7
    private const int MAX_INDEX = 2*24*64*64; // stm * psq * wksq * bksq = 196608

    // Each uint32_t stores results of 32 positions, one per bit
    internal static int[] KPKBitbase = new int[MAX_INDEX/32];

    // A KPK bitbase index is an integer in [0, IndexMax] range
    //
    // Information is mapped in a way that minimizes the number of iterations:
    //
    // bit  0- 5: white king square (from SQ_A1 to SQ_H8)
    // bit  6-11: black king square (from SQ_A1 to SQ_H8)
    // bit    12: side to move (WHITE or BLACK)
    // bit 13-14: white pawn file (from FILE_A to FILE_D)
    // bit 15-17: white pawn RANK_7 - rank (from RANK_7 - RANK_7 to RANK_7 - RANK_2)
    internal static int index(ColorT us, SquareT bksq, SquareT wksq, SquareT psq)
    {
        return (wksq | (bksq << 6) | (us << 12) | (Square.file_of(psq) << 13) |
                 ((Rank.RANK_7 - Square.rank_of(psq)) << 15));
    }

    internal static bool probe(SquareT wksq, SquareT wpsq, SquareT bksq, ColorT us)
    {
        Debug.Assert(Square.file_of(wpsq) <= File.FILE_D);

        var idx = index(us, bksq, wksq, wpsq);
        return (KPKBitbase[idx/32] & (1 << (int) (idx & 0x1F))) != 0;
    }

    internal static void init()
    {
        var db = new KPKPosition[MAX_INDEX];
        int idx, repeat = 1;

        // Initialize db with known win / draw positions
        for (idx = 0; idx < MAX_INDEX; ++idx)
        {
            db[idx] = new KPKPosition(idx);
        }

        // Iterate through the positions until none of the unknown positions can be
        // changed to either wins or draws (15 cycles needed).
        while (repeat != 0)
        {
            for (repeat = idx = 0; idx < MAX_INDEX; ++idx)
            {
                repeat |= ((db[idx] == Result.UNKNOWN && db[idx].classify(db) != Result.UNKNOWN)) ? 1 : 0;
            }
        }

        // Map 32 results into one KPKBitbase[] entry
        for (idx = 0; idx < MAX_INDEX; ++idx)
        {
            if (db[idx] == Result.WIN)
            {
                KPKBitbase[idx/32] |= (1 << (int) (idx & 0x1F));
            }
        }
    }
}