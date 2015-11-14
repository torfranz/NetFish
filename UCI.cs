using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

internal static class UCI
{
    // FEN string of the initial position, normal chess
    internal const string StartFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    // Stack to keep track of the position states along the setup moves (from the
    // start position to the position just before the search starts). Needed by
    // 'draw by repetition' detection.
    internal static StateInfoWrapper SetupStates = new StateInfoWrapper();

    /// UCI::square() converts a Square to a string in algebraic notation (g1, a7, etc.)
    internal static string square(Square s)
    {
        return $"{ (char)('a' + (int)Square.file_of(s))}{ (char)('1' + (int)Square.rank_of(s))}";
    }

    /// UCI::pv() formats PV information according to the UCI protocol. UCI requires
    /// that all (if any) unsearched PV lines are sent using a previous search score.
    internal static string pv(Position pos, Depth depth, Value alpha, Value beta)
    {
        var ss = new StringBuilder();
        var elapsed = TimeManagement.elapsed() + 1;
        var multiPV = Math.Min(int.Parse(OptionMap.Instance["MultiPV"].v), Search.RootMoves.Count);
        var selDepth = ThreadPool.threads.Select(th => th.maxPly).Concat(new[] {0}).Max();

        for (var i = 0; i < multiPV; ++i)
        {
            var updated = (i <= Search.PVIdx);

            if (depth == Depth.ONE_PLY && !updated)
            {
                continue;
            }

            var d = updated ? depth : depth - Depth.ONE_PLY;
            var v = updated ? Search.RootMoves[i].score : Search.RootMoves[i].previousScore;

            var tb = Tablebases.RootInTB && Math.Abs(v) < Value.VALUE_MATE - _.MAX_PLY;
            v = tb? Tablebases.Score : v;

            ss.Append($"info depth {d/Depth.ONE_PLY} seldepth {selDepth} multipv {i + 1} score {value(v)}");

            if (!tb && i == Search.PVIdx)
            {
                ss.Append(v >= beta ? " lowerbound" : v <= alpha ? " upperbound" : "");
            }

            ss.Append($" nodes {pos.nodes_searched()} nps {pos.nodes_searched()*1000/elapsed}");

            if (elapsed > 1000) // Earlier makes little sense
                ss.Append($" hashfull {TranspositionTable.hashfull()}");

            ss.Append($" tbhits {Tablebases.Hits} time {elapsed} pv");
            
            foreach (var m in Search.RootMoves[i].pv)
            {
                ss.Append($" {move(m, pos.is_chess960())}");
            }
        }

        return ss.ToString();
    }

    /// UCI::value() converts a ValueMe to a string suitable for use with the UCI
    /// protocol specification:
    /// 
    /// cp
    /// x
    ///     The score from the engine's point of view in centipawns.
    ///     mate
    ///     y
    ///         Mate in y moves, not plies. If the engine is getting mated
    ///         use negative values for y.
    internal static string value(Value v)
    {
        if (Math.Abs(v) < Value.VALUE_MATE - _.MAX_PLY)
        {
            return $"cp {v*100/Value.PawnValueEg}";
        }
        return $"mate {(v > 0 ? Value.VALUE_MATE - v + 1 : -Value.VALUE_MATE - v)/2}";
    }

    // position() is called when engine receives the "position" UCI command.
    // The function sets up the position described in the given FEN string ("fen")
    // or the starting position ("startpos") and then makes the moves given in the
    // following move list ("moves").
    internal static void position(Position pos, Stack<string> stack)
    {
        Move m;
        string fen = string.Empty;

        if (stack.Count == 0)
        {
            // do nothing for incomplete command
            return;
        }
        var token = stack.Pop();

        if (token == "startpos")
        {
            fen = StartFEN;
            if (stack.Count > 0)
            {
                token = stack.Pop();
            } // Consume "moves" token if any
        }
        else if (token == "fen")
        {
            while ((stack.Count > 0) && (token = stack.Pop()) != "moves")
            {
                fen += token + " ";
            }
        }
        else
        {
            return;
        }

        pos.set(fen, bool.Parse(OptionMap.Instance["UCI_Chess960"].v), ThreadPool.main());

        // Parse move list (if any)
        while ((stack.Count > 0) && (m = to_move(pos, token = stack.Pop())) != Move.MOVE_NONE)
        {
            pos.do_move(m, SetupStates[SetupStates.current], pos.gives_check(m, new CheckInfo(pos)));

            // Increment pointer to SetupStates circular buffer
            SetupStates++;
        }
        
    }

