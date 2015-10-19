using System.Diagnostics;
using System.Runtime.CompilerServices;

public struct Square
{
    public const int SQ_A1 = 0;

    public const int SQ_B1 = 1;

    public const int SQ_C1 = 2;

    public const int SQ_D1 = 3;

    public const int SQ_E1 = 4;

    public const int SQ_F1 = 5;

    public const int SQ_G1 = 6;

    public const int SQ_H1 = 7;

    public const int SQ_A2 = 8;

    public const int SQ_B2 = 9;

    public const int SQ_C2 = 10;

    public const int SQ_D2 = 11;

    public const int SQ_E2 = 12;

    public const int SQ_F2 = 13;

    public const int SQ_G2 = 14;

    public const int SQ_H2 = 15;

    public const int SQ_A3 = 16;

    public const int SQ_B3 = 17;

    public const int SQ_C3 = 18;

    public const int SQ_D3 = 19;

    public const int SQ_E3 = 20;

    public const int SQ_F3 = 21;

    public const int SQ_G3 = 22;

    public const int SQ_H3 = 23;

    public const int SQ_A4 = 24;

    public const int SQ_B4 = 25;

    public const int SQ_C4 = 26;

    public const int SQ_D4 = 27;

    public const int SQ_E4 = 28;

    public const int SQ_F4 = 29;

    public const int SQ_G4 = 30;

    public const int SQ_H4 = 31;

    public const int SQ_A5 = 32;

    public const int SQ_B5 = 33;

    public const int SQ_C5 = 34;

    public const int SQ_D5 = 35;

    public const int SQ_E5 = 36;

    public const int SQ_F5 = 37;

    public const int SQ_G5 = 38;

    public const int SQ_H5 = 39;

    public const int SQ_A6 = 40;

    public const int SQ_B6 = 41;

    public const int SQ_C6 = 42;

    public const int SQ_D6 = 43;

    public const int SQ_E6 = 44;

    public const int SQ_F6 = 45;

    public const int SQ_G6 = 46;

    public const int SQ_H6 = 47;

    public const int SQ_A7 = 48;

    public const int SQ_B7 = 49;

    public const int SQ_C7 = 50;

    public const int SQ_D7 = 51;

    public const int SQ_E7 = 52;

    public const int SQ_F7 = 53;

    public const int SQ_G7 = 54;

    public const int SQ_H7 = 55;

    public const int SQ_A8 = 56;

    public const int SQ_B8 = 57;

    public const int SQ_C8 = 58;

    public const int SQ_D8 = 59;

    public const int SQ_E8 = 60;

    public const int SQ_F8 = 61;

    public const int SQ_G8 = 62;

    public const int SQ_H8 = 63;

    public const int SQ_NONE = 64;

    public const int SQUARE_NB = 64;

    public const int DELTA_N = 8;

    public const int DELTA_E = 1;

    public const int DELTA_S = -8;

    public const int DELTA_W = -1;

    public const int DELTA_NN = DELTA_N + DELTA_N;

    public const int DELTA_NE = DELTA_N + DELTA_E;

    public const int DELTA_SE = DELTA_S + DELTA_E;

    public const int DELTA_SS = DELTA_S + DELTA_S;

    public const int DELTA_SW = DELTA_S + DELTA_W;

    public const int DELTA_NW = DELTA_N + DELTA_W;

    public int Value { get; }

    #region constructors

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Square(Square value)
        : this(value.Value)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Square(int value)
    {
        this.Value = value;
        // Debug.Assert(this.Value >= -9 && this.Value <= 64);
    }

    #endregion

    #region base operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator +(Square v1, Square v2)
    {
        return new Square(v1.Value + v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator +(Square v1, int v2)
    {
        return new Square(v1.Value + v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator +(int v1, Square v2)
    {
        return new Square(v1 + v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator -(Square v1, Square v2)
    {
        return new Square(v1.Value - v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator -(Square v1, int v2)
    {
        return new Square(v1.Value - v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator -(Square v1)
    {
        return new Square(-v1.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator *(int v1, Square v2)
    {
        return new Square(v1 * v2.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator *(Square v1, int v2)
    {
        return new Square(v1.Value * v2);
    }

    #endregion

    #region extended operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int operator /(Square v1, Square v2)
    {
        return v1.Value / v2.Value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator /(Square v1, int v2)
    {
        return new Square(v1.Value / v2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square operator ~(Square c)
    {
        return new Square(c.Value ^ SQ_A8); // Vertical flip SQ_A1 -> SQ_A8
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool is_ok()
    {
        return this.Value >= SQ_A1 && this.Value <= SQ_H8;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool opposite_colors(Square other)
    {
        var s = this.Value ^ other.Value;
        return (((s >> 3) ^ s) & 1) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Square relative_square(Color c)
    {
        return new Square(this.Value ^ (c.Value * 56));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square make_square(File f, Rank r)
    {
        return new Square((r.Value << 3) | f.Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public File file_of()
    {
        return new File(this.Value & 7);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Rank rank_of()
    {
        return new Rank(this.Value >> 3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square pawn_push(Color c)
    {
        return c.Value == Color.WHITE ? new Square(DELTA_N) : new Square(DELTA_S);
    }
}