using System;
using System.Runtime.CompilerServices;

/// The Stats struct stores moves statistics. According to the template parameter
/// the class can store History and Countermoves. History records how often
/// different moves have been successful or unsuccessful during the current search
/// and is used for reduction and move ordering decisions.
/// Countermoves store the move that refute a previous one. Entries are stored
/// using only the moving piece and destination square, hence two moves with
/// different origin but same destination and piece will be considered identical.
internal class Stats<T>
    where T : new()
{
    internal static Value Max = new Value(1 << 28);
    internal readonly T[,] table = new T[Piece.PIECE_NB, Square.SQUARE_NB_C];

    internal Stats()
    {
        for (var idx1 = 0; idx1 < table.GetLength(0); idx1++)
        {
            for (var idx2 = 0; idx2 < table.GetLength(1); idx2++)
            {
                table[idx1, idx2] = new T();
            }
        }
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal T value(Piece p, Square to)
    {
        return table[p, (int)to];
    }
};

internal class MovesStats : Stats<Move>
{
    internal void update(Piece pc, Square to, Move m)
    {
        table[pc, (int)to] = m;
    }
}

internal class HistoryStats : Stats<Value>
{
    internal void updateH(Piece pc, Square to, Value v)
    {
        if (Math.Abs(v) >= 324)
        {
            return;
        }
        table[pc, (int)to] -= table[pc, (int)to]*Math.Abs(v)/324;
        table[pc, (int)to] += v*32;
    }

    internal void updateCMH(Piece pc, Square to, Value v)
    {
        if (Math.Abs(v) >= 324)
        {
            return;
        }
        table[pc, (int)to] -= table[pc, (int)to]*Math.Abs(v)/512;
        table[pc, (int)to] += v*64;
    }
}

internal class CounterMovesHistoryStats : Stats<HistoryStats>
{
}