
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

        //UCI::loop(argc, argv);

        //Threads.exit();
    }
}
