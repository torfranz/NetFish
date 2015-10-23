using System;
using System.IO;
using System.Text;

internal class Program
{
    private static readonly byte[] inputBuffer = new byte[8192];

    private static void Main(string[] args)
    {
        // Setup an 8k inputBuffer because really long UCI strings were getting truncated
        var inputStream = Console.OpenStandardInput(inputBuffer.Length);
        Console.SetIn(new StreamReader(inputStream, Encoding.ASCII, false, inputBuffer.Length));

        Console.WriteLine(Utils.engine_info());

        var t = new System.Threading.Thread(Program.Run);
        t.Start(args);
    }

    private static void Run(object arguments)
    {
        var args = (string[])arguments;

        //UCI.init(Options);
        PSQT.init();
        Bitboards.init();
        Position.init();
        Bitbases.init();
        //Search::init();
        //Eval::init();
        //Pawns::init();
        //Threads.init();
        //Tablebases::init(Options["SyzygyPath"]);
        //TT.resize(Options["Hash"]);

        //TODO: Enable calls
        /*
        Threads.init();

        // .Net warmup sequence
        Plug.IsWarmup = true;
        var pos = new Position(UCI.StartFEN, false, Threads.main_thread());
        var stack = Position.CreateStack("go depth 7");
        UCI.go(pos, stack);
        Threads.wait_for_think_finished();
        Plug.IsWarmup = false;
        */

        var sb = new StringBuilder();
        for (var i = 1; i < args.Length; i++)
        {
            sb.Append(args[i]).Append(" ");
        }

        UCI.loop(sb.ToString());

        //TODO: Enable calls
        //Threads.exit();

    }
}