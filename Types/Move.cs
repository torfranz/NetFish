using System.Runtime.CompilerServices;

#if PRIMITIVE
using PieceTypeT = System.Int32;
using SquareT = System.Int32;
using MoveT = System.Int32;
using ColorT = System.Int32;
#else

/// A move needs 16 bits to be stored
/// 
/// bit  0- 5: destination square (from 0 to 63)
/// bit  6-11: origin square (from 0 to 63)
/// bit 12-13: promotion piece type - 2 (from KNIGHT-2 to QUEEN-2)
/// bit 14-15: special move flag: promotion (1), en passant (2), castling (3)
/// NOTE: EN-PASSANT bit is set only when a pawn can be captured
/// 
/// Special cases are MOVE_NONE and MOVE_NULL. We can sneak these in because in
/// any normal move destination square is always different from origin square
/// while MOVE_NONE and MOVE_NULL have the same origin and destination square.
internal struct MoveT
{
    private readonly int Value;

#region constructors

    internal MoveT(int value)
    {
        Value = value;
    }

#endregion

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static implicit operator int(MoveT m)
    {
        return m.Value;
    }

    public static bool operator ==(MoveT v1, MoveT v2)
    {
        return v1.Value == v2.Value;
    }

    public static bool operator !=(MoveT v1, MoveT v2)
    {
        return v1.Value != v2.Value;
    }

    public override string ToString()
    {
        return $"{Value}";
    }
}
#endif

internal static class Move
{

#if PRIMITIVE
    internal const MoveT MOVE_NONE = 0;

    internal const MoveT MOVE_NULL = 65;
    
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static ColorT Create(int value)
    {
        return value;
    }

#else
    internal static MoveT MOVE_NONE = Move.Create(0);

    internal static MoveT MOVE_NULL = Move.Create(65);

    public static MoveT Create(int value)
    {
        return new MoveT(value);
    }
#endif

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static SquareT from_sq(MoveT m)
    {
        return Square.Create((m >> 6) & 0x3F);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static SquareT to_sq(MoveT m)
    {
        return Square.Create(m & 0x3F);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static MoveType type_of(MoveT m)
    {
        return (MoveType)(m & (3 << 14));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static PieceTypeT promotion_type(MoveT m)
    {
        return PieceType.Create(((m >> 12) & 3) + PieceType.KNIGHT);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static MoveT make_move(SquareT from, SquareT to)
    {
        return Move.Create(to | (from << 6));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static MoveT make(MoveType moveType, SquareT from, SquareT to)
    {
        return make(moveType, from, to, PieceType.KNIGHT);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static MoveT make(MoveType moveType, SquareT from, SquareT to, PieceTypeT pt)
    {
        return Move.Create(to | (from << 6) | (int)moveType | ((pt - PieceType.KNIGHT) << 12));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static bool is_ok(MoveT m)
    {
        return from_sq(m) != to_sq(m); // Catch MOVE_NULL and MOVE_NONE
    }
}