    /// UCI::move() converts a Move to a string in coordinate notation (g1f3, a7a8q).
    /// The only special case is castling, where we print in the e1g1 notation in
    /// normal chess mode, and in e1h1 notation in chess960 mode. Internally all
    /// castling moves are always encoded as 'king captures rook'.
    internal static string move(Move m, bool chess960)
    {
        var from = Move.from_sq(m);
        var to = Move.to_sq(m);

        if (m == Move.MOVE_NONE)
        {
            return "(none)";
        }

        if (m == Move.MOVE_NULL)
        {
            return "0000";
        }

        if (Move.type_of(m) == MoveType.CASTLING && !chess960)
        {
            to = Square.make_square(to > from ? File.FILE_G : File.FILE_C, Square.rank_of(from));
        }

        var move = square(from) + square(to);

        if (Move.type_of(m) == MoveType.PROMOTION)
        {
            move += " pnbrqk"[(int)Move.promotion_type(m)];
        }

        return move;
    }

    /// UCI::to_move() converts a string representing a move in coordinate notation
    /// (g1f3, a7a8q) to the corresponding legal Move, if any.
    private static Move to_move(Position pos, string str)
    {
        if (str.Length == 5) // Junior could send promotion piece in uppercase
        {
            var chars = str.ToCharArray();
            chars[4] = Position.tolower(str[4]);
            str = new string(chars);
        }

        var ml = new MoveList(GenType.LEGAL, pos);
        for (var index = ml.begin(); index < ml.end(); index++)
        {
            var extMove = ml.moveList.table[index];
            if (str == move(extMove, pos.is_chess960()))
            {
                return extMove;
            }
        }

        return Move.MOVE_NONE;
    }

    // setoption() is called when engine receives the "setoption" UCI command. The
    // function updates the UCI option ("name") to the given value ("value").

    internal static void setoption(Stack<string> stack)
    {
        string token, name = null, value = null;

        // Consume "name" token
        stack.Pop();

        // Read option name (can contain spaces)
        while ((stack.Count > 0) && ((token = stack.Pop()) != "value"))
        {
            name += (name == null ? string.Empty : " ") + token;
        }

        // Read option value (can contain spaces)
        while ((stack.Count > 0) && ((token = stack.Pop()) != "value"))
        {
            value += (value == null ? string.Empty : " ") + token;
        }

        if (OptionMap.Instance.Contains(name))
        {
            OptionMap.Instance[name].v = value;
        }
        else
        {
            Output.Write("No such option: ");
            Output.Write(name);
            Output.Write(Environment.NewLine);
        }
    }

    // go() is called when engine receives the "go" UCI command. The function sets
    // the thinking time and other parameters from the input string, then starts
    // the search.
    internal static void go(Position pos, Stack<string> stack)
    {
        var token = string.Empty;
        var limits = new LimitsType();

        while (stack.Count > 0)
        {
            token = stack.Pop();

            if (token == "wtime")
            {
                limits.time[Color.WHITE_C] = int.Parse(stack.Pop());
            }
            else if (token == "btime")
            {
                limits.time[Color.BLACK_C] = int.Parse(stack.Pop());
            }
            else if (token == "winc")
            {
                limits.inc[Color.WHITE_C] = int.Parse(stack.Pop());
            }
            else if (token == "binc")
            {
                limits.inc[Color.BLACK_C] = int.Parse(stack.Pop());
            }
            else if (token == "movestogo")
            {
                limits.movestogo = int.Parse(stack.Pop());
            }
            else if (token == "depth")
            {
                limits.depth = int.Parse(stack.Pop());
            }
            else if (token == "nodes")
            {
                limits.nodes = ulong.Parse(stack.Pop());
            }
            else if (token == "movetime")
            {
                limits.movetime = int.Parse(stack.Pop());
            }
            else if (token == "mate")
            {
                limits.mate = int.Parse(stack.Pop());
            }
            else if (token == "infinite")
            {
                limits.infinite = 1;
            }
            else if (token == "ponder")
            {
                limits.ponder = true;
            }
            else if (token == "searchmoves")
            {
                while ((token = stack.Pop()) != null)
                {
                    limits.searchmoves.Add(to_move(pos, token));
                }
            }
        }

        ThreadPool.start_thinking(pos, limits, SetupStates);
    }

