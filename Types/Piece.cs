using System.Diagnostics;
using System.Runtime.CompilerServices;

public struct Piece
{
    public const int NO_PIECE = 0;

    public const int W_PAWN = 1;

    public const int W_KNIGHT = 2;

    public const int W_BISHOP = 3;

    public const int W_ROOK = 4;

    public const int W_QUEEN = 5;

    public const int W_KING = 6;

    public const int B_PAWN = 9;

    public const int B_KNIGHT = 10;

    public const int B_BISHOP = 11;

    public const int B_ROOK = 12;

    public const int B_QUEEN = 13;

    public const int B_KING = 14;

    public const int PIECE_NB = 16;

    public int Value { get; }

    #region constructors

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Piece(Piece value)
        : this(value.Value)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Piece(int value)
    {
        this.Value = value;
        Debug.Assert(this.Value >= 0 && this.Value <= 16);
        Debug.Assert(this.Value != 7);
        Debug.Assert(this.Value != 8);
        Debug.Assert(this.Value != 15);
    }

    #endregion

    #region base operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator +(Piece v1, Piece v2)
    {
        return new Piece(v1.Value + v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator +(Piece v1, int v2)
    {
        return new Piece(v1.Value + v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator +(int v1, Piece v2)
    {
        return new Piece(v1 + v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator -(Piece v1, Piece v2)
    {
        return new Piece(v1.Value - v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator -(Piece v1, int v2)
    {
        return new Piece(v1.Value - v2);
    }

    /*
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator -(Piece v1)
    {
        return new Piece(-v1.value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator *(int v1, Piece v2)
    {
        return new Piece(v1 * v2.value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator *(Piece v1, int v2)
    {
        return new Piece(v1.value * v2);
    }

    #endregion

    #region extended operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator /(Piece v1, Piece v2)
    {
        return v1.value / v2.value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator /(Piece v1, int v2)
    {
        return new Piece(v1.value / v2);
    }
    */

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PieceType type_of()
    {
        return new PieceType(this.Value & 7);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Color color_of()
    {
        return new Color(this.Value >> 3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece make_piece(Color c, PieceType pt)
    {
        return new Piece((c.Value << 3) | pt.Value);
    }
}