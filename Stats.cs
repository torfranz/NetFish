using System;
using System.Runtime.CompilerServices;

#if PRIMITIVE
using PieceT = System.Int32;
using ValueT = System.Int32;
using SquareT = System.Int32;
#endif
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
    internal static ValueT Max = Value.Create(1 << 28);
    internal readonly T[,] table = new T[Piece.PIECE_NB, Square.SQUARE_NB];

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
    internal T value(PieceT p, SquareT to)
    {
        return table[p, to];
    }
};

internal class MovesStats : Stats<Move>
{
    internal void update(PieceT pc, SquareT to, Move m)
    {
        table[pc, to] = m;
    }
}

internal class HistoryStats : Stats<ValueT>
{
    internal void updateH(PieceT pc, SquareT to, ValueT v)
    {
        if (Math.Abs(v) >= 324)
        {
            return;
        }
        table[pc, to] -= table[pc, to]*Math.Abs(v)/324;
        table[pc, to] += v*32;
    }

    internal void updateCMH(PieceT pc, SquareT to, ValueT v)
    {
        if (Math.Abs(v) >= 324)
        {
            return;
        }
        table[pc, to] -= table[pc, to]*Math.Abs(v)/512;
        table[pc, to] += v*64;
    }
}

internal class CounterMovesHistoryStats : Stats<HistoryStats>
{
}