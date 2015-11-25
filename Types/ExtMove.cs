
#if PRIMITIVE
using ValueT = System.Int32;
using MoveT = System.Int32;
#endif

internal class ExtMove
{
    internal MoveT Move { get; }

    internal ValueT Value { get; }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    internal ExtMove(MoveT move, ValueT value)
    {
        this.Move = move;
        this.Value = value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif

    public static implicit operator MoveT(ExtMove move)
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
        return $"{this.Move},{this.Value}";
    }
};