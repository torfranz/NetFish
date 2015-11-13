﻿using System.Runtime.CompilerServices;

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
internal struct Move
{
    internal static Move MOVE_NONE = new Move(0);

    internal static Move MOVE_NULL = new Move(65);

    private int Value { get; }

    #region constructors

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Move(int value)
    {
        Value = value;
    }

    #endregion

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static implicit operator int(Move m)
    {
        return m.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static implicit operator bool(Move m)
    {
        return m.Value != 0;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator ==(Move v1, Move v2)
    {
        return v1.Value == v2.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator !=(Move v1, Move v2)
    {
        return v1.Value != v2.Value;
    }
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public override string ToString()
    {
        return $"{Value}";
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Square from_sq(Move m)
    {
        return new Square((m.Value >> 6) & 0x3F);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Square to_sq(Move m)
    {
        return new Square(m.Value & 0x3F);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static MoveType type_of(Move m)
    {
        return (MoveType) (m.Value & (3 << 14));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static PieceType promotion_type(Move m)
    {
        return PieceType.Create(((m.Value >> 12) & 3) + PieceType.KNIGHT_C);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Move make_move(Square from, Square to)
    {
        return new Move((int)to | ((int)from << 6));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Move make(MoveType moveType, Square from, Square to)
    {
        return make(moveType, from, to, PieceType.KNIGHT);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Move make(MoveType moveType, Square from, Square to, PieceType pt)
    {
        return new Move((int)to | ((int)from << 6) | (int) moveType | (((int)pt - PieceType.KNIGHT_C) << 12));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static bool is_ok(Move m)
    {
        return from_sq(m) != to_sq(m); // Catch MOVE_NULL and MOVE_NONE
    }
}