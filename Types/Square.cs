using System.Diagnostics;
using System.Runtime.CompilerServices;

#if PRIMITIVE
using FileT = System.Int32;
using RankT = System.Int32;
using ColorT = System.Int32;
using SquareT = System.Int32;
#else

internal struct SquareT
{
    private int Value;
    
#region constructors

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal SquareT(SquareT value)
        : this(value.Value)
    {
    }

    internal SquareT(int value)
    {
        Value = value;
        // Debug.Assert(this.Value >= -9 && this.Value <= 64);
    }

#endregion

#region base operators

    public static SquareT operator +(SquareT v1, SquareT v2)
    {
        return new SquareT(v1.Value + v2.Value);
    }

    public static SquareT operator +(SquareT v1, int v2)
    {
        return new SquareT(v1.Value + v2);
    }

    public static SquareT operator -(SquareT v1, SquareT v2)
    {
        return new SquareT(v1.Value - v2.Value);
    }

    public static SquareT operator -(SquareT v1, int v2)
    {
        return new SquareT(v1.Value - v2);
    }

    public static SquareT operator *(int v1, SquareT v2)
    {
        return new SquareT(v1*v2.Value);
    }

    public static implicit operator int(SquareT s)
    {
        return s.Value;
    }

    public static SquareT operator ++(SquareT v1)
    {
        v1.Value += 1;
        return v1;
    }

    public static SquareT operator --(SquareT v1)
    {
        v1.Value -= 1;
        return v1;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

#endregion

#region extended operators

    public static SquareT operator /(SquareT v1, int v2)
    {
        return new SquareT(v1.Value/v2);
    }

    public static bool operator ==(SquareT v1, SquareT v2)
    {
        return v1.Value == v2.Value;
    }

    public static bool operator !=(SquareT v1, SquareT v2)
    {
        return v1.Value != v2.Value;
    }

    public static bool operator <(SquareT v1, SquareT v2)
    {
        return v1.Value < v2.Value;
    }

    public static bool operator <=(SquareT v1, SquareT v2)
    {
        return v1.Value <= v2.Value;
    }

    public static bool operator >=(SquareT v1, SquareT v2)
    {
        return v1.Value >= v2.Value;
    }

    public static bool operator >(SquareT v1, SquareT v2)
    {
        return v1.Value > v2.Value;
    }
#endregion
}
#endif

internal static class Square
{

#if PRIMITIVE

    internal const SquareT SQ_A1 = 0;

    internal const SquareT SQ_B1 = 1;

    internal const SquareT SQ_C1 = 2;

    internal const SquareT SQ_D1 = 3;

    internal const SquareT SQ_E1 = 4;

    internal const SquareT SQ_F1 = 5;

    internal const SquareT SQ_G1 = 6;

    internal const SquareT SQ_H1 = 7;

    internal const SquareT SQ_A2 = 8;

    internal const SquareT SQ_B2 = 9;

    internal const SquareT SQ_C2 = 10;

    internal const SquareT SQ_D2 = 11;

    internal const SquareT SQ_E2 = 12;

    internal const SquareT SQ_F2 = 13;

    internal const SquareT SQ_G2 = 14;

    internal const SquareT SQ_H2 = 15;

    internal const SquareT SQ_A3 = 16;

    internal const SquareT SQ_B3 = 17;

    internal const SquareT SQ_C3 = 18;

    internal const SquareT SQ_D3 = 19;

    internal const SquareT SQ_E3 = 20;

    internal const SquareT SQ_F3 = 21;

    internal const SquareT SQ_G3 = 22;

    internal const SquareT SQ_H3 = 23;

    internal const SquareT SQ_A4 = 24;

    internal const SquareT SQ_B4 = 25;

    internal const SquareT SQ_C4 = 26;

    internal const SquareT SQ_D4 = 27;

    internal const SquareT SQ_E4 = 28;

    internal const SquareT SQ_F4 = 29;

    internal const SquareT SQ_G4 = 30;

    internal const SquareT SQ_H4 = 31;

    internal const SquareT SQ_A5 = 32;

    internal const SquareT SQ_B5 = 33;

    internal const SquareT SQ_C5 = 34;

