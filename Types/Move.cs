using System.Runtime.CompilerServices;

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
public struct Move
{
    public static Move MOVE_NONE = new Move(0);

    public static Move MOVE_NULL = new Move(65);

    private int Value { get; }

    #region constructors

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Move(int value)
    {
        this.Value = value;
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Move v1, Move v2)
    {
        return v1.Value == v2.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Move v1, Move v2)
    {
        return v1.Value != v2.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Square from_sq()
    {
        return new Square((this.Value >> 6) & 0x3F);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Square to_sq()
    {
        return new Square(this.Value & 0x3F);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MoveType type_of()
    {
        return (MoveType)(this.Value & (3 << 14));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PieceType promotion_type()
    {
        return new PieceType(((this.Value >> 12) & 3) + PieceType.KNIGHT);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Move make_move(Square from, Square to)
    {
        return new Move(to | (from << 6));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Move make(MoveType moveType, Square from, Square to, PieceType pt)
    {
        return new Move(to | (from << 6) | (int)moveType | ((pt - PieceType.KNIGHT) << 12));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool is_ok()
    {
        return this.from_sq() != this.to_sq(); // Catch MOVE_NULL and MOVE_NONE
    }
}