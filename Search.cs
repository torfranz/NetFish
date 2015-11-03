using System;
using System.Collections.Generic;
using System.Diagnostics;

public static class Search
{
    public static SignalsType Signals;
    public static LimitsType Limits;
    public static List<RootMove> RootMoves = new List<RootMove>();
    public static Position RootPos;
    public static StateInfoWrapper SetupStates;

    public static uint PVIdx;
    private static readonly EasyMoveManager EasyMove = new EasyMoveManager();
    private static double BestMoveChanges;
    private static readonly Value[] DrawValue = new Value[Color.COLOR_NB];
    private static readonly HistoryStats History = new HistoryStats();
    private static readonly CounterMovesHistoryStats CounterMovesHistory = new CounterMovesHistoryStats();
    private static readonly MovesStats Countermoves = new MovesStats();

    /// check_time() is called by the timer thread when the timer triggers. It is
    /// used to print debug info and, more importantly, to detect when we are out of
    /// available time and thus stop the search.
    private static DateTime lastInfoTime = DateTime.Now;

    // Futility and reductions lookup tables, initialized at startup
    private static readonly int[,] FutilityMoveCounts = new int[2, 16]; // [improving][depth]
    private static readonly Depth[,,,] Reductions = new Depth[2, 2, 64, 64]; // [pv][improving][depth][moveNumber]

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
                        if ((sp.slavesMask & (1u << idx)) != 0 && ThreadPool.threads[idx].activePosition != null)
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

    /// Search::think() is the external interface to Stockfish's search, and is
    /// called by the main thread when the program receives the UCI 'go' command. It
    /// searches from RootPos and at the end prints the "bestmove" to output.
    public static void think()
    {
        var us = RootPos.side_to_move();
        TimeManagement.init(Limits, us, RootPos.game_ply(), DateTime.Now);

        int contempt = int.Parse(OptionMap.Instance["Contempt"].v)*Value.PawnValueEg/100; // From centipawns
        DrawValue[us] = Value.VALUE_DRAW - contempt;
        DrawValue[~us] = Value.VALUE_DRAW + contempt;

        //TODO: enable Tablebases
        /*
        TB::Hits = 0;
        TB::RootInTB = false;
        TB::UseRule50 = bool.Parse(OptionMap.Instance["Syzygy50MoveRule"].v);
        TB::ProbeDepth = int.Parse(OptionMap.Instance["SyzygyProbeDepth"].v) * Depth.ONE_PLY;
        TB::Cardinality = int.Parse(OptionMap.Instance["SyzygyProbeLimit"].v);

        // Skip TB probing when no TB found: !TBLargest -> !TB::Cardinality
        if (TB::Cardinality > TB::MaxCardinality)
        {
            TB::Cardinality = TB::MaxCardinality;
            TB::ProbeDepth = Depth.DEPTH_ZERO;
        }
        */

        if (RootMoves.Count == 0)
        {
            RootMoves.Add(new RootMove(Move.MOVE_NONE));
            Console.WriteLine(
                $"info depth 0 score {UCI.value(RootPos.checkers() ? -Value.VALUE_MATE : Value.VALUE_DRAW)}");
        }
        else
        {
            //TODO: enable tablebases
            /*
            if (TB::Cardinality >= RootPos.count(PieceType.ALL_PIECES,Color.WHITE)
                                  + RootPos.count(PieceType.ALL_PIECES,Color.BLACK))
            {
                // If the current root position is in the tablebases then RootMoves
                // contains only moves that preserve the draw or win.
                TB::RootInTB = Tablebases::root_probe(RootPos, RootMoves, TB::Score);

                if (TB::RootInTB)
                    TB::Cardinality = 0; // Do not probe tablebases during the search

                else // If DTZ tables are missing, use WDL tables as a fallback
                {
                    // Filter out moves that do not preserve a draw or win
                    TB::RootInTB = Tablebases::root_probe_wdl(RootPos, RootMoves, TB::Score);

                    // Only probe during search if winning
                    if (TB::Score <= Value.VALUE_DRAW)
                        TB::Cardinality = 0;
                }

                if (TB::RootInTB)
                {
                    TB::Hits = RootMoves.Count;

                    if (!TB::UseRule50)
                        TB::Score = TB::Score > Value.VALUE_DRAW ? Value.VALUE_MATE - _.MAX_PLY - 1
                                   : TB::Score < Value.VALUE_DRAW ? -Value.VALUE_MATE + _.MAX_PLY + 1
                                                            : Value.VALUE_DRAW;
                }
                
            }
            */

            foreach (var th in ThreadPool.threads)
            {
                th.maxPly = 0;
                th.notify_one(); // Wake up all the threads
            }

            ThreadPool.timer.run = true;
            ThreadPool.timer.notify_one(); // Start the recurring timer

            id_loop(RootPos); // Let's start searching !

            ThreadPool.timer.run = false;
        }

        // When playing in 'nodes as time' mode, subtract the searched nodes from
        // the available ones before to exit.
        if (Limits.npmsec != 0)
            TimeManagement.availableNodes += Limits.inc[us] - RootPos.nodes_searched();

        // When we reach the maximum depth, we can arrive here without a raise of
        // Signals.stop. However, if we are pondering or in an infinite search,
        // the UCI protocol states that we shouldn't print the best move before the
        // GUI sends a "stop" or "ponderhit" command. We therefore simply wait here
        // until the GUI sends one of those commands (which also raises Signals.stop).
        if (!Signals.stop && (Limits.ponder || Limits.infinite != 0))
        {
            Signals.stopOnPonderhit = true;

            //RootPos.this_thread().wait_for(Signals.stop);
            ThreadHelper.lock_grab(RootPos.this_thread().mutex);

            while (!Signals.stop)
            {
                ThreadHelper.cond_wait(RootPos.this_thread().sleepCondition, RootPos.this_thread().mutex);
            }

            ThreadHelper.lock_release(RootPos.this_thread().mutex);
        }

        Console.Write($"bestmove {UCI.move(RootMoves[0].pv[0], RootPos.is_chess960())}");

        if (RootMoves[0].pv.Count > 1 || RootMoves[0].extract_ponder_from_tt(RootPos))
            Console.Write($" ponder {UCI.move(RootMoves[0].pv[1], RootPos.is_chess960())}");

        Console.WriteLine();
    }

