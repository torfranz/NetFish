using System;
using System.Linq;

/// A TranspositionTable consists of a power of 2 number of clusters and each
/// cluster consists of ClusterSize number of TTEntry. Each non-empty entry
/// contains information of exactly one position. The size of a cluster should
/// not be bigger than a cache line size. In case it is less, it should be padded
/// to guarantee always aligned accesses.
public static class TranspositionTable
{
    private const int CacheLineSize = 64;

    private const int ClusterSize = 3;

    private static uint clusterCount;

    private static Cluster[] table;

    //void* mem;
    private static byte generation8; // Size must be not bigger than TTEntry::genBound8

    public static void new_search()
    {
        generation8 += 4;
    } // Lower 2 bits are used by Bound

    public static byte generation()
    {
        return generation8;
    }

    // The lowest order bits of the key are used to get the index of the cluster
    public static Cluster first_entry(ulong key)
    {
        return table[(uint)key & (clusterCount - 1)];
    }

    /// Returns an approximation of the hashtable occupation during a search. The
    /// hash is x permill full, as per UCI protocol.
    public static int hashfull()
    {
        var cnt = 0;
        for (var i = 0; i < 1000 / ClusterSize; i++)
        {
            var cluster = table[i];
            for (var j = 0; j < ClusterSize; j++)
            {
                if ((cluster.entry[j].genBound8 & 0xFC) == generation8)
                {
                    cnt++;
                }
            }
        }
        return cnt;
    }

    /// TranspositionTable::clear() overwrites the entire transposition table
    /// with zeros. It is called whenever the table is resized, or when the
    /// user asks the program to clear the table (from the UCI interface).
    public static void clear()
    {
        for (int idx = 0; idx < table.Count(); idx++)
        {
            table[idx] = new Cluster();
        }
    }

    /// TranspositionTable::resize() sets the size of the transposition table,
    /// measured in megabytes. Transposition table consists of a power of 2 number
    /// of clusters and each cluster consists of ClusterSize number of TTEntry.
    public static void resize(uint mbSize)
    {
        var newClusterCount = mbSize * 1024 * 1024 / 32;

        if (newClusterCount == clusterCount)
        {
            return;
        }

        clusterCount = newClusterCount;

        table = new Cluster[clusterCount + CacheLineSize - 1];
        for (int idx = 0; idx < table.Count(); idx++)
        {
            table[idx]=new Cluster();
        }
    }

    /// TranspositionTable::probe() looks up the current position in the transposition
    /// table. It returns true and a pointer to the TTEntry if the position is found.
    /// Otherwise, it returns false and a pointer to an empty or least valuable TTEntry
    /// to be replaced later. The replace value of an entry is calculated as its depth
    /// minus 8 times its relative age. TTEntry t1 is considered more valuable than
    /// TTEntry t2 if its replace value is greater than that of t2.
    public static TTEntry probe(ulong key, ref bool found)
    {
        var cluster = first_entry(key);
        var key16 = (ushort)(key >> 48); // Use the high 16 bits as key inside the cluster

        for (var i = 0; i < ClusterSize; ++i)
        {
            if (cluster.entry[i].key16 == 0 || cluster.entry[i].key16 == key16)
            {
                if ((cluster.entry[i].genBound8 & 0xFC) != generation8 && cluster.entry[i].key16 != 0)
                {
                    cluster.entry[i].genBound8 = (byte)(generation8 | (int)cluster.entry[i].bound()); // Refresh
                }

                found = cluster.entry[i].key16 != 0;
                return cluster.entry[i];
            }
        }

        // Find an entry to be replaced according to the replacement strategy
        var replace = cluster.entry[0];
        for (var i = 1; i < ClusterSize; ++i)
        {
            // Due to our packed storage format for generation and its cyclic
            // nature we add 259 (256 is the modulus plus 3 to keep the lowest
            // two bound bits from affecting the result) to calculate the entry
            // age correctly even after generation8 overflows into the next cycle.
            if (replace.depth8 - ((259 + generation8 - replace.genBound8) & 0xFC) * 2 * Depth.ONE_PLY
                > cluster.entry[i].depth8
                - ((259 + generation8 - cluster.entry[i].genBound8) & 0xFC) * 2 * Depth.ONE_PLY)
            {
                replace = cluster.entry[i];
            }
        }

        found = false;
        return replace;
    }

    public class Cluster
    {
        public TTEntry[] entry = new [] { new TTEntry(), new TTEntry(), new TTEntry()}; //ClusterSize = 3
    };
};