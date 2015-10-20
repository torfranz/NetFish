using System.Diagnostics;
using System.Runtime.CompilerServices;

public struct Piece
{
    public static Piece NO_PIECE = new Piece(0);

    public static Piece W_PAWN = new Piece(1);

    public static Piece W_KNIGHT = new Piece(2);

    public static Piece W_BISHOP = new Piece(3);

    public static Piece W_ROOK = new Piece(4);

    public static Piece W_QUEEN = new Piece(5);

    public static Piece W_KING = new Piece(6);

    public static Piece B_PAWN = new Piece(9);

    public static Piece B_KNIGHT = new Piece(10);

    public static Piece B_BISHOP = new Piece(11);

    public static Piece B_ROOK = new Piece(12);

    public static Piece B_QUEEN = new Piece(13);

    public static Piece B_KING = new Piece(14);

    public static Piece PIECE_NB = new Piece(16);

    private int Value { get; }

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator int(Piece p)
    {
        return p.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Piece v1, Piece v2)
    {
        return v1.Value == v2.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Piece v1, Piece v2)
    {
        return v1.Value != v2.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Piece operator ++(Piece v1)
    {
        return new Piece(v1.Value + 1);
    }

    public override string ToString()
    {
        return this.Value.ToString();
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
        return new Piece((c << 3) | pt);
    }
}