    /// UCI::loop() waits for a command from stdin, parses it and calls the appropriate
    /// function. Also intercepts EOF from stdin to ensure gracefully exiting if the
    /// GUI dies unexpectedly. When called with some command line arguments, e.g. to
    /// run 'bench', once the command is executed the function returns immediately.
    /// In addition to the UCI ones, also some additional debug commands are supported.
    internal static void loop(string args)
    {
        var pos = new Position(StartFEN, false, ThreadPool.main()); // The root position
        string token = string.Empty;

        do
        {
            try
            {
                string cmd;
                if (args.Length > 0)
                {
                    cmd = args;
                }
                else if (null == (cmd = Console.ReadLine())) // Block here waiting for input
                {
                    cmd = "quit";
                }
                
                var stack = Position.CreateStack(cmd);
                if (stack.Count == 0)
                {
                    continue;
                }

                token = stack.Pop();

                // The GUI sends 'ponderhit' to tell us to ponder on the same move the
                // opponent has played. In case Signals.stopOnPonderhit is set we are
                // waiting for 'ponderhit' to stop the search (for instance because we
                // already ran out of time), otherwise we should continue searching but
                // switching from pondering to normal search.
                if (token == "quit" || token == "stop" || (token == "ponderhit") && Search.Signals.stopOnPonderhit)
                {
                    Search.Signals.stop = true;
                    ThreadPool.main().notify_one(); // Could be sleeping
                }
                else if (token == "ponderhit")
                {
                    Search.Limits.ponder = false; // Switch to normal search
                }
                else if (token == "uci")
                {
                    Output.Write("id name ");
                    Output.Write(Utils.engine_info(true));
                    Output.Write("\n");
                    Output.Write(OptionMap.Instance.ToString());
                    Output.WriteLine("\nuciok");
                }
                else if (token == "ucinewgame")
                {
                    Search.reset();
                    TimeManagement.availableNodes = 0;
                }
                else if (token == "isready")
                {
                    Output.WriteLine("readyok");
                }
                else if (token == "go")
                {
                    go(pos, stack);
                }
                else if (token == "position")
                {
                    position(pos, stack);
                }
                else if (token == "setoption")
                {
                    setoption(stack);
                }

                // Additional custom non-UCI commands, useful for debugging
                else if (token == "flip")
                {
                    pos.flip();
                }
                else if (token == "eval")
                {
                    Output.WriteLine(Eval.trace(pos));
                }
                else if (token == "bench")
                {
                    Benchmark.benchmark(pos, stack);
                }
                else if (token == "d")
                {
                    Output.Write(pos.displayString());
                }
                else if (token == "perft")
                {
                    token = stack.Pop(); // Read depth
                    var ss =
                        Position.CreateStack(
                            $"{OptionMap.Instance["Hash"].v} {OptionMap.Instance["Threads"].v} {token} current perft");
                    Benchmark.benchmark(pos, ss);
                }

                else
                {
                    Output.Write("Unknown command: ");
                    Output.WriteLine(cmd);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An error occurred: {ex}");
            }
        } while (token != "quit" && args.Length == 0); // Passed args have one-shot behaviour

        ThreadPool.main().join(); // Cannot quit whilst the search is running
    }
}