using System;

static internal class Output
{
    internal static bool showOutput = false;

    internal static void WriteLine(string content)
    {
        if (showOutput)
        {
            Console.WriteLine(content);
        }
    }

    internal static void WriteLine()
    {
        if (showOutput)
        {
            Console.WriteLine();
        }
    }

    internal static void Write(string content)
    {
        if (showOutput)
        {
            Console.Write(content);
        }
    }
}