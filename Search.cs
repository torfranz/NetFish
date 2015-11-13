using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Key = System.UInt64;

internal static class Search
{
    internal static SignalsType Signals;

    internal static LimitsType Limits;

    internal static List<RootMove> RootMoves = new List<RootMove>();

    internal static Position RootPos;

    internal static StateInfoWrapper SetupStates;

    internal static uint PVIdx;

    private static readonly EasyMoveManager EasyMove = new EasyMoveManager();

    private static double BestMoveChanges;

    private static readonly Value[] DrawValue = new Value[Color.COLOR_NB_C];

    private static HistoryStats History = new HistoryStats();

    private static CounterMovesHistoryStats CounterMovesHistory = new CounterMovesHistoryStats();

    private static MovesStats Countermoves = new MovesStats();

    /// check_time() is called by the timer thread when the timer triggers. It is
    /// used to print debug info and, more importantly, to detect when we are out of
    /// available time and thus stop the search.
    
    // Futility and reductions lookup tables, initialized at startup
    private static readonly int[,] FutilityMoveCounts = new int[2, 16]; // [improving][depth]
    private static readonly Depth[,,,] Reductions = new Depth[2, 2, 64, 64]; // [pv][improving][depth][moveNumber]

    internal static void check_time()
    {
        var elapsed = TimeManagement.elapsed();

        if (elapsed >= 1000)
        {
            //TODO: enable db_print?
            //dbg_print();
        }

        // An engine may not stop pondering until told so by the GUI
        if (Limits.ponder)
        {
            return;
        }

        if (Limits.use_time_management())
        {
            var stillAtFirstMove = Signals.firstRootMove && !Signals.failedLowAtRoot
                                   && elapsed > TimeManagement.available()*75/100;

            if (stillAtFirstMove || elapsed > TimeManagement.maximum() - 2*TimerThread.Resolution)
            {
                Signals.stop = true;
            }
        }
        else if (Limits.movetime != 0 && elapsed >= Limits.movetime)
        {
            Signals.stop = true;
        }

        else if (Limits.nodes != 0)
        {
            long nodes = RootPos.nodes_searched();

            // Loop across all split points and sum accumulated SplitPoint nodes plus
            // all the currently active positions nodes.
            // FIXME: Racy...
            foreach (var th in ThreadPool.threads)
            {
                for (var i = 0; i < th.splitPointsSize; ++i)
                {
                    var sp = th.splitPoints[i];

                    ThreadHelper.lock_grab(sp.spinlock);

                    nodes += sp.nodes;

                    nodes = ThreadPool.threads.Where((t, idx) => (sp.slavesMask & (1u << idx)) != 0 && t.activePosition != null).Aggregate(nodes, (current, t) => current + t.activePosition.nodes_searched());

                    ThreadHelper.lock_release(sp.spinlock);
                }
            }

            if (nodes >= (long) Limits.nodes)
            {
                Signals.stop = true;
            }
        }
    }

    /// Search::reset() clears all search memory, to obtain reproducible search results
    internal static void reset()
    {
        TranspositionTable.clear();
        History = new HistoryStats();
        CounterMovesHistory = new CounterMovesHistoryStats();
        Countermoves = new MovesStats();
    }

