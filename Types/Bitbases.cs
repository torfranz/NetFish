using System.Diagnostics;

public static class Bitbases
{
    // There are 24 possible pawn squares: the first 4 files and ranks from 2 to 7
    private const uint MAX_INDEX = 2 * 24 * 64 * 64; // stm * psq * wksq * bksq = 196608

    // Each uint32_t stores results of 32 positions, one per bit
    public static uint[] KPKBitbase = new uint[MAX_INDEX / 32];

    // A KPK bitbase index is an integer in [0, IndexMax] range
    //
    // Information is mapped in a way that minimizes the number of iterations:
    //
    // bit  0- 5: white king square (from SQ_A1 to SQ_H8)
    // bit  6-11: black king square (from SQ_A1 to SQ_H8)
    // bit    12: side to move (WHITE or BLACK)
    // bit 13-14: white pawn file (from FILE_A to FILE_D)
    // bit 15-17: white pawn RANK_7 - rank (from RANK_7 - RANK_7 to RANK_7 - RANK_2)
    public static uint index(Color us, Square bksq, Square wksq, Square psq)
    {
        return (uint)(wksq | (bksq << 6) | (us << 12) | (psq.file_of() << 13) | ((Rank.RANK_7 - psq.rank_of()) << 15));
    }

    public static bool probe(Square wksq, Square wpsq, Square bksq, Color us)
    {
        Debug.Assert(wpsq.file_of() <= File.FILE_D);

        var idx = index(us, bksq, wksq, wpsq);
        return (KPKBitbase[idx / 32] & (1 << (int)(idx & 0x1F))) != 0;
    }

    public static void init()
    {
        var db = new KPKPosition[MAX_INDEX];
        uint idx, repeat = 1;

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
                repeat |= ((db[idx] == Result.UNKNOWN && db[idx].classify(db) != Result.UNKNOWN)) ? 1u : 0;
            }
        }

        // Map 32 results into one KPKBitbase[] entry
        for (idx = 0; idx < MAX_INDEX; ++idx)
        {
            if (db[idx] == Result.WIN)
            {
                KPKBitbase[idx / 32] |= (uint)(1 << (int)(idx & 0x1F));
            }
        }
    }
}