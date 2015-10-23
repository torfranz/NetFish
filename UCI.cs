using System;
using System.Collections.Generic;

public static class UCI
{
    // FEN string of the initial position, normal chess
    internal const string StartFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    // Stack to keep track of the position states along the setup moves (from the
    // start position to the position just before the search starts). Needed by
    // 'draw by repetition' detection.
    internal static readonly StateInfo[] StateRingBuf = new StateInfo[102];

    internal static int SetupStatePos; // *SetupState = StateRingBuf;

    /// UCI::square() converts a Square to a string in algebraic notation (g1, a7, etc.)
    public static string square(Square s)
    {
        return $"{(char) ('a' + Square.file_of(s))}{(char) ('1' + Square.rank_of(s))}";
    }

    // position() is called when engine receives the "position" UCI command.
    // The function sets up the position described in the given FEN string ("fen")
    // or the starting position ("startpos") and then makes the moves given in the
    // following move list ("moves").
    internal static void position(Position pos, Stack<string> stack)
    {
        Move m;
        string token, fen = string.Empty;

        token = stack.Pop();

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

        //TODO: enable call
        //pos.set(fen, bool.Parse(OptionMap.Instance["UCI_Chess960"].v), Threads.main_thread());

        // Parse move list (if any)
        while ((stack.Count > 0) && (m = to_move(pos, token = stack.Pop())) != Move.MOVE_NONE)
        {
            pos.do_move(m, StateRingBuf[SetupStatePos], pos.gives_check(m, new CheckInfo(pos)));

            // Increment pointer to StateRingBuf circular buffer
            SetupStatePos = (SetupStatePos + 1)%102;
        }
    }

    /// UCI::move() converts a Move to a string in coordinate notation (g1f3, a7a8q).
    /// The only special case is castling, where we print in the e1g1 notation in
    /// normal chess mode, and in e1h1 notation in chess960 mode. Internally all
    /// castling moves are always encoded as 'king captures rook'.
    private static string move(Move m, bool chess960)
    {
        var from = Move.from_sq(m);
        var to = Move.to_sq(m);

        if (m == Move.MOVE_NONE)
            return "(none)";

        if (m == Move.MOVE_NULL)
            return "0000";

        if (Move.type_of(m) == MoveType.CASTLING && !chess960)
            to = Square.make_square(to > from ? File.FILE_G : File.FILE_C, Square.rank_of(from));

        var move = square(from) + square(to);

        if (Move.type_of(m) == MoveType.PROMOTION)
            move += " pnbrqk"[Move.promotion_type(m)];

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
        foreach (var extMove in ml.moveList.table)
        {
            if (str == move(extMove, pos.is_chess960()))
                return extMove;
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
            Console.Write("No such option: ");
            Console.Write(name);
            Console.Write(Environment.NewLine);
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
                limits.time[Color.WHITE] = int.Parse(stack.Pop());
            }
            else if (token == "btime")
            {
                limits.time[Color.BLACK] = int.Parse(stack.Pop());
            }
            else if (token == "winc")
            {
                limits.inc[Color.WHITE] = int.Parse(stack.Pop());
            }
            else if (token == "binc")
            {
                limits.inc[Color.BLACK] = int.Parse(stack.Pop());
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

        //TODO: enable call
        //Threads.start_thinking(pos, limits, searchMoves);
    }


    /// UCI::loop() waits for a command from stdin, parses it and calls the appropriate
    /// function. Also intercepts EOF from stdin to ensure gracefully exiting if the
    /// GUI dies unexpectedly. When called with some command line arguments, e.g. to
    /// run 'bench', once the command is executed the function returns immediately.
    /// In addition to the UCI ones, also some additional debug commands are supported.
    internal static void loop(string args)
    {
        for (var i = 0; i < 102; i++)
        {
            StateRingBuf[i] = new StateInfo();
        }

        //TODO: add thread
        var pos = new Position(StartFEN, false /*, Threads.main_thread()*/); // The root position
        string cmd, token = string.Empty;

        do
        {
            if (args.Length > 0)
            {
                cmd = args;
            }
            else if (string.IsNullOrEmpty(cmd = Console.ReadLine())) // Block here waiting for input
            {
                cmd = "quit";
            }

            var stack = Position.CreateStack(cmd);

            token = stack.Pop();

            // The GUI sends 'ponderhit' to tell us to ponder on the same move the
            // opponent has played. In case Signals.stopOnPonderhit is set we are
            // waiting for 'ponderhit' to stop the search (for instance because we
            // already ran out of time), otherwise we should continue searching but
            // switching from pondering to normal search.
            if (token == "quit" || token == "stop" || (token == "ponderhit"))
                //TODO: enable call 
                //&& Search::Signals.stopOnPonderhit))
            {
                //TODO: enable call
                //Search::Signals.stop = true;
                //Threads.main()->notify_one(); // Could be sleeping
            }
            else if (token == "ponderhit")
            {
                //TODO: enable call
                //Search::Limits.ponder = 0; // Switch to normal search
            }
            else if (token == "uci")
            {
                Console.Write("id name ");
                Console.Write(Utils.engine_info(true));
                Console.Write("\n");
                Console.Write(OptionMap.Instance.ToString());
                Console.WriteLine("\nuciok");
            }
            else if (token == "ucinewgame")
            {
                //TODO: enable call
                //Search::reset();
                //Time.availableNodes = 0;
            }
            else if (token == "isready")
            {
                Console.WriteLine("readyok");
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
            else if (token == "bench")
            {
                //TODO: enable call
                //benchmark(pos, stack);
            }
            else if (token == "d")
            {
                Console.Write(pos.displayString());
            }
            else if (token == "eval")
            {
                //TODO: enable call
                //Console.WriteLine(Evaluate.trace(pos));
            }
            else if (token == "perft")
            {
                token = stack.Pop(); // Read depth
                var ss =
                    Position.CreateStack(
                        string.Format(
                            "{0} {1} {2} current perft",
                            OptionMap.Instance["Hash"].v,
                            OptionMap.Instance["Threads"].v,
                            token));
                //TODO: enable call
                //Benchmark.benchmark(pos, ss);
            }

            else
            {
                Console.Write("Unknown command: ");
                Console.WriteLine(cmd);
            }
        } while (token != "quit" && args.Length == 1); // Passed args have one-shot behaviour

        //TODO: enable call
        //Threads.main()->join(); // Cannot quit whilst the search is running
    }
}