    /// Search::think() is the external interface to Stockfish's search, and is
    /// called by the main thread when the program receives the UCI 'go' command. It
    /// searches from RootPos and at the end prints the "bestmove" to output.
    internal static void think()
    {
        var us = RootPos.side_to_move();
        TimeManagement.init(Limits, us, RootPos.game_ply(), DateTime.Now);

        int contempt = int.Parse(OptionMap.Instance["Contempt"].v)*Value.PawnValueEg/100; // From centipawns
        DrawValue[us.ValueMe] = Value.VALUE_DRAW - contempt;
        DrawValue[us.ValueThem] = Value.VALUE_DRAW + contempt;

        Tablebases.Hits = 0;
        Tablebases.RootInTB = false;
        Tablebases.UseRule50 = bool.Parse(OptionMap.Instance["Syzygy50MoveRule"].v);
        Tablebases.ProbeDepth = int.Parse(OptionMap.Instance["SyzygyProbeDepth"].v)*Depth.ONE_PLY;
        Tablebases.Cardinality = int.Parse(OptionMap.Instance["SyzygyProbeLimit"].v);

        // Skip TB probing when no TB found: !TBLargest . !Tablebases.Cardinality
        if (Tablebases.Cardinality > Tablebases.MaxCardinality)
        {
            Tablebases.Cardinality = Tablebases.MaxCardinality;
            Tablebases.ProbeDepth = Depth.DEPTH_ZERO;
        }

        if (RootMoves.Count == 0)
        {
            RootMoves.Add(new RootMove(Move.MOVE_NONE));
            Console.WriteLine(
                $"info depth 0 score {UCI.value(RootPos.checkers() ? -Value.VALUE_MATE : Value.VALUE_DRAW)}");
        }
        else
        {
            if (Tablebases.Cardinality >= RootPos.count(PieceType.ALL_PIECES, Color.WHITE)
                + RootPos.count(PieceType.ALL_PIECES, Color.BLACK))
            {
                // If the current root position is in the tablebases then RootMoves
                // contains only moves that preserve the draw or win.
                Tablebases.RootInTB = Tablebases.root_probe(RootPos, RootMoves, Tablebases.Score);

                if (Tablebases.RootInTB)
                    Tablebases.Cardinality = 0; // Do not probe tablebases during the search

                else // If DTZ tables are missing, use WDL tables as a fallback
                {
                    // Filter out moves that do not preserve a draw or win
                    Tablebases.RootInTB = Tablebases.root_probe_wdl(RootPos, RootMoves, Tablebases.Score);

                    // Only probe during search if winning
                    if (Tablebases.Score <= Value.VALUE_DRAW)
                        Tablebases.Cardinality = 0;
                }

                if (Tablebases.RootInTB)
                {
                    Tablebases.Hits = RootMoves.Count;

                    if (!Tablebases.UseRule50)
                        Tablebases.Score = Tablebases.Score > Value.VALUE_DRAW
                            ? Value.VALUE_MATE - _.MAX_PLY - 1
                            : Tablebases.Score < Value.VALUE_DRAW
                                ? -Value.VALUE_MATE + _.MAX_PLY + 1
                                : Value.VALUE_DRAW;
                }
            }

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
        {
            TimeManagement.availableNodes += Limits.inc[us.ValueMe] - RootPos.nodes_searched();
        }

        // When we reach the maximum depth, we can arrive here without a raise of
        // Signals.stop. However, if we are pondering or in an infinite search,
        // the UCI protocol states that we shouldn't print the best move before the
        // GUI sends a "stop" or "ponderhit" command. We therefore simply wait here
        // until the GUI sends one of those commands (which also raises Signals.stop).
        if (!Signals.stop && (Limits.ponder || Limits.infinite != 0))
        {
            Signals.stopOnPonderhit = true;

            //RootPos.this_thread().wait_for(Signals.stop);
            ThreadHelper.lock_grab(RootPos.this_thread().spinlock /*mutex*/);

            while (!Signals.stop)
            {
                ThreadHelper.cond_wait(RootPos.this_thread().sleepCondition, RootPos.this_thread().spinlock /*mutex*/);
            }

            ThreadHelper.lock_release(RootPos.this_thread().spinlock /*mutex*/);
        }

        Console.Write($"bestmove {UCI.move(RootMoves[0].pv[0], RootPos.is_chess960())}");

        if (RootMoves[0].pv.Count > 1 || RootMoves[0].extract_ponder_from_tt(RootPos))
        {
            Console.Write($" ponder {UCI.move(RootMoves[0].pv[1], RootPos.is_chess960())}");
        }

        Console.WriteLine();
    }

    // id_loop() is the main iterative deepening loop. It calls search() repeatedly
    // with increasing depth until the allocated thinking time has been consumed,
    // user stops the search, or the maximum search depth is reached.

    private static void id_loop(Position pos)
    {
        var stack = new Stack[_.MAX_PLY + 4];
        for (var idx = 0; idx < stack.Length; idx++)
        {
            stack[idx] = new Stack();
        }

        var ss = new StackArrayWrapper(stack, 2); // To allow referencing (ss-2) and (ss+2)

        Value alpha, delta;

        var easyMove = EasyMove.get(pos.key());
        EasyMove.clear();

        //TODO: need to memset?
        //Math.Memset(ss - 2, 0, 5 * sizeof(Stack));

        var depth = Depth.DEPTH_ZERO;
        BestMoveChanges = 0;
        var bestValue = delta = alpha = -Value.VALUE_INFINITE;
        var beta = Value.VALUE_INFINITE;

        TranspositionTable.new_search();

        var multiPV = uint.Parse(OptionMap.Instance["MultiPV"].v);
        var skill = new Skill(int.Parse(OptionMap.Instance["Skill Level"].v));

        // When playing with strength handicap enable MultiPV search that we will
        // use behind the scenes to retrieve a set of possible moves.
        if (skill.enabled())
        {
            multiPV = Math.Max(multiPV, 4);
            multiPV = Math.Max(multiPV, 4);
            multiPV = Math.Max(multiPV, 4);
        }

        multiPV = (uint) Math.Min(multiPV, RootMoves.Count);

        // Iterative deepening loop until requested to stop or target depth reached;
        while (++depth < Depth.DEPTH_MAX && !Signals.stop && (Limits.depth == 0 || depth <= Limits.depth))
        {
            // Age out PV variability metric
            BestMoveChanges *= 0.5;

            // Save the last iteration's scores before first PV line is searched and
            // all the move scores except the (new) PV are set to -VALUE_INFINITE.
            foreach (var rm in RootMoves)
            {
                rm.previousScore = rm.score;
            }

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
                    bestValue = search(NodeType.Root, false, pos, ss, alpha, beta, depth, false);

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
                    {
                        RootMoves[i].insert_pv_in_tt(pos);
                    }

                    // If search has been stopped break immediately. Sorting and
                    // writing PV back to TT is safe because RootMoves is still
                    // valid, although it refers to previous iteration.
                    if (Signals.stop)
                    {
                        break;
                    }

                    // When failing high/low give some update (without cluttering
                    // the UI) before a re-search.
                    if (multiPV == 1 && (bestValue <= alpha || bestValue >= beta) && TimeManagement.elapsed() > 3000)
                    {
                        Console.WriteLine(UCI.pv(pos, depth, alpha, beta));
                    }

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
                    {
                        break;
                    }

                    delta += delta/2;

                    Debug.Assert(alpha >= -Value.VALUE_INFINITE && beta <= Value.VALUE_INFINITE);
                }

                // Sort the PV lines searched so far and update the GUI
                //TODO: Check for stable sort replacement
                Utils.stable_sort(RootMoves, 0, (int) PVIdx + 1);
                //std::stable_sort(RootMoves.begin(), RootMoves.begin() + PVIdx + 1);

                if (Signals.stop)
                {
                    Console.WriteLine($"info nodes {RootPos.nodes_searched()} time {TimeManagement.elapsed()}");
                }

                else if (PVIdx + 1 == multiPV || TimeManagement.elapsed() > 3000)
                {
                    Console.WriteLine(UCI.pv(pos, depth, alpha, beta));
                }
            }

            // If skill level is enabled and time is up, pick a sub-optimal best move
            if (skill.enabled() && skill.time_to_pick(depth))
            {
                skill.pick_best(multiPV);
            }

            // Have we found a "mate in x"?
            if (Limits.mate != 0 && bestValue >= Value.VALUE_MATE_IN_MAX_PLY
                && Value.VALUE_MATE - bestValue <= 2*Limits.mate)
            {
                Signals.stop = true;
            }

            // Do we have time for the next iteration? Can we stop searching now?
            if (Limits.use_time_management())
            {
                if (!Signals.stop && !Signals.stopOnPonderhit)
                {
                    // Take some extra time if the best move has changed
                    if (depth > 4*Depth.ONE_PLY && multiPV == 1)
                    {
                        TimeManagement.pv_instability(BestMoveChanges);
                    }

                    // Stop the search if only one legal move is available or all
                    // of the available time has been used or we matched an easyMove
                    // from the previous search and just did a fast verification.
                    if (RootMoves.Count == 1 || TimeManagement.elapsed() > TimeManagement.available()
                        || (RootMoves[0].pv[0] == easyMove && BestMoveChanges < 0.03
                            && TimeManagement.elapsed() > TimeManagement.available()/10))
                    {
                        // If we are allowed to ponder do not stop the search now but
                        // keep pondering until the GUI sends "ponderhit" or "stop".
                        if (Limits.ponder)
                        {
                            Signals.stopOnPonderhit = true;
                        }
                        else
                        {
                            Signals.stop = true;
                        }
                    }
                }

                if (RootMoves[0].pv.Count >= 3)
                {
                    EasyMove.update(pos, RootMoves[0].pv);
                }
                else
                {
                    EasyMove.clear();
                }
            }
        }