    // id_loop() is the main iterative deepening loop. It calls search() repeatedly
    // with increasing depth until the allocated thinking time has been consumed,
    // user stops the search, or the maximum search depth is reached.

    private static void id_loop(Position pos)
    {
        var ss = new StackArrayWrapper(new Stack[_.MAX_PLY + 4], 2); // To allow referencing (ss-2) and (ss+2)

        Depth depth;
        Value bestValue, alpha, beta, delta;

        var easyMove = EasyMove.get(pos.key());
        EasyMove.clear();

        //TODO: need to memset?
        //std::memset(ss - 2, 0, 5 * sizeof(Stack));

        depth = Depth.DEPTH_ZERO;
        BestMoveChanges = 0;
        bestValue = delta = alpha = -Value.VALUE_INFINITE;
        beta = Value.VALUE_INFINITE;


        //TODO: enable Tablebases
        //TT.new_search();

        var multiPV = uint.Parse(OptionMap.Instance["MultiPV"].v);
        var skill = new Skill(int.Parse(OptionMap.Instance["Skill Level"].v));

        // When playing with strength handicap enable MultiPV search that we will
        // use behind the scenes to retrieve a set of possible moves.
        if (skill.enabled())
            multiPV = Math.Max(multiPV, 4);

        multiPV = (uint) Math.Min(multiPV, RootMoves.Count);

        // Iterative deepening loop until requested to stop or target depth reached;
        while (++depth < Depth.DEPTH_MAX && !Signals.stop && (Limits.depth == 0 || depth <= Limits.depth))
        {
            // Age out PV variability metric
            BestMoveChanges *= 0.5;

            // Save the last iteration's scores before first PV line is searched and
            // all the move scores except the (new) PV are set to -VALUE_INFINITE.
            foreach (var rm in RootMoves)
                rm.previousScore = rm.score;

            // MultiPV loop. We perform a full root search for each PV line
            for (PVIdx = 0; PVIdx < multiPV && !Signals.stop; ++PVIdx)
            {
                // Reset aspiration window starting size
                if (depth >= 5*Depth.ONE_PLY)
                {
                    delta = new Value(16);
                    alpha = new Value(Math.Max(RootMoves[(int) PVIdx].previousScore - delta, -Value.VALUE_INFINITE));
                    beta = new Value(Math.Min(RootMoves[(int) PVIdx].previousScore + delta, Value.VALUE_INFINITE));
                }

                // Start with a small aspiration window and, in the case of a fail
                // high/low, re-search with a bigger window until we're not failing
                // high/low anymore.
                while (true)
                {
                    //TODO: Impl Search.search
                    //bestValue = search(NodeType.Root, false,pos, ss, alpha, beta, depth, false);

                    // Bring the best move to the front. It is critical that sorting
                    // is done with a stable algorithm because all the values but the
                    // first and eventually the new best one are set to -VALUE_INFINITE
                    // and we want to keep the same order for all the moves except the
                    // new PV that goes to the front. Note that in case of MultiPV
                    // search the already searched PV lines are preserved.

                    //TODO: Check for stable sort replacement
                    Utils.stable_sort(RootMoves, (int) PVIdx, RootMoves.Count);
                    //std::stable_sort(RootMoves.begin() + PVIdx, RootMoves.end());

                    // Write PV back to transposition table in case the relevant
                    // entries have been overwritten during the search.
                    for (var i = 0; i <= PVIdx; ++i)
                        RootMoves[i].insert_pv_in_tt(pos);

                    // If search has been stopped break immediately. Sorting and
                    // writing PV back to TT is safe because RootMoves is still
                    // valid, although it refers to previous iteration.
                    if (Signals.stop)
                        break;

                    // When failing high/low give some update (without cluttering
                    // the UI) before a re-search.
                    if (multiPV == 1
                        && (bestValue <= alpha || bestValue >= beta)
                        && TimeManagement.elapsed() > 3000)
                        Console.WriteLine(UCI.pv(pos, depth, alpha, beta));

                    // In case of failing low/high increase aspiration window and
                    // re-search, otherwise exit the loop.
                    if (bestValue <= alpha)
                    {
                        beta = (alpha + beta)/2;
                        alpha = new Value(Math.Max(bestValue - delta, -Value.VALUE_INFINITE));

                        Signals.failedLowAtRoot = true;
                        Signals.stopOnPonderhit = false;
                    }
                    else if (bestValue >= beta)
                    {
                        alpha = (alpha + beta)/2;
                        beta = new Value(Math.Min(bestValue + delta, Value.VALUE_INFINITE));
                    }
                    else
                        break;

                    delta += delta/2;

                    Debug.Assert(alpha >= -Value.VALUE_INFINITE && beta <= Value.VALUE_INFINITE);
                }

                // Sort the PV lines searched so far and update the GUI
                //TODO: Check for stable sort replacement
                Utils.stable_sort(RootMoves, 0, (int) PVIdx + 1);
                //std::stable_sort(RootMoves.begin(), RootMoves.begin() + PVIdx + 1);

                if (Signals.stop)
                    Console.WriteLine($"info nodes {RootPos.nodes_searched()} time {TimeManagement.elapsed()}");

                else if (PVIdx + 1 == multiPV || TimeManagement.elapsed() > 3000)
                    Console.WriteLine(UCI.pv(pos, depth, alpha, beta));
            }

            // If skill level is enabled and time is up, pick a sub-optimal best move
            if (skill.enabled() && skill.time_to_pick(depth))
                skill.pick_best(multiPV);

            // Have we found a "mate in x"?
            if (Limits.mate != 0
                && bestValue >= Value.VALUE_MATE_IN_MAX_PLY
                && Value.VALUE_MATE - bestValue <= 2*Limits.mate)
                Signals.stop = true;

            // Do we have time for the next iteration? Can we stop searching now?
            if (Limits.use_time_management())
            {
                if (!Signals.stop && !Signals.stopOnPonderhit)
                {
                    // Take some extra time if the best move has changed
                    if (depth > 4*Depth.ONE_PLY && multiPV == 1)
                        TimeManagement.pv_instability(BestMoveChanges);

                    // Stop the search if only one legal move is available or all
                    // of the available time has been used or we matched an easyMove
                    // from the previous search and just did a fast verification.
                    if (RootMoves.Count == 1
                        || TimeManagement.elapsed() > TimeManagement.available()
                        || (RootMoves[0].pv[0] == easyMove
                            && BestMoveChanges < 0.03
                            && TimeManagement.elapsed() > TimeManagement.available()/10))
                    {
                        // If we are allowed to ponder do not stop the search now but
                        // keep pondering until the GUI sends "ponderhit" or "stop".
                        if (Limits.ponder)
                            Signals.stopOnPonderhit = true;
                        else
                            Signals.stop = true;
                    }
                }

                if (RootMoves[0].pv.Count >= 3)
                    EasyMove.update(pos, RootMoves[0].pv);
                else
                    EasyMove.clear();
            }
        }

        // Clear any candidate easy move that wasn't stable for the last search
        // iterations; the second condition prevents consecutive fast moves.
        if (EasyMove.stableCnt < 6 || TimeManagement.elapsed() < TimeManagement.available())
            EasyMove.clear();

        // If skill level is enabled, swap best PV line with the sub-optimal one
        if (skill.enabled())
        {
            var foundIdx = RootMoves.FindIndex(move => move == skill.best_move(multiPV));
            Debug.Assert(foundIdx >= 0);
            var rootMove = RootMoves[0];
            RootMoves[0] = RootMoves[foundIdx];
            RootMoves[foundIdx] = rootMove;
        }
    }

