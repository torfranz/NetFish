using System.Diagnostics;
using System.Runtime.CompilerServices;

public struct PieceType
{
    public const int NO_PIECE_TYPE = 0;

    public const int PAWN = 1;

    public const int KNIGHT = 2;

    public const int BISHOP = 3;

    public const int ROOK = 4;

    public const int QUEEN = 5;

    public const int KING = 6;

    public const int ALL_PIECES = 0;

    public const int PIECE_TYPE_NB = 8;

    private int Value { get; }

    #region constructors

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PieceType(PieceType value)
        : this(value.Value)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PieceType(int value)
    {
        this.Value = value;
        Debug.Assert(this.Value >= 0 && this.Value <= 8);
        Debug.Assert(this.Value != 7);
    }

    #endregion

    #region base operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceType operator +(PieceType v1, PieceType v2)
    {
        return new PieceType(v1.Value + v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceType operator +(PieceType v1, int v2)
    {
        return new PieceType(v1.Value + v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceType operator +(int v1, PieceType v2)
    {
        return new PieceType(v1 + v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceType operator -(PieceType v1, PieceType v2)
    {
        return new PieceType(v1.Value - v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceType operator -(PieceType v1, int v2)
    {
        return new PieceType(v1.Value - v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator int (PieceType pt)
    {
        return pt.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(PieceType v1, PieceType v2)
    {
        return v1.Value == v2.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(PieceType v1, PieceType v2)
    {
        return v1.Value != v2.Value;
    }

    public override string ToString()
    {
        return this.Value.ToString();
    }
    /*
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceType operator -(PieceType v1)
    {
        return new PieceType(-v1.value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceType operator *(int v1, PieceType v2)
    {
        return new PieceType(v1 * v2.value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceType operator *(PieceType v1, int v2)
    {
        return new PieceType(v1.value * v2);
    }

    #endregion

    #region extended operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator /(PieceType v1, PieceType v2)
    {
        return v1.value / v2.value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceType operator /(PieceType v1, int v2)
    {
        return new PieceType(v1.value / v2);
    }
    */

    #endregion
}