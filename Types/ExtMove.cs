using System.Runtime.CompilerServices;

internal struct ExtMove
{
    internal Move Move { get; }

    internal Value Value { get; }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal ExtMove(Move move, Value value)
    {
        Move = move;
        Value = value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static implicit operator Move(ExtMove move)
    {
        return move.Move;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator <(ExtMove f, ExtMove s)
    {
        return f.Value < s.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator >(ExtMove f, ExtMove s)
    {
        return f.Value > s.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public override string ToString()
    {
        return $"{Move},{Value}";
    }
};