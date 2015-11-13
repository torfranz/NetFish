using System;

/// The TimeManagement class computes the optimal time to think depending on
/// the maximum available time, the game move number and other parameters.
internal static class TimeManagement
{
    private const int MoveHorizon = 50; // Plan time management at most this many moves ahead

    private const double MaxRatio = 7.0; // When in trouble, we can step over reserved time with this ratio

    private const double StealRatio = 0.33; // However we must not steal time from remaining moves over this ratio

    internal static DateTime start;

    internal static int optimumTime;

    internal static int maximumTime;

    internal static double unstablePvFactor;

    internal static long availableNodes; // When in 'nodes as time' mode

    // move_importance() is a skew-logistic function based on naive statistical
    // analysis of "how many games are still undecided after n half-moves". Game
    // is considered "undecided" as long as neither side has >275cp advantage.
    // Data was extracted from CCRL game database with some simple filtering criteria.

    private static double move_importance(int ply)
    {
        const double XScale = 9.3;
        const double XShift = 59.8;
        const double Skew = 0.172;

        return Math.Pow((1 + Math.Exp((ply - XShift)/XScale)), -Skew) + _.DBL_MIN; // Ensure non-zero
    }

    private static int remaining(TimeType T, int myTime, int movesToGo, int ply, int slowMover)
    {
        var TMaxRatio = (T == TimeType.OptimumTime ? 1 : MaxRatio);
        var TStealRatio = (T == TimeType.OptimumTime ? 0 : StealRatio);

        var moveImportance = (move_importance(ply)*slowMover)/100;
        double otherMovesImportance = 0;

        for (var i = 1; i < movesToGo; ++i)
        {
            otherMovesImportance += move_importance(ply + 2*i);
        }

        var ratio1 = (TMaxRatio*moveImportance)/(TMaxRatio*moveImportance + otherMovesImportance);
        var ratio2 = (moveImportance + TStealRatio*otherMovesImportance)/(moveImportance + otherMovesImportance);

        return (int) (myTime*Math.Min(ratio1, ratio2)); // Intel C++ asks an explicit cast
    }

    /// init() is called at the beginning of the search and calculates the allowed
    /// thinking time out of the time control and current game ply. We support four
    /// different kinds of time controls, passed in 'limits':
    /// 
    /// inc == 0 && movestogo == 0 means: x basetime  [sudden death!]
    /// inc == 0 && movestogo != 0 means: x moves in y minutes
    /// inc >  0 && movestogo == 0 means: x basetime + z increment
    /// inc >  0 && movestogo != 0 means: x moves in y minutes + z increment
    internal static void init(LimitsType limits, Color us, int ply, DateTime now)
    {
        var minThinkingTime = int.Parse(OptionMap.Instance["Minimum Thinking Time"].v);
        var moveOverhead = int.Parse(OptionMap.Instance["Move Overhead"].v);
        var slowMover = int.Parse(OptionMap.Instance["Slow Mover"].v);
        var npmsec = int.Parse(OptionMap.Instance["nodestime"].v);

        // If we have to play in 'nodes as time' mode, then convert from time
        // to nodes, and use resulting values in time management formulas.
        // WARNING: Given npms (nodes per millisecond) must be much lower then
        // real engine speed to avoid time losses.
        if (npmsec != 0)
        {
            if (availableNodes == 0) // Only once at game start
            {
                availableNodes = npmsec*limits.time[us.Value]; // Time is in msec
            }

            // Convert from millisecs to nodes
            limits.time[us.Value] = (int) availableNodes;
            limits.inc[us.Value] *= npmsec;
            limits.npmsec = npmsec;
        }

        start = now;
        unstablePvFactor = 1;
        optimumTime = maximumTime = Math.Max(limits.time[us.Value], minThinkingTime);

        var MaxMTG = limits.movestogo != 0 ? Math.Min(limits.movestogo, MoveHorizon) : MoveHorizon;

        // We calculate optimum time usage for different hypothetical "moves to go"-values
        // and choose the minimum of calculated search time values. Usually the greatest
        // hypMTG gives the minimum values.
        for (var hypMTG = 1; hypMTG <= MaxMTG; ++hypMTG)
        {
            // Calculate thinking time for hypothetical "moves to go"-value
            var hypMyTime = limits.time[us.Value] + limits.inc[us.Value] *(hypMTG - 1) - moveOverhead*(2 + Math.Min(hypMTG, 40));

            hypMyTime = Math.Max(hypMyTime, 0);

            var t1 = minThinkingTime + remaining(TimeType.OptimumTime, hypMyTime, hypMTG, ply, slowMover);
            var t2 = minThinkingTime + remaining(TimeType.MaxTime, hypMyTime, hypMTG, ply, slowMover);

            optimumTime = Math.Min(t1, optimumTime);
            maximumTime = Math.Min(t2, maximumTime);
        }

        if (bool.Parse(OptionMap.Instance["Ponder"].v))
        {
            optimumTime += optimumTime/4;
        }

        optimumTime = Math.Min(optimumTime, maximumTime);
    }

    internal static void pv_instability(double bestMoveChanges)
    {
        unstablePvFactor = 1 + bestMoveChanges;
    }

    internal static int available()
    {
        return (int) (optimumTime*unstablePvFactor*0.76);
    }

    internal static int maximum()
    {
        return maximumTime;
    }

    internal static int elapsed()
    {
        return Search.Limits.npmsec != 0
            ? Search.RootPos.nodes_searched()
            : (int) (DateTime.Now - start).TotalMilliseconds;
    }

    private enum TimeType
    {
        OptimumTime,

        MaxTime
    };
};