    internal const SquareT SQ_D5 = 35;

    internal const SquareT SQ_E5 = 36;

    internal const SquareT SQ_F5 = 37;

    internal const SquareT SQ_G5 = 38;

    internal const SquareT SQ_H5 = 39;

    internal const SquareT SQ_A6 = 40;

    internal const SquareT SQ_B6 = 41;

    internal const SquareT SQ_C6 = 42;

    internal const SquareT SQ_D6 = 43;

    internal const SquareT SQ_E6 = 44;

    internal const SquareT SQ_F6 = 45;

    internal const SquareT SQ_G6 = 46;

    internal const SquareT SQ_H6 = 47;

    internal const SquareT SQ_A7 = 48;

    internal const SquareT SQ_B7 = 49;

    internal const SquareT SQ_C7 = 50;

    internal const SquareT SQ_D7 = 51;

    internal const SquareT SQ_E7 = 52;

    internal const SquareT SQ_F7 = 53;

    internal const SquareT SQ_G7 = 54;

    internal const SquareT SQ_H7 = 55;

    internal const SquareT SQ_A8 = 56;

    internal const SquareT SQ_B8 = 57;

    internal const SquareT SQ_C8 = 58;

    internal const SquareT SQ_D8 = 59;

    internal const SquareT SQ_E8 = 60;

    internal const SquareT SQ_F8 = 61;

    internal const SquareT SQ_G8 = 62;

    internal const SquareT SQ_H8 = 63;

    internal const SquareT SQ_NONE = 64;

    internal const SquareT SQUARE_NB = 64;

    internal const SquareT DELTA_N = 8;

    internal const SquareT DELTA_E = 1;

    internal const SquareT DELTA_S = -8;

    internal const SquareT DELTA_W = -1;

    internal const SquareT DELTA_NN = DELTA_N + DELTA_N;

    internal const SquareT DELTA_NE = DELTA_N + DELTA_E;

    internal const SquareT DELTA_SE = DELTA_S + DELTA_E;

    internal const SquareT DELTA_SS = DELTA_S + DELTA_S;

    internal const SquareT DELTA_SW = DELTA_S + DELTA_W;

    internal const SquareT DELTA_NW = DELTA_N + DELTA_W;

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static SquareT Create(int value)
    {
        return value;
    }

#else
    internal static SquareT SQ_A1 = Square.Create(0);

    internal static SquareT SQ_B1 = Square.Create(1);

    internal static SquareT SQ_C1 = Square.Create(2);

    internal static SquareT SQ_D1 = Square.Create(3);

    internal static SquareT SQ_E1 = Square.Create(4);

    internal static SquareT SQ_F1 = Square.Create(5);

    internal static SquareT SQ_G1 = Square.Create(6);

    internal static SquareT SQ_H1 = Square.Create(7);

    internal static SquareT SQ_A2 = Square.Create(8);

    internal static SquareT SQ_B2 = Square.Create(9);

    internal static SquareT SQ_C2 = Square.Create(10);

    internal static SquareT SQ_D2 = Square.Create(11);

    internal static SquareT SQ_E2 = Square.Create(12);

    internal static SquareT SQ_F2 = Square.Create(13);

    internal static SquareT SQ_G2 = Square.Create(14);

    internal static SquareT SQ_H2 = Square.Create(15);

    internal static SquareT SQ_A3 = Square.Create(16);

    internal static SquareT SQ_B3 = Square.Create(17);

    internal static SquareT SQ_C3 = Square.Create(18);

    internal static SquareT SQ_D3 = Square.Create(19);

    internal static SquareT SQ_E3 = Square.Create(20);

    internal static SquareT SQ_F3 = Square.Create(21);

    internal static SquareT SQ_G3 = Square.Create(22);

    internal static SquareT SQ_H3 = Square.Create(23);

    internal static SquareT SQ_A4 = Square.Create(24);

    internal static SquareT SQ_B4 = Square.Create(25);

    internal static SquareT SQ_C4 = Square.Create(26);

    internal static SquareT SQ_D4 = Square.Create(27);

    internal static SquareT SQ_E4 = Square.Create(28);

