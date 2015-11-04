using System.Collections.Generic;

public static class Tablebases
{
    public static int Hits;

    public static bool UseRule50;

    public static int ProbeDepth;

    public static int Cardinality;


    public static int MaxCardinality;

    public static bool RootInTB;

    public static Value Score;

    public static bool root_probe(Position rootPos, List<RootMove> rootMoves, Value score)
    {
        return false;
    }

    public static bool root_probe_wdl(Position rootPos, List<RootMove> rootMoves, Value score)
    {
        return false;
    }

    public static int probe_wdl(Position pos, ref int found)
    {
        found = 0;
        return 0;
    }
}