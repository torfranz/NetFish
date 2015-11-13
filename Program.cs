using System;
using System.Diagnostics;
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

        var t = new System.Threading.Thread(Run);
        t.Start(args);
    }

    private static void Run(object arguments)
    {
        var args = (string[]) arguments;

        PSQT.init();
        Bitboards.init();
        Position.init();
        Bitbases.init();
        Search.init();
        Eval.init();
        Pawns.init();
        
        //Tablebases::init(Options["SyzygyPath"]);
        TranspositionTable.resize(uint.Parse(OptionMap.Instance["Hash"].v));

        ThreadPool.init();

#if WARMUP
        // .Net warmup sequence
        var pos = new Position(UCI.StartFEN, false, ThreadPool.main());
        var stack = Position.CreateStack("go depth 7");
        UCI.go(pos, stack);
        ThreadPool.wait_for_think_finished();
#endif
        var sb = new StringBuilder();
        for (var i = 1; i < args.Length; i++)
        {
            sb.Append(args[i]).Append(" ");
        }

        // start showing output
        Output.showOutput = true;

        UCI.loop(sb.ToString());

        ThreadPool.exit();
    }
}