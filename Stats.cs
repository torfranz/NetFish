using System;
using System.Runtime.CompilerServices;

/// The Stats struct stores moves statistics. According to the template parameter
/// the class can store History and Countermoves. History records how often
/// different moves have been successful or unsuccessful during the current search
/// and is used for reduction and move ordering decisions.
/// Countermoves store the move that refute a previous one. Entries are stored
/// using only the moving piece and destination square, hence two moves with
/// different origin but same destination and piece will be considered identical.
public class Stats<T>
    where T : new()
{
    public readonly T[,] table = new T[Piece.PIECE_NB, Square.SQUARE_NB];

    public static Value Max = new Value(1 << 28);

#if FORCEINLINE
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public T value(Piece p, Square to)
    {
        return this.table[p, to];
    }

    public Stats()
    {
        for (int idx1 = 0; idx1 < table.GetLength(0); idx1++)
        {
            for (int idx2 = 0; idx2 < table.GetLength(1); idx2++)
            {
                table[idx1,idx2] = new T();
            }
        }
    }
};

public class MovesStats : Stats<Move>
{
    public void update(Piece pc, Square to, Move m)
    {
        if (m != this.table[pc, to])
        {
            this.table[pc, to] = m;
        }
    }
}

public class HistoryStats : Stats<Value>
{
    public void updateH(Piece pc, Square to, Value v)
    {
        if (Math.Abs(v) >= 324)
        {
            return;
        }
        this.table[pc, to] -= this.table[pc, to] * Math.Abs(v) / 324;
        this.table[pc, to] += v * 32;
    }

    public void updateCMH(Piece pc, Square to, Value v)
    {
        if (Math.Abs(v) >= 324)
        {
            return;
        }
        this.table[pc, to] -= this.table[pc, to] * Math.Abs(v) / 512;
        this.table[pc, to] += v * 64;
    }
}

public class CounterMovesHistoryStats : Stats<HistoryStats>
{
}