    /// Search::perft() is our utility to verify move generation. All the leaf nodes
    /// up to the given depth are generated and counted and the sum returned.
    public static long perft(bool Root, Position pos, Depth depth)
    {
        var st = new StateInfo();
        long cnt, nodes = 0;
        var ci = new CheckInfo(pos);
        var leaf = (depth == 2*Depth.ONE_PLY);

        var ml = new MoveList(GenType.LEGAL, pos);
        for (var index = ml.begin(); index < ml.end(); index++)
        {
            var m = ml.moveList.table[index];
            if (Root && depth <= Depth.ONE_PLY)
            {
                cnt = 1;
                nodes++;
            }
            else
            {
                pos.do_move(m, st, pos.gives_check(m, ci));
                cnt = leaf ? new MoveList(GenType.LEGAL, pos).size() : perft(false, pos, depth - Depth.ONE_PLY);
                nodes += cnt;
                pos.undo_move(m);
            }
            if (Root)
                Console.WriteLine($"{UCI.move(m, pos.is_chess960())}: {cnt}");
        }
        return nodes;
    }

    /// Search::init() is called during startup to initialize various lookup tables
    // Razoring and futility margin based on depth
    private static Value razor_margin(Depth d)
    {
        return new Value(512 + 32*d);
    }

    private static Value futility_margin(Depth d)
    {
        return new Value(200*d);
    }

