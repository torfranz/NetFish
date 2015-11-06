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
        var args = (string[])arguments;

        var sw = Stopwatch.StartNew();
        PSQT.init();
        Console.WriteLine($"   PSQT init took {sw.ElapsedMilliseconds} ms");
        sw = Stopwatch.StartNew();
        Bitboards.init();
        Console.WriteLine($"   Bitboards init took {sw.ElapsedMilliseconds} ms");
        sw = Stopwatch.StartNew();
        Position.init();
        Console.WriteLine($"   Position init took {sw.ElapsedMilliseconds} ms");
        sw = Stopwatch.StartNew();
        Bitbases.init();
        Console.WriteLine($"   Bitbases init took {sw.ElapsedMilliseconds} ms");
        sw = Stopwatch.StartNew();
        Search.init();
        Console.WriteLine($"   Search init took {sw.ElapsedMilliseconds} ms");
        sw = Stopwatch.StartNew();
        Eval.init();
        Console.WriteLine($"   Eval init took {sw.ElapsedMilliseconds} ms");
        sw = Stopwatch.StartNew();
        Pawns.init();
        Console.WriteLine($"   Pawns init took {sw.ElapsedMilliseconds} ms");

        //Tablebases::init(Options["SyzygyPath"]);
        TranspositionTable.resize(uint.Parse(OptionMap.Instance["Hash"].v));

        ThreadPool.init();

        // .Net warmup sequence
        var pos = new Position(UCI.StartFEN, false, ThreadPool.main());
        var stack = Position.CreateStack("go depth 5");
        UCI.go(pos, stack);
        ThreadPool.wait_for_think_finished();
        
        var sb = new StringBuilder();
        for (var i = 1; i < args.Length; i++)
        {
            sb.Append(args[i]).Append(" ");
        }

        UCI.loop(sb.ToString());

        ThreadPool.exit();
    }
}