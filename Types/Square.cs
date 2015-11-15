using System.Runtime.CompilerServices;

#if IMPLICIT
using File = System.Int32;
#endif

internal struct Square
{
    internal const int SQ_A1_C = 0;
    internal const int SQ_A8_C = 56;
    internal const int SQ_H8_C = 63;
    internal const int SQUARE_NB_C = 64;
    internal static Square SQ_A1 = new Square(SQ_A1_C);

    internal static Square SQ_B1 = new Square(1);

    internal static Square SQ_C1 = new Square(2);

    internal static Square SQ_D1 = new Square(3);

    internal static Square SQ_E1 = new Square(4);

    internal static Square SQ_F1 = new Square(5);

    internal static Square SQ_G1 = new Square(6);

    internal static Square SQ_H1 = new Square(7);

    internal static Square SQ_A2 = new Square(8);

    internal static Square SQ_B2 = new Square(9);

    internal static Square SQ_C2 = new Square(10);

    internal static Square SQ_D2 = new Square(11);

    internal static Square SQ_E2 = new Square(12);

    internal static Square SQ_F2 = new Square(13);

    internal static Square SQ_G2 = new Square(14);

    internal static Square SQ_H2 = new Square(15);

    internal static Square SQ_A3 = new Square(16);

    internal static Square SQ_B3 = new Square(17);

    internal static Square SQ_C3 = new Square(18);

    internal static Square SQ_D3 = new Square(19);

    internal static Square SQ_E3 = new Square(20);

    internal static Square SQ_F3 = new Square(21);

    internal static Square SQ_G3 = new Square(22);

    internal static Square SQ_H3 = new Square(23);

    internal static Square SQ_A4 = new Square(24);

    internal static Square SQ_B4 = new Square(25);

    internal static Square SQ_C4 = new Square(26);

    internal static Square SQ_D4 = new Square(27);

    internal static Square SQ_E4 = new Square(28);

    internal static Square SQ_F4 = new Square(29);

    internal static Square SQ_G4 = new Square(30);

    internal static Square SQ_H4 = new Square(31);

    internal static Square SQ_A5 = new Square(32);

    internal static Square SQ_B5 = new Square(33);

    internal static Square SQ_C5 = new Square(34);

    internal static Square SQ_D5 = new Square(35);

    internal static Square SQ_E5 = new Square(36);

    internal static Square SQ_F5 = new Square(37);

    internal static Square SQ_G5 = new Square(38);

    internal static Square SQ_H5 = new Square(39);

    internal static Square SQ_A6 = new Square(40);

    internal static Square SQ_B6 = new Square(41);

    internal static Square SQ_C6 = new Square(42);

    internal static Square SQ_D6 = new Square(43);

    internal static Square SQ_E6 = new Square(44);

    internal static Square SQ_F6 = new Square(45);

    internal static Square SQ_G6 = new Square(46);

    internal static Square SQ_H6 = new Square(47);

    internal static Square SQ_A7 = new Square(48);

    internal static Square SQ_B7 = new Square(49);

    internal static Square SQ_C7 = new Square(50);

    internal static Square SQ_D7 = new Square(51);

    internal static Square SQ_E7 = new Square(52);

    internal static Square SQ_F7 = new Square(53);

    internal static Square SQ_G7 = new Square(54);

    internal static Square SQ_H7 = new Square(55);

    internal static Square SQ_A8 = new Square(56);

    internal static Square SQ_B8 = new Square(57);

    internal static Square SQ_C8 = new Square(58);

    internal static Square SQ_D8 = new Square(59);

    internal static Square SQ_E8 = new Square(60);

    internal static Square SQ_F8 = new Square(61);

    internal static Square SQ_G8 = new Square(62);

    internal static Square SQ_H8 = new Square(SQ_H8_C);

    internal static Square SQ_NONE = new Square(64);

    internal static Square SQUARE_NB = new Square(64);

    internal static Square DELTA_N = new Square(8);

    internal static Square DELTA_E = new Square(1);

    internal static Square DELTA_S = new Square(-8);

    internal static Square DELTA_W = new Square(-1);

    internal static Square DELTA_NN = new Square(DELTA_N + DELTA_N);

    internal static Square DELTA_NE = new Square(DELTA_N + DELTA_E);

    internal static Square DELTA_SE = new Square(DELTA_S + DELTA_E);

    internal static Square DELTA_SS = new Square(DELTA_S + DELTA_S);

    internal static Square DELTA_SW = new Square(DELTA_S + DELTA_W);

    internal static Square DELTA_NW = new Square(DELTA_N + DELTA_W);

    private int Value;
    
    #region constructors

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Square(Square value)
        : this(value.Value)
    {
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Square(uint value)
        : this((int) value)
    {
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Square(int value)
    {
        Value = value;
        // Debug.Assert(this.ValueMe >= -9 && this.ValueMe <= 64);
    }

    #endregion

    #region base operators

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Square operator +(Square v1, Square v2)
    {
        return new Square(v1.Value + v2.Value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Square operator +(Square v1, int v2)
    {
        return new Square(v1.Value + v2);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Square operator -(Square v1, Square v2)
    {
        return new Square(v1.Value - v2.Value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Square operator -(Square v1, int v2)
    {
        return new Square(v1.Value - v2);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Square operator *(int v1, Square v2)
    {
        return new Square(v1*v2.Value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static implicit operator int(Square s)
    {
        return s.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Square operator ++(Square v1)
    {
        v1.Value += 1;
        return v1;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Square operator --(Square v1)
    {
        v1.Value -= 1;
        return v1;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public override string ToString()
    {
        return Value.ToString();
    }

    #endregion

    #region extended operators

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Square operator /(Square v1, int v2)
    {
        return new Square(v1.Value/v2);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Square operator ~(Square s)
    {
        return new Square(s.Value ^ SQ_A8_C); // Vertical flip SQ_A1 -> SQ_A8
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator ==(Square v1, Square v2)
    {
        return v1.Value == v2.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator !=(Square v1, Square v2)
    {
        return v1.Value != v2.Value;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator <(Square v1, Square v2)
    {
        return v1.Value < v2.Value;
    }
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator <=(Square v1, Square v2)
    {
        return v1.Value <= v2.Value;
    }
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator >=(Square v1, Square v2)
    {
        return v1.Value >= v2.Value;
    }
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool operator >(Square v1, Square v2)
    {
        return v1.Value > v2.Value;
    }
    #endregion

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal bool is_ok()
    {
        return Value >= SQ_A1_C && Value <= SQ_H8_C;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static bool opposite_colors(Square s1, Square s2)
    {
        var s = s1.Value ^ s2.Value;
        return (((s >> 3) ^ s) & 1) != 0;
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Square relative_square(Color c, Square s)
    {
        return new Square(s.Value ^ (c.ValueMe * 56));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Square make_square(File f, Rank r)
    {
        return new Square((r << 3) | f);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static File file_of(Square s)
    {
        return FileConstants.Create(s.Value & 7);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Rank rank_of(Square s)
    {
        return Rank.Create(s.Value >> 3);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Square pawn_push(Color c)
    {
        return c == Color.WHITE ? new Square(DELTA_N) : new Square(DELTA_S);
    }
}