    private static Depth reduction(bool PvNode, bool i, Depth d, int mn)
    {
        return Reductions[PvNode ? 1 : 0, i ? 1 : 0, Math.Min(d, 63*Depth.ONE_PLY), Math.Min(mn, 63)];
    }

    public static void init()
    {
        double[][] K = {new[] {0.83, 2.25}, new[] {0.50, 3.00}};

        for (var pv = 0; pv <= 1; ++pv)
            for (var imp = 0; imp <= 1; ++imp)
                for (var d = 1; d < 64; ++d)
                    for (var mc = 1; mc < 64; ++mc)
                    {
                        var r = K[pv][0] + Math.Log(d)*Math.Log(mc)/K[pv][1];

                        if (r >= 1.5)
                            Reductions[pv, imp, d, mc] = new Depth((int) (r*Depth.ONE_PLY));

                        // Increase reduction when eval is not improving
                        if (pv == 0 && imp == 0 && Reductions[pv, imp, d, mc] >= 2*Depth.ONE_PLY)
                            Reductions[pv, imp, d, mc] += Depth.ONE_PLY;
                    }

        for (var d = 0; d < 16; ++d)
        {
            FutilityMoveCounts[0, d] = (int) (2.4 + 0.773*Math.Pow(d + 0.00, 1.8));
            FutilityMoveCounts[1, d] = (int) (2.9 + 1.045*Math.Pow(d + 0.49, 1.8));
        }
    }
}