        // Clear any candidate easy move that wasn't stable for the last search
        // iterations; the second condition prevents consecutive fast moves.
        if (EasyMove.stableCnt < 6 || TimeManagement.elapsed() < TimeManagement.available())
        {
            EasyMove.clear();
        }

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
    internal static long perft(bool Root, Position pos, Depth depth)
    {
        var st = new StateInfo();
        long nodes = 0;
        var ci = new CheckInfo(pos);
        var leaf = (depth == 2*Depth.ONE_PLY);

        var ml = new MoveList(GenType.LEGAL, pos);
        for (var index = ml.begin(); index < ml.end(); index++)
        {
            var m = ml.moveList.table[index];
            long cnt;
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
            {
                Console.WriteLine($"{UCI.move(m, pos.is_chess960())}: {cnt}");
            }
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

    internal static void init()
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

    // value_from_tt() is the inverse of value_to_tt(): It adjusts a mate score
    // from the transposition table (which refers to the plies to mate/be mated
    // from current position) to "plies to mate/be mated from the root".

    private static Value value_from_tt(Value v, int ply)
    {
        return v == Value.VALUE_NONE
            ? Value.VALUE_NONE
            : v >= Value.VALUE_MATE_IN_MAX_PLY
                ? v - ply
                : v <= Value.VALUE_MATED_IN_MAX_PLY ? v + ply : v;
    }

    // search<>() is the main search function for both PV and non-PV nodes and for
    // normal and SplitPoint nodes. When called just after a split point the search
    // is simpler because we have already probed the hash table, done a null move
    // search, and searched the first move before splitting, so we don't have to
    // repeat all this work again. We also don't need to store anything to the hash
    // table here: This is taken care of after we return from the split point.

    private static Value search(NodeType NT, bool SpNode, Position pos, StackArrayWrapper ss, Value alpha, Value beta,
        Depth depth, bool cutNode)
    {
        Utils.WriteToLog(
            $"search(NT={(int) NT}, SpNode={(SpNode ? 1 : 0)}, pos={pos.key()}, ss, alpha={alpha}, beta={beta}, depth={(int) depth}, cutNode={(cutNode ? 1 : 0)})");
        var RootNode = NT == NodeType.Root;
        var PvNode = NT == NodeType.PV || NT == NodeType.Root;

        Debug.Assert(-Value.VALUE_INFINITE <= alpha && alpha < beta && beta <= Value.VALUE_INFINITE);
        Debug.Assert(PvNode || (alpha == beta - 1));
        Debug.Assert(depth > Depth.DEPTH_ZERO);

        var pv = new List<Move>(_.MAX_PLY + 1) {Move.MOVE_NONE};
        var quietsSearched = new Move[64];
        var st = new StateInfo();
        TTEntry tte;
        SplitPoint splitPoint = null;
        ulong posKey = 0;
        Move ttMove, move, excludedMove, bestMove;
        Value bestValue, value, ttValue, eval;
        bool ttHit;
        int moveCount = 0, quietCount = 0;

        // Step 1. Initialize node
        var thisThread = pos.this_thread();
        bool inCheck = pos.checkers();

        if (SpNode)
        {
            splitPoint = ss[ss.current].splitPoint;
            bestMove = new Move(splitPoint.bestMove);
            bestValue = new Value(splitPoint.bestValue);
            tte = new TTEntry();
            ttMove = excludedMove = Move.MOVE_NONE;
            ttValue = Value.VALUE_NONE;

            Debug.Assert(splitPoint.bestValue > -Value.VALUE_INFINITE && splitPoint.moveCount > 0);

            goto moves_loop;
        }

        moveCount = quietCount = ss[ss.current].moveCount = 0;
        bestValue = -Value.VALUE_INFINITE;
        ss[ss.current].ply = ss[ss.current - 1].ply + 1;

        // Used to send selDepth info to GUI
        if (PvNode && thisThread.maxPly < ss[ss.current].ply)
            thisThread.maxPly = ss[ss.current].ply;

        if (!RootNode)
        {
            // Step 2. Check for aborted search and immediate draw
            if (Signals.stop || pos.is_draw() || ss[ss.current].ply >= _.MAX_PLY)
                return ss[ss.current].ply >= _.MAX_PLY && !inCheck
                    ? Eval.evaluate(false, pos)
                    : DrawValue[pos.side_to_move().ValueMe];

            // Step 3. Mate distance pruning. Even if we mate at the next move our score
            // would be at best mate_in(ss.ply+1), but if alpha is already bigger because
            // a shorter mate was found upward in the tree then there is no need to search
            // because we will never beat the current alpha. Same logic but with reversed
            // signs applies also in the opposite condition of being mated instead of giving
            // mate. In this case return a fail-high score.
            alpha = new Value(Math.Max(Value.mated_in(ss[ss.current].ply), alpha));
            beta = new Value(Math.Min(Value.mate_in(ss[ss.current].ply + 1), beta));
            if (alpha >= beta)
                return alpha;
        }

        Debug.Assert(0 <= ss[ss.current].ply && ss[ss.current].ply < _.MAX_PLY);

        ss[ss.current].currentMove = ss[ss.current].ttMove = ss[ss.current + 1].excludedMove = bestMove = Move.MOVE_NONE;
        ss[ss.current + 1].skipEarlyPruning = false;
        ss[ss.current + 1].reduction = Depth.DEPTH_ZERO;
        ss[ss.current + 2].killers0 = ss[ss.current + 2].killers1 = Move.MOVE_NONE;

        // Step 4. Transposition table lookup
        // We don't want the score of a partial search to overwrite a previous full search
        // TT value, so we use a different position key in case of an excluded move.
        excludedMove = ss[ss.current].excludedMove;
        posKey = excludedMove ? pos.exclusion_key() : pos.key();
        tte = TranspositionTable.probe(posKey, out ttHit);
        ss[ss.current].ttMove = ttMove = RootNode ? RootMoves[(int) PVIdx].pv[0] : ttHit ? tte.move() : Move.MOVE_NONE;
        ttValue = ttHit ? value_from_tt(tte.value(), ss[ss.current].ply) : Value.VALUE_NONE;

        // At non-PV nodes we check for a fail high/low. We don't prune at PV nodes
        if (!PvNode
            && ttHit
            && tte.depth() >= depth
            && ttValue != Value.VALUE_NONE // Only in case of TT access race
            && (ttValue >= beta
                ? (tte.bound() & Bound.BOUND_LOWER) != 0
                : (tte.bound() & Bound.BOUND_UPPER) != 0))
        {
            ss[ss.current].currentMove = ttMove; // Can be Move.MOVE_NONE

            // If ttMove is quiet, update killers, history, counter move on TT hit
            if (ttValue >= beta && ttMove && !pos.capture_or_promotion(ttMove))
                update_stats(pos, ss, ttMove, depth, null, 0);

            return ttValue;
        }

        // Step 4a. Tablebase probe
        if (!RootNode && Tablebases.Cardinality != 0)
        {
            var piecesCnt = pos.count(PieceType.ALL_PIECES, Color.WHITE) + pos.count(PieceType.ALL_PIECES, Color.BLACK);

            if (piecesCnt <= Tablebases.Cardinality
                && (piecesCnt < Tablebases.Cardinality || depth >= Tablebases.ProbeDepth)
                && pos.rule50_count() == 0)
            {
                var found = 0;
                var v = Tablebases.probe_wdl(pos, ref found);

                if (found != 0)
                {
                    Tablebases.Hits++;

                    var drawScore = Tablebases.UseRule50 ? 1 : 0;

                    value = v < -drawScore
                        ? -Value.VALUE_MATE + _.MAX_PLY + ss[ss.current].ply
                        : v > drawScore
                            ? Value.VALUE_MATE - _.MAX_PLY - ss[ss.current].ply
                            : Value.VALUE_DRAW + 2*v*drawScore;

                    tte.save(posKey, value_to_tt(value, ss[ss.current].ply), Bound.BOUND_EXACT,
                        new Depth(Math.Min(Depth.DEPTH_MAX - Depth.ONE_PLY, depth + 6*Depth.ONE_PLY)),
                        Move.MOVE_NONE, Value.VALUE_NONE, TranspositionTable.generation());

                    return value;
                }
            }
        }

        // Step 5. Evaluate the position statically
        if (inCheck)
        {
            ss[ss.current].staticEval = Value.VALUE_NONE;
            goto moves_loop;
        }

        if (ttHit)
        {
            // Never assume anything on values stored in TT
            if ((ss[ss.current].staticEval = eval = tte.eval()) == Value.VALUE_NONE)
                eval = ss[ss.current].staticEval = Eval.evaluate(false, pos);

            // Can ttValue be used as a better position evaluation?
            if (ttValue != Value.VALUE_NONE)
                if ((tte.bound() & (ttValue > eval ? Bound.BOUND_LOWER : Bound.BOUND_UPPER)) != 0)
                    eval = ttValue;
        }
        else
        {
            eval = ss[ss.current].staticEval =
                ss[ss.current - 1].currentMove != Move.MOVE_NULL
                    ? Eval.evaluate(false, pos)
                    : -ss[ss.current - 1].staticEval + 2*Eval.Tempo;

            tte.save(posKey, Value.VALUE_NONE, Bound.BOUND_NONE, Depth.DEPTH_NONE, Move.MOVE_NONE,
                ss[ss.current].staticEval, TranspositionTable.generation());
        }

        if (ss[ss.current].skipEarlyPruning)
            goto moves_loop;

        // Step 6. Razoring (skipped when in check)
        if (!PvNode
            && depth < 4*Depth.ONE_PLY
            && eval + razor_margin(depth) <= alpha
            && ttMove == Move.MOVE_NONE)
        {
            if (depth <= Depth.ONE_PLY
                && eval + razor_margin(3*Depth.ONE_PLY) <= alpha)
                return qsearch(NodeType.NonPV, false, pos, ss, alpha, beta, Depth.DEPTH_ZERO);

            var ralpha = alpha - razor_margin(depth);
            var v = qsearch(NodeType.NonPV, false, pos, ss, ralpha, ralpha + 1, Depth.DEPTH_ZERO);
            if (v <= ralpha)
                return v;
        }

        // Step 7. Futility pruning: child node (skipped when in check)
        if (!RootNode
            && depth < 7*Depth.ONE_PLY
            && eval - futility_margin(depth) >= beta
            && eval < Value.VALUE_KNOWN_WIN // Do not return unproven wins
            && pos.non_pawn_material(pos.side_to_move()))
            return eval - futility_margin(depth);

        // Step 8. Null move search with verification search (is omitted in PV nodes)
        if (!PvNode
            && depth >= 2*Depth.ONE_PLY
            && eval >= beta
            && pos.non_pawn_material(pos.side_to_move()))
        {
            ss[ss.current].currentMove = Move.MOVE_NULL;

            Debug.Assert(eval - beta >= 0);

            // Null move dynamic reduction based on depth and value
            var R = ((823 + 67*depth)/256 + Math.Min((eval - beta)/Value.PawnValueMg, 3))*(int) Depth.ONE_PLY;

            pos.do_null_move(st);
            ss[ss.current + 1].skipEarlyPruning = true;
            var nullValue = depth - R < Depth.ONE_PLY
                ? -qsearch(NodeType.NonPV, false, pos, new StackArrayWrapper(ss.table, ss.current + 1), -beta, -beta + 1,
                    Depth.DEPTH_ZERO)
                : -search(NodeType.NonPV, false, pos, new StackArrayWrapper(ss.table, ss.current + 1), -beta, -beta + 1,
                    depth - R, !cutNode);
            ss[ss.current + 1].skipEarlyPruning = false;
            pos.undo_null_move();

            if (nullValue >= beta)
            {
                // Do not return unproven mate scores
                if (nullValue >= Value.VALUE_MATE_IN_MAX_PLY)
                    nullValue = beta;

                if (depth < 12*Depth.ONE_PLY && Math.Abs(beta) < Value.VALUE_KNOWN_WIN)
                    return nullValue;

                // Do verification search at high depths
                ss[ss.current].skipEarlyPruning = true;
                var v = depth - R < Depth.ONE_PLY
                    ? qsearch(NodeType.NonPV, false, pos, ss, beta - 1, beta, Depth.DEPTH_ZERO)
                    : search(NodeType.NonPV, false, pos, ss, beta - 1, beta, depth - R, false);
                ss[ss.current].skipEarlyPruning = false;

                if (v >= beta)
                    return nullValue;
            }
        }

        // Step 9. ProbCut (skipped when in check)
        // If we have a very good capture (i.e. SEE > seeValues[captured_piece_type])
        // and a reduced search returns a value much above beta, we can (almost) safely
        // prune the previous move.
        if (!PvNode
            && depth >= 5*Depth.ONE_PLY
            && Math.Abs(beta) < Value.VALUE_MATE_IN_MAX_PLY)
        {
            var rbeta = new Value(Math.Min(beta + 200, Value.VALUE_INFINITE));
            var rdepth = depth - 4*Depth.ONE_PLY;

            Debug.Assert(rdepth >= Depth.ONE_PLY);
            Debug.Assert(ss[ss.current - 1].currentMove != Move.MOVE_NONE);
            Debug.Assert(ss[ss.current - 1].currentMove != Move.MOVE_NULL);

            var mp2 = new MovePicker(pos, ttMove, History, CounterMovesHistory,
                Value.PieceValue[(int) Phase.MG][pos.captured_piece_type()]);
            var ci2 = new CheckInfo(pos);

            while ((move = mp2.next_move(false)) != Move.MOVE_NONE)
                if (pos.legal(move, ci2.pinned))
                {
                    ss[ss.current].currentMove = move;
                    pos.do_move(move, st, pos.gives_check(move, ci2));
                    value =
                        -search(NodeType.NonPV, false, pos, new StackArrayWrapper(ss.table, ss.current + 1), -rbeta,
                            -rbeta + 1, rdepth, !cutNode);
                    pos.undo_move(move);
                    if (value >= rbeta)
                        return value;
                }
        }

        // Step 10. Internal iterative deepening (skipped when in check)
        if (depth >= (PvNode ? 5*Depth.ONE_PLY : 8*Depth.ONE_PLY)
            && !ttMove
            && (PvNode || ss[ss.current].staticEval + 256 >= beta))
        {
            var d = depth - 2*Depth.ONE_PLY - (PvNode ? Depth.DEPTH_ZERO : depth/4);
            ss[ss.current].skipEarlyPruning = true;
            search(PvNode ? NodeType.PV : NodeType.NonPV, false, pos, ss, alpha, beta, d, true);
            ss[ss.current].skipEarlyPruning = false;

            tte = TranspositionTable.probe(posKey, out ttHit);
            ttMove = ttHit ? tte.move() : Move.MOVE_NONE;
        }

        moves_loop: // When in check and at SpNode search starts from here

        var prevMoveSq = Move.to_sq(ss[ss.current - 1].currentMove);
        var countermove = Countermoves.table[pos.piece_on(prevMoveSq), prevMoveSq];

        var mp = new MovePicker(pos, ttMove, depth, History, CounterMovesHistory, countermove, ss);
        var ci = new CheckInfo(pos);
        value = bestValue; // Workaround a bogus 'uninitialized' warning under gcc
        var improving = ss[ss.current].staticEval >= ss[ss.current - 2].staticEval
                         || ss[ss.current].staticEval == Value.VALUE_NONE
                         || ss[ss.current - 2].staticEval == Value.VALUE_NONE;

        var singularExtensionNode = !RootNode
                                     && !SpNode
                                     && depth >= 8*Depth.ONE_PLY
                                     && ttMove != Move.MOVE_NONE
            /*  &&  ttValue != ValueMe.VALUE_NONE Already implicit in the next condition */
                                     && Math.Abs(ttValue) < Value.VALUE_KNOWN_WIN
                                     && !excludedMove // Recursive singular search is not allowed
                                     && ((tte.bound() & Bound.BOUND_LOWER) != 0)
                                     && tte.depth() >= depth - 3*Depth.ONE_PLY;

        // Step 11. Loop through moves
        // Loop through all pseudo-legal moves until no moves remain or a beta cutoff occurs
        while ((move = mp.next_move(SpNode)) != Move.MOVE_NONE)
        {
            Utils.WriteToLog($"mp.next_move = {(int) move}");
            Debug.Assert(Move.is_ok(move));

            if (move == excludedMove)
                continue;

            // At root obey the "searchmoves" option and skip moves not listed in Root
            // Move List. As a consequence any illegal move is also skipped. In MultiPV
            // mode we also skip PV moves which have been already searched.
            if (RootNode && RootMoves.All(rootMove => rootMove != move))
                continue;

            if (SpNode)
            {
                // Shared counter cannot be decremented later if the move turns out to be illegal
                if (!pos.legal(move, ci.pinned))
                    continue;

                ss[ss.current].moveCount = moveCount = ++splitPoint.moveCount;
                ThreadHelper.lock_release(splitPoint.spinlock);
            }
            else
                ss[ss.current].moveCount = ++moveCount;

            if (RootNode)
            {
                Signals.firstRootMove = (moveCount == 1);

                if (thisThread == ThreadPool.main() && TimeManagement.elapsed() > 3000)
                    Console.WriteLine(
                        $"info depth {depth/Depth.ONE_PLY} currmove {UCI.move(move, pos.is_chess960())} currmovenumber {moveCount + PVIdx}");
            }

            if (PvNode)
                ss[ss.current + 1].pv = new List<Move>();

            var extension = Depth.DEPTH_ZERO;
            var captureOrPromotion = pos.capture_or_promotion(move);

            var givesCheck = Move.type_of(move) == MoveType.NORMAL && !ci.dcCandidates
                ? ci.checkSquares[Piece.type_of(pos.piece_on(Move.from_sq(move)))] & Move.to_sq(move)
                : pos.gives_check(move, ci);

            // Step 12. Extend checks
            if (givesCheck && pos.see_sign(move) >= Value.VALUE_ZERO)
                extension = Depth.ONE_PLY;

            // Singular extension search. If all moves but one fail low on a search of
            // (alpha-s, beta-s), and just one fails high on (alpha, beta), then that move
            // is singular and should be extended. To verify this we do a reduced search
            // on all the other moves but the ttMove and if the result is lower than
            // ttValue minus a margin then we extend the ttMove.
            if (singularExtensionNode
                && move == ttMove
                && extension == 0
                && pos.legal(move, ci.pinned))
            {
                var rBeta = ttValue - 2*depth/Depth.ONE_PLY;
                ss[ss.current].excludedMove = move;
                ss[ss.current].skipEarlyPruning = true;
                value = search(NodeType.NonPV, false, pos, ss, rBeta - 1, rBeta, depth/2, cutNode);
                ss[ss.current].skipEarlyPruning = false;
                ss[ss.current].excludedMove = Move.MOVE_NONE;

                if (value < rBeta)
                    extension = Depth.ONE_PLY;
            }

            // Update the current move (this must be done after singular extension search)
            var newDepth = depth - Depth.ONE_PLY + extension;

            // Step 13. Pruning at shallow depth
            if (!RootNode
                && !captureOrPromotion
                && !inCheck
                && !givesCheck
                && !pos.advanced_pawn_push(move)
                && bestValue > Value.VALUE_MATED_IN_MAX_PLY)
            {
                // Move count based pruning
                if (depth < 16*Depth.ONE_PLY
                    && moveCount >= FutilityMoveCounts[improving ? 1 : 0, depth])
                {
                    if (SpNode)
                        ThreadHelper.lock_grab(splitPoint.spinlock);

                    continue;
                }

                var predictedDepth = newDepth - reduction(PvNode, improving, depth, moveCount);

                // Futility pruning: parent node
                if (predictedDepth < 7*Depth.ONE_PLY)
                {
                    var futilityValue = ss[ss.current].staticEval + futility_margin(predictedDepth) + 256;

                    if (futilityValue <= alpha)
                    {
                        bestValue = new Value(Math.Max(bestValue, futilityValue));

                        if (SpNode)
                        {
                            ThreadHelper.lock_grab(splitPoint.spinlock);
                            if (bestValue > splitPoint.bestValue)
                                splitPoint.bestValue = bestValue;
                        }
                        continue;
                    }
                }

                // Prune moves with negative SEE at low depths
                if (predictedDepth < 4*Depth.ONE_PLY && pos.see_sign(move) < Value.VALUE_ZERO)
                {
                    if (SpNode)
                        ThreadHelper.lock_grab(splitPoint.spinlock);

                    continue;
                }
            }

            // Speculative prefetch as early as possible
            //prefetch(TT.first_entry(pos.key_after(move)));

            // Check for legality just before making the move
            if (!RootNode && !SpNode && !pos.legal(move, ci.pinned))
            {
                ss[ss.current].moveCount = --moveCount;
                continue;
            }

            ss[ss.current].currentMove = move;

            // Step 14. Make the move
            pos.do_move(move, st, givesCheck);

            // Step 15. Reduced depth search (LMR). If the move fails high it will be
            // re-searched at full depth.
            bool doFullDepthSearch;
            if (depth >= 3*Depth.ONE_PLY
                && moveCount > 1
                && !captureOrPromotion
                && move != ss[ss.current].killers0
                && move != ss[ss.current].killers1)
            {
                ss[ss.current].reduction = reduction(PvNode, improving, depth, moveCount);

                if ((!PvNode && cutNode)
                    || (History.table[pos.piece_on(Move.to_sq(move)), Move.to_sq(move)] < Value.VALUE_ZERO
                        &&
                        CounterMovesHistory.table[pos.piece_on(prevMoveSq), prevMoveSq].table[
                            pos.piece_on(Move.to_sq(move)), Move.to_sq(move)] <= Value.VALUE_ZERO))
                    ss[ss.current].reduction += Depth.ONE_PLY;

                if (History.table[pos.piece_on(Move.to_sq(move)), Move.to_sq(move)] > Value.VALUE_ZERO
                    &&
                    CounterMovesHistory.table[pos.piece_on(prevMoveSq), prevMoveSq].table[
                        pos.piece_on(Move.to_sq(move)), Move.to_sq(move)] > Value.VALUE_ZERO)
                    ss[ss.current].reduction =
                        new Depth(Math.Max(Depth.DEPTH_ZERO, ss[ss.current].reduction - Depth.ONE_PLY));

                // Decrease reduction for moves that escape a capture
                if (ss[ss.current].reduction > 0
                    && Move.type_of(move) == MoveType.NORMAL
                    && Piece.type_of(pos.piece_on(Move.to_sq(move))) != PieceType.PAWN
                    && pos.see(Move.make_move(Move.to_sq(move), Move.from_sq(move))) < Value.VALUE_ZERO)
                    ss[ss.current].reduction =
                        new Depth(Math.Max(Depth.DEPTH_ZERO, ss[ss.current].reduction - Depth.ONE_PLY));

                var d = new Depth(Math.Max(newDepth - ss[ss.current].reduction, Depth.ONE_PLY));
                if (SpNode)
                    alpha = new Value(splitPoint.alpha);

                value =
                    -search(NodeType.NonPV, false, pos, new StackArrayWrapper(ss.table, ss.current + 1), -(alpha + 1),
                        -alpha, d, true);

                doFullDepthSearch = (value > alpha && ss[ss.current].reduction != Depth.DEPTH_ZERO);
                ss[ss.current].reduction = Depth.DEPTH_ZERO;
            }
            else
                doFullDepthSearch = !PvNode || moveCount > 1;

            // Step 16. Full depth search, when LMR is skipped or fails high
            if (doFullDepthSearch)
            {
                if (SpNode)
                    alpha = new Value(splitPoint.alpha);

                value = newDepth < Depth.ONE_PLY
                    ? givesCheck
                        ? -qsearch(NodeType.NonPV, true, pos, new StackArrayWrapper(ss.table, ss.current + 1),
                            -(alpha + 1), -alpha, Depth.DEPTH_ZERO)
                        : -qsearch(NodeType.NonPV, false, pos, new StackArrayWrapper(ss.table, ss.current + 1),
                            -(alpha + 1), -alpha, Depth.DEPTH_ZERO)
                    : -search(NodeType.NonPV, false, pos, new StackArrayWrapper(ss.table, ss.current + 1), -(alpha + 1),
                        -alpha, newDepth, !cutNode);
            }

            // For PV nodes only, do a full PV search on the first move or after a fail
            // high (in the latter case search only if value < beta), otherwise let the
            // parent node fail low with value <= alpha and to try another move.
            if (PvNode && (moveCount == 1 || (value > alpha && (RootNode || value < beta))))
            {
                ss[ss.current + 1].pv = pv;
                ss[ss.current + 1].pv[0] = Move.MOVE_NONE;

                value = newDepth < Depth.ONE_PLY
                    ? givesCheck
                        ? -qsearch(NodeType.PV, true, pos, new StackArrayWrapper(ss.table, ss.current + 1), -beta,
                            -alpha, Depth.DEPTH_ZERO)
                        : -qsearch(NodeType.PV, false, pos, new StackArrayWrapper(ss.table, ss.current + 1), -beta,
                            -alpha, Depth.DEPTH_ZERO)
                    : -search(NodeType.PV, false, pos, new StackArrayWrapper(ss.table, ss.current + 1), -beta, -alpha,
                        newDepth, false);
            }

            // Step 17. Undo move
            pos.undo_move(move);

            Debug.Assert(value > -Value.VALUE_INFINITE && value < Value.VALUE_INFINITE);

            // Step 18. Check for new best move
            if (SpNode)
            {
                ThreadHelper.lock_grab(splitPoint.spinlock);
                bestValue = new Value(splitPoint.bestValue);
                alpha = new Value(splitPoint.alpha);
            }

            // Finished searching the move. If a stop or a cutoff occurred, the return
            // value of the search cannot be trusted, and we return immediately without
            // updating best move, PV and TT.
            if (Signals.stop || thisThread.cutoff_occurred())
                return Value.VALUE_ZERO;

            if (RootNode)
            {
                var rm = RootMoves.Find(rootmove => rootmove == move);

                // PV move or new best move ?
                if (moveCount == 1 || value > alpha)
                {
                    rm.score = value;

                    var firstEntry = rm.pv[0];
                    rm.pv.Clear();
                    rm.pv.Add(firstEntry);

                    Debug.Assert(ss[ss.current + 1].pv != null);

                    foreach (var m in ss[ss.current + 1].pv.TakeWhile(m => m != Move.MOVE_NONE))
                    {
                        rm.pv.Add(m);
                    }

                    // We record how often the best move has been changed in each
                    // iteration. This information is used for time management: When
                    // the best move changes frequently, we allocate some more time.
                    if (moveCount > 1)
                        ++BestMoveChanges;
                }
                else
                // All other moves but the PV are set to the lowest value: this is
                // not a problem when sorting because the sort is stable and the
                // move position in the list is preserved - just the PV is pushed up.
                    rm.score = -Value.VALUE_INFINITE;
            }

            if (value > bestValue)
            {
                bestValue = new Value(SpNode ? splitPoint.bestValue = value : value);

                if (value > alpha)
                {
                    // If there is an easy move for this position, clear it if unstable
                    if (PvNode
                        && EasyMove.get(pos.key())
                        && (move != EasyMove.get(pos.key()) || moveCount > 1))
                        EasyMove.clear();

                    bestMove = new Move(SpNode ? splitPoint.bestMove = move : move);

                    if (PvNode && !RootNode) // Update pv even in fail-high case
                        update_pv(SpNode ? splitPoint.ss[ss.current].pv : ss[ss.current].pv, move, ss[ss.current + 1].pv);

                    if (PvNode && value < beta) // Update alpha! Always alpha < beta
                        alpha = new Value(SpNode ? splitPoint.alpha = value : value);
                    else
                    {
                        Debug.Assert(value >= beta); // Fail high

                        if (SpNode)
                            splitPoint.cutoff = true;

                        break;
                    }
                }
            }

            if (!SpNode && !captureOrPromotion && move != bestMove && quietCount < 64)
                quietsSearched[quietCount++] = move;

            // Step 19. Check for splitting the search
            if (!SpNode
                && ThreadPool.threads.Count >= 2
                && depth >= ThreadPool.minimumSplitDepth
                && (thisThread.activeSplitPoint == null
                    || !thisThread.activeSplitPoint.allSlavesSearching
                    || (ThreadPool.threads.Count > _.MAX_SLAVES_PER_SPLITPOINT
                        && Bitcount.popcount_Full(thisThread.activeSplitPoint.slavesMask) == _.MAX_SLAVES_PER_SPLITPOINT))
                && thisThread.splitPointsSize < _.MAX_SPLITPOINTS_PER_THREAD)
            {
                Debug.Assert(bestValue > -Value.VALUE_INFINITE && bestValue < beta);

                thisThread.split(pos, ss, alpha, beta, ref bestValue, ref bestMove,
                    depth, moveCount, mp, NT, cutNode);

                if (Signals.stop || thisThread.cutoff_occurred())
                    return Value.VALUE_ZERO;

                if (bestValue >= beta)
                    break;
            }
        }

        if (SpNode)
            return bestValue;

        // Following condition would detect a stop or a cutoff set only after move
        // loop has been completed. But in this case bestValue is valid because we
        // have fully searched our subtree, and we can anyhow save the result in TT.
        /*
           if (Signals.stop || thisThread.cutoff_occurred())
            return VALUE_DRAW;
        */

        // Step 20. Check for mate and stalemate
        // All legal moves have been searched and if there are no legal moves, it
        // must be mate or stalemate. If we are in a singular extension search then
        // return a fail low score.
        if (moveCount == 0)
            bestValue = excludedMove
                ? alpha
                : inCheck ? Value.mated_in(ss[ss.current].ply) : DrawValue[pos.side_to_move().ValueMe];

        // Quiet best move: update killers, history and countermoves
        else if (bestMove && !pos.capture_or_promotion(bestMove))
            update_stats(pos, ss, bestMove, depth, quietsSearched, quietCount);

        // Bonus for prior countermove that caused the fail low
        else if (!bestMove)
        {
            if (Move.is_ok(ss[ss.current - 2].currentMove) && Move.is_ok(ss[ss.current - 1].currentMove) &&
                !pos.captured_piece_type() && !inCheck && depth >= 3*Depth.ONE_PLY)
            {
                var bonus = new Value((depth/Depth.ONE_PLY)*(depth/Depth.ONE_PLY));
                var prevSq = Move.to_sq(ss[ss.current - 1].currentMove);
                var prevPrevSq = Move.to_sq(ss[ss.current - 2].currentMove);
                var flMoveCmh = CounterMovesHistory.table[pos.piece_on(prevPrevSq), prevPrevSq];
                flMoveCmh.updateCMH(pos.piece_on(prevSq), prevSq, bonus);
            }
        }

        tte.save(posKey, value_to_tt(bestValue, ss[ss.current].ply),
            bestValue >= beta
                ? Bound.BOUND_LOWER
                : PvNode && bestMove ? Bound.BOUND_EXACT : Bound.BOUND_UPPER,
            depth, bestMove, ss[ss.current].staticEval, TranspositionTable.generation());

        Debug.Assert(bestValue > -Value.VALUE_INFINITE && bestValue < Value.VALUE_INFINITE);

        return bestValue;
    }

    private static Value qsearch(NodeType NT, bool InCheck, Position pos, StackArrayWrapper ss, Value alpha, Value beta,
        Depth depth)
    {
        Utils.WriteToLog(
            $"qsearch(NT={(int) NT}, InCheck={(InCheck ? 1 : 0)}, pos={pos.key()}, ss, alpha={alpha}, beta={beta}, depth={(int) depth})");
        var PvNode = NT == NodeType.PV;

        Debug.Assert(NT == NodeType.PV || NT == NodeType.NonPV);
        Debug.Assert(InCheck == pos.checkers());
        Debug.Assert(alpha >= -Value.VALUE_INFINITE && alpha < beta && beta <= Value.VALUE_INFINITE);
        Debug.Assert(PvNode || (alpha == beta - 1));
        Debug.Assert(depth <= Depth.DEPTH_ZERO);

        var pv = new List<Move>(_.MAX_MOVES + 1) {Move.MOVE_NONE};
        var st = new StateInfo();
        Move move, bestMove;
        Value bestValue, futilityBase, oldAlpha = new Value(0);
        bool ttHit;

        if (PvNode)
        {
            oldAlpha = alpha; // To flag BOUND_EXACT when eval above alpha and no available moves
            ss[ss.current + 1].pv = pv;
            ss[ss.current].pv[0] = Move.MOVE_NONE;
        }

        ss[ss.current].currentMove = bestMove = Move.MOVE_NONE;
        ss[ss.current].ply = ss[ss.current - 1].ply + 1;

        // Check for an instant draw or if the maximum ply has been reached
        if (pos.is_draw() || ss[ss.current].ply >= _.MAX_PLY)
            return ss[ss.current].ply >= _.MAX_PLY && !InCheck
                ? Eval.evaluate(false, pos)
                : DrawValue[pos.side_to_move().ValueMe];

        Debug.Assert(0 <= ss[ss.current].ply && ss[ss.current].ply < _.MAX_PLY);

        // Decide whether or not to include checks: this fixes also the type of
        // TT entry depth that we are going to use. Note that in qsearch we use
        // only two types of depth in TT: DEPTH_QS_CHECKS or DEPTH_QS_NO_CHECKS.
        var ttDepth = InCheck || depth >= Depth.DEPTH_QS_CHECKS
            ? Depth.DEPTH_QS_CHECKS
            : Depth.DEPTH_QS_NO_CHECKS;

        // Transposition table lookup
        var posKey = pos.key();
        var tte = TranspositionTable.probe(posKey, out ttHit);
        var ttMove = ttHit ? tte.move() : Move.MOVE_NONE;
        var ttValue = ttHit ? value_from_tt(tte.value(), ss[ss.current].ply) : Value.VALUE_NONE;

        if (!PvNode
            && ttHit
            && tte.depth() >= ttDepth
            && ttValue != Value.VALUE_NONE // Only in case of TT access race
            && ((ttValue >= beta ? (tte.bound() & Bound.BOUND_LOWER) : (tte.bound() & Bound.BOUND_UPPER))) != 0)
        {
            ss[ss.current].currentMove = ttMove; // Can be MOVE_NONE
            return ttValue;
        }

        // Evaluate the position statically
        if (InCheck)
        {
            ss[ss.current].staticEval = Value.VALUE_NONE;
            bestValue = futilityBase = -Value.VALUE_INFINITE;
        }
        else
        {
            if (ttHit)
            {
                // Never assume anything on values stored in TT
                if ((ss[ss.current].staticEval = bestValue = tte.eval()) == Value.VALUE_NONE)
                    ss[ss.current].staticEval = bestValue = Eval.evaluate(false, pos);

                // Can ttValue be used as a better position evaluation?
                if (ttValue != Value.VALUE_NONE)
                    if ((tte.bound() & (ttValue > bestValue ? Bound.BOUND_LOWER : Bound.BOUND_UPPER)) != 0)
                        bestValue = ttValue;
            }
            else
                ss[ss.current].staticEval = bestValue =
                    ss[ss.current - 1].currentMove != Move.MOVE_NULL
                        ? Eval.evaluate(false, pos)
                        : -ss[ss.current - 1].staticEval + 2*Eval.Tempo;

            // Stand pat. Return immediately if static value is at least beta
            if (bestValue >= beta)
            {
                if (!ttHit)
                    tte.save(pos.key(), value_to_tt(bestValue, ss[ss.current].ply), Bound.BOUND_LOWER,
                        Depth.DEPTH_NONE, Move.MOVE_NONE, ss[ss.current].staticEval, TranspositionTable.generation());

                return bestValue;
            }

            if (PvNode && bestValue > alpha)
                alpha = bestValue;

            futilityBase = bestValue + 128;
        }

        // Initialize a MovePicker object for the current position, and prepare
        // to search the moves. Because the depth is <= 0 here, only captures,
        // queen promotions and checks (only if depth >= DEPTH_QS_CHECKS) will
        // be generated.
        var mp = new MovePicker(pos, ttMove, depth, History, CounterMovesHistory,
            Move.to_sq(ss[ss.current - 1].currentMove));
        var ci = new CheckInfo(pos);

        // Loop through the moves until no moves remain or a beta cutoff occurs
        while ((move = mp.next_move(false)) != Move.MOVE_NONE)
        {
            Debug.Assert(Move.is_ok(move));

            var givesCheck = Move.type_of(move) == MoveType.NORMAL && !ci.dcCandidates
                ? ci.checkSquares[Piece.type_of(pos.piece_on(Move.from_sq(move)))] & Move.to_sq(move)
                : pos.gives_check(move, ci);

            // Futility pruning
            if (!InCheck
                && !givesCheck
                && futilityBase > -Value.VALUE_KNOWN_WIN
                && !pos.advanced_pawn_push(move))
            {
                Debug.Assert(Move.type_of(move) != MoveType.ENPASSANT); // Due to !pos.advanced_pawn_push

                var futilityValue = futilityBase + Value.PieceValue[(int) Phase.EG][pos.piece_on(Move.to_sq(move))];

                if (futilityValue <= alpha)
                {
                    bestValue = new Value(Math.Max(bestValue, futilityValue));
                    continue;
                }

                if (futilityBase <= alpha && pos.see(move) <= Value.VALUE_ZERO)
                {
                    bestValue = new Value(Math.Max(bestValue, futilityBase));
                    continue;
                }
            }

            // Detect non-capture evasions that are candidates to be pruned
            var evasionPrunable = InCheck
                                   && bestValue > Value.VALUE_MATED_IN_MAX_PLY
                                   && !pos.capture(move);

            // Don't search moves with negative SEE values
            if ((!InCheck || evasionPrunable)
                && Move.type_of(move) != MoveType.PROMOTION
                && pos.see_sign(move) < Value.VALUE_ZERO)
                continue;

            // Speculative prefetch as early as possible
            //prefetch(TT.first_entry(pos.key_after(move)));

            // Check for legality just before making the move
            if (!pos.legal(move, ci.pinned))
                continue;

            ss[ss.current].currentMove = move;

            // Make and search the move
            pos.do_move(move, st, givesCheck);
            var value = givesCheck
                ? -qsearch(NT, true, pos, new StackArrayWrapper(ss.table, ss.current + 1), -beta, -alpha,
                    depth - Depth.ONE_PLY)
                : -qsearch(NT, false, pos, new StackArrayWrapper(ss.table, ss.current + 1), -beta, -alpha,
                    depth - Depth.ONE_PLY);
            pos.undo_move(move);

            Debug.Assert(value > -Value.VALUE_INFINITE && value < Value.VALUE_INFINITE);

            // Check for new best move
            if (value > bestValue)
            {
                bestValue = value;

                if (value > alpha)
                {
                    if (PvNode) // Update pv even in fail-high case
                        update_pv(ss[ss.current].pv, move, ss[ss.current + 1].pv);

                    if (PvNode && value < beta) // Update alpha here! Always alpha < beta
                    {
                        alpha = value;
                        bestMove = move;
                    }
                    else // Fail high
                    {
                        tte.save(posKey, value_to_tt(value, ss[ss.current].ply), Bound.BOUND_LOWER,
                            ttDepth, move, ss[ss.current].staticEval, TranspositionTable.generation());

                        return value;
                    }
                }
            }
        }

        // All legal moves have been searched. A special case: If we're in check
        // and no legal moves were found, it is checkmate.
        if (InCheck && bestValue == -Value.VALUE_INFINITE)
            return Value.mated_in(ss[ss.current].ply); // Plies to mate from the root

        tte.save(posKey, value_to_tt(bestValue, ss[ss.current].ply),
            PvNode && bestValue > oldAlpha ? Bound.BOUND_EXACT : Bound.BOUND_UPPER,
            ttDepth, bestMove, ss[ss.current].staticEval, TranspositionTable.generation());

        Debug.Assert(bestValue > -Value.VALUE_INFINITE && bestValue < Value.VALUE_INFINITE);

        return bestValue;
    }

    // update_stats() updates killers, history, countermove history and
    // countermoves stats for a quiet best move.

    private static void update_stats(Position pos, StackArrayWrapper ss, Move move,
        Depth depth, Move[] quiets, int quietsCnt)
    {
        if (ss[ss.current].killers0 != move)
        {
            ss[ss.current].killers1 = ss[ss.current].killers0;
            ss[ss.current].killers0 = move;
        }

        var bonus = new Value((depth/Depth.ONE_PLY)*(depth/Depth.ONE_PLY));

        var prevSq = Move.to_sq(ss[ss.current - 1].currentMove);
        var cmh = CounterMovesHistory.table[pos.piece_on(prevSq), prevSq];

        History.updateH(pos.moved_piece(move), Move.to_sq(move), bonus);

        if (Move.is_ok(ss[ss.current - 1].currentMove))
        {
            Countermoves.update(pos.piece_on(prevSq), prevSq, move);
            cmh.updateCMH(pos.moved_piece(move), Move.to_sq(move), bonus);
        }

        // Decrease all the other played quiet moves
        for (var i = 0; i < quietsCnt; ++i)
        {
            History.updateH(pos.moved_piece(quiets[i]), Move.to_sq(quiets[i]), -bonus);

            if (Move.is_ok(ss[ss.current - 1].currentMove))
                cmh.updateCMH(pos.moved_piece(quiets[i]), Move.to_sq(quiets[i]), -bonus);
        }

        // Extra penalty for PV move in previous ply when it gets refuted
        if (Move.is_ok(ss[ss.current - 2].currentMove) && ss[ss.current - 1].moveCount == 1 &&
            !pos.captured_piece_type())
        {
            var prevPrevSq = Move.to_sq(ss[ss.current - 2].currentMove);
            var ttMoveCmh = CounterMovesHistory.table[pos.piece_on(prevPrevSq), prevPrevSq];
            ttMoveCmh.updateCMH(pos.piece_on(prevSq), prevSq, -bonus - 2*depth/Depth.ONE_PLY - 1);
        }
    }

    // value_to_tt() adjusts a mate score from "plies to mate from the root" to
    // "plies to mate from the current position". Non-mate scores are unchanged.
    // The function is called before storing a value in the transposition table.

    private static Value value_to_tt(Value v, int ply)
    {
        Utils.WriteToLog($"value_to_tt(v={v}, ply={ply})");
        Debug.Assert(v != Value.VALUE_NONE);

        return v >= Value.VALUE_MATE_IN_MAX_PLY
            ? v + ply
            : v <= Value.VALUE_MATED_IN_MAX_PLY ? v - ply : v;
    }

    // update_pv() adds current move and appends child pv[]
    private static void update_pv(List<Move> pv, Move move, List<Move> childPv)
    {
        pv.Clear();
        pv.Add(move);
        pv.AddRange(childPv);
    }
}