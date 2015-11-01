using System;
using System.Collections.Generic;

public static class Search
{
    public static SignalsType Signals;
    public static LimitsType Limits;
    public static List<RootMove> RootMoves = new List<RootMove>();
    public static Position RootPos;
    public static StateInfoWrapper SetupStates;

    static uint PVIdx;
    static EasyMoveManager EasyMove = new EasyMoveManager();
    static double BestMoveChanges;
    static Value[] DrawValue = new Value[Color.COLOR_NB];
    static HistoryStats History = new HistoryStats();
    static CounterMovesHistoryStats CounterMovesHistory = new CounterMovesHistoryStats();
    static MovesStats Countermoves;

    /// check_time() is called by the timer thread when the timer triggers. It is
    /// used to print debug info and, more importantly, to detect when we are out of
    /// available time and thus stop the search.
    private static DateTime lastInfoTime = DateTime.Now;

    public static void check_time()
    {
        var elapsed = (DateTime.Now - lastInfoTime).Milliseconds;

        if (elapsed >= 1000)
        {
            lastInfoTime = DateTime.Now;
            //TODO: enable db_print?
            //dbg_print();
        }

        // An engine may not stop pondering until told so by the GUI
        if (Limits.ponder)
            return;

        if (Limits.use_time_management())
        {
            var stillAtFirstMove = Signals.firstRootMove
                                   && !Signals.failedLowAtRoot
                                   && elapsed > TimeManagement.available()*75/100;

            if (stillAtFirstMove
                || elapsed > TimeManagement.maximum() - 2*TimerThread.Resolution)
                Signals.stop = true;
        }
        else if (Limits.movetime != 0 && elapsed >= Limits.movetime)
            Signals.stop = true;

        else if (Limits.nodes != 0)
        {
            long nodes = RootPos.nodes_searched();

            // Loop across all split points and sum accumulated SplitPoint nodes plus
            // all the currently active positions nodes.
            // FIXME: Racy...
            foreach (var th in ThreadPool.threads)
                for (var i = 0; i < th.splitPointsSize; ++i)
                {
                    var sp = th.splitPoints[i];

                    ThreadHelper.lock_grab(sp.spinlock);

                    nodes += sp.nodes;

                    for (var idx = 0; idx < ThreadPool.threads.Count; ++idx)
                        if ((sp.slavesMask & (1u << idx)) != 0 && ThreadPool.threads[idx].activePosition !=null)
                            nodes += ThreadPool.threads[idx].activePosition.nodes_searched();

                    ThreadHelper.lock_release(sp.spinlock);
                }

            if (nodes >= (long) Limits.nodes)
                Signals.stop = true;
        }
    }

    /// Search::reset() clears all search memory, to obtain reproducible search results

    public static void reset()
    {
        //enable TT.clear call
        //TT.clear();
        History.clear();
        CounterMovesHistory.clear();
        Countermoves.clear();
    }
}