    internal static SquareT SQ_F4 = Square.Create(29);

    internal static SquareT SQ_G4 = Square.Create(30);

    internal static SquareT SQ_H4 = Square.Create(31);

    internal static SquareT SQ_A5 = Square.Create(32);

    internal static SquareT SQ_B5 = Square.Create(33);

    internal static SquareT SQ_C5 = Square.Create(34);

    internal static SquareT SQ_D5 = Square.Create(35);

    internal static SquareT SQ_E5 = Square.Create(36);

    internal static SquareT SQ_F5 = Square.Create(37);

    internal static SquareT SQ_G5 = Square.Create(38);

    internal static SquareT SQ_H5 = Square.Create(39);

    internal static SquareT SQ_A6 = Square.Create(40);

    internal static SquareT SQ_B6 = Square.Create(41);

    internal static SquareT SQ_C6 = Square.Create(42);

    internal static SquareT SQ_D6 = Square.Create(43);

    internal static SquareT SQ_E6 = Square.Create(44);

    internal static SquareT SQ_F6 = Square.Create(45);

    internal static SquareT SQ_G6 = Square.Create(46);

    internal static SquareT SQ_H6 = Square.Create(47);

    internal static SquareT SQ_A7 = Square.Create(48);

    internal static SquareT SQ_B7 = Square.Create(49);

    internal static SquareT SQ_C7 = Square.Create(50);

    internal static SquareT SQ_D7 = Square.Create(51);

    internal static SquareT SQ_E7 = Square.Create(52);

    internal static SquareT SQ_F7 = Square.Create(53);

    internal static SquareT SQ_G7 = Square.Create(54);

    internal static SquareT SQ_H7 = Square.Create(55);

    internal static SquareT SQ_A8 = Square.Create(56);

    internal static SquareT SQ_B8 = Square.Create(57);

    internal static SquareT SQ_C8 = Square.Create(58);

    internal static SquareT SQ_D8 = Square.Create(59);

    internal static SquareT SQ_E8 = Square.Create(60);

    internal static SquareT SQ_F8 = Square.Create(61);

    internal static SquareT SQ_G8 = Square.Create(62);

    internal static SquareT SQ_H8 = Square.Create(63);

    internal static SquareT SQ_NONE = Square.Create(64);

    internal static SquareT SQUARE_NB = Square.Create(64);

    internal static SquareT DELTA_N = Square.Create(8);

    internal static SquareT DELTA_E = Square.Create(1);

    internal static SquareT DELTA_S = Square.Create(-8);

    internal static SquareT DELTA_W = Square.Create(-1);

    internal static SquareT DELTA_NN = Square.Create(DELTA_N + DELTA_N);

    internal static SquareT DELTA_NE = Square.Create(DELTA_N + DELTA_E);

    internal static SquareT DELTA_SE = Square.Create(DELTA_S + DELTA_E);

    internal static SquareT DELTA_SS = Square.Create(DELTA_S + DELTA_S);

    internal static SquareT DELTA_SW = Square.Create(DELTA_S + DELTA_W);

    internal static SquareT DELTA_NW = Square.Create(DELTA_N + DELTA_W);

    public static SquareT Create(int value)
    {
        return new SquareT(value);
    }
#endif
    
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static SquareT opposite(SquareT s)
    {
        return Square.Create(s ^ SQ_A8); // Vertical flip SQ_A1 -> SQ_A8
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static bool is_ok(SquareT s)
    {
        return s >= SQ_A1 && s <= SQ_H8;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static bool opposite_colors(SquareT s1, SquareT s2)
    {
        var s = s1 ^ s2;
        return (((s >> 3) ^ s) & 1) != 0;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static SquareT relative_square(ColorT c, SquareT s)
    {
        return Square.Create(s ^ (c * 56));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static SquareT make_square(FileT f, RankT r)
    {
        return Square.Create((r << 3) | f);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static FileT file_of(SquareT s)
    {
        return File.Create(s & 7);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static RankT rank_of(SquareT s)
    {
        return Rank.Create(s >> 3);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static SquareT pawn_push(ColorT c)
    {
        return c == Color.WHITE ? DELTA_N : DELTA_S;
    }
}