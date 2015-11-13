using System.Collections.Generic;

internal static class Tablebases
{
    internal static int Hits;

    internal static bool UseRule50;

    internal static int ProbeDepth;

    internal static int Cardinality;


    internal static int MaxCardinality;

    internal static bool RootInTB;

    internal static Value Score;

    internal static bool root_probe(Position rootPos, List<RootMove> rootMoves, Value score)
    {
        return false;
    }

    internal static bool root_probe_wdl(Position rootPos, List<RootMove> rootMoves, Value score)
    {
        return false;
    }

    internal static int probe_wdl(Position pos, ref int found)
    {
        found = 0;
        return 0;
    }
}