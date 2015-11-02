public struct Square
{
    public static Square SQ_A1 = new Square(0);

    public static Square SQ_B1 = new Square(1);

    public static Square SQ_C1 = new Square(2);

    public static Square SQ_D1 = new Square(3);

    public static Square SQ_E1 = new Square(4);

    public static Square SQ_F1 = new Square(5);

    public static Square SQ_G1 = new Square(6);

    public static Square SQ_H1 = new Square(7);

    public static Square SQ_A2 = new Square(8);

    public static Square SQ_B2 = new Square(9);

    public static Square SQ_C2 = new Square(10);

    public static Square SQ_D2 = new Square(11);

    public static Square SQ_E2 = new Square(12);

    public static Square SQ_F2 = new Square(13);

    public static Square SQ_G2 = new Square(14);

    public static Square SQ_H2 = new Square(15);

    public static Square SQ_A3 = new Square(16);

    public static Square SQ_B3 = new Square(17);

    public static Square SQ_C3 = new Square(18);

    public static Square SQ_D3 = new Square(19);

    public static Square SQ_E3 = new Square(20);

    public static Square SQ_F3 = new Square(21);

    public static Square SQ_G3 = new Square(22);

    public static Square SQ_H3 = new Square(23);

    public static Square SQ_A4 = new Square(24);

    public static Square SQ_B4 = new Square(25);

    public static Square SQ_C4 = new Square(26);

    public static Square SQ_D4 = new Square(27);

    public static Square SQ_E4 = new Square(28);

    public static Square SQ_F4 = new Square(29);

    public static Square SQ_G4 = new Square(30);

    public static Square SQ_H4 = new Square(31);

    public static Square SQ_A5 = new Square(32);

    public static Square SQ_B5 = new Square(33);

    public static Square SQ_C5 = new Square(34);

    public static Square SQ_D5 = new Square(35);

    public static Square SQ_E5 = new Square(36);

    public static Square SQ_F5 = new Square(37);

    public static Square SQ_G5 = new Square(38);

    public static Square SQ_H5 = new Square(39);

    public static Square SQ_A6 = new Square(40);

    public static Square SQ_B6 = new Square(41);

    public static Square SQ_C6 = new Square(42);

    public static Square SQ_D6 = new Square(43);

    public static Square SQ_E6 = new Square(44);

    public static Square SQ_F6 = new Square(45);

    public static Square SQ_G6 = new Square(46);

    public static Square SQ_H6 = new Square(47);

    public static Square SQ_A7 = new Square(48);

    public static Square SQ_B7 = new Square(49);

    public static Square SQ_C7 = new Square(50);

    public static Square SQ_D7 = new Square(51);

    public static Square SQ_E7 = new Square(52);

    public static Square SQ_F7 = new Square(53);

    public static Square SQ_G7 = new Square(54);

    public static Square SQ_H7 = new Square(55);

    public static Square SQ_A8 = new Square(56);

    public static Square SQ_B8 = new Square(57);

    public static Square SQ_C8 = new Square(58);

    public static Square SQ_D8 = new Square(59);

    public static Square SQ_E8 = new Square(60);

    public static Square SQ_F8 = new Square(61);

    public static Square SQ_G8 = new Square(62);

    public static Square SQ_H8 = new Square(63);

    public static Square SQ_NONE = new Square(64);

    public static Square SQUARE_NB = new Square(64);

    public static Square DELTA_N = new Square(8);

    public static Square DELTA_E = new Square(1);

    public static Square DELTA_S = new Square(-8);

    public static Square DELTA_W = new Square(-1);

    public static Square DELTA_NN = new Square(DELTA_N + DELTA_N);

    public static Square DELTA_NE = new Square(DELTA_N + DELTA_E);

    public static Square DELTA_SE = new Square(DELTA_S + DELTA_E);

    public static Square DELTA_SS = new Square(DELTA_S + DELTA_S);

    public static Square DELTA_SW = new Square(DELTA_S + DELTA_W);

    public static Square DELTA_NW = new Square(DELTA_N + DELTA_W);

    private int Value { get; }

    #region constructors

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Square(Square value)
        : this(value.Value)
    {
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Square(uint value)
        : this((int)value)
    {
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Square(int value)
    {
        this.Value = value;
        // Debug.Assert(this.Value >= -9 && this.Value <= 64);
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

    public static Square operator +(int v1, Square v2)
    {
        return new Square(v1 + v2.Value);
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

    public static Square operator -(Square v1)
    {
        return new Square(-v1.Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Square operator *(int v1, Square v2)
    {
        return new Square(v1 * v2.Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Square operator *(Square v1, int v2)
    {
        return new Square(v1.Value * v2);
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
        return new Square(v1.Value + 1);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Square operator --(Square v1)
    {
        return new Square(v1.Value - 1);
    }

    public override string ToString()
    {
        return this.Value.ToString();
    }

    #endregion

    #region extended operators

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static int operator /(Square v1, Square v2)
    {
        return v1.Value / v2.Value;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Square operator /(Square v1, int v2)
    {
        return new Square(v1.Value / v2);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Square operator ~(Square s)
    {
        return new Square(s.Value ^ SQ_A8); // Vertical flip SQ_A1 -> SQ_A8
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

    #endregion

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public bool is_ok()
    {
        return this.Value >= SQ_A1 && this.Value <= SQ_H8;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static bool opposite_colors(Square s1, Square s2)
    {
        var s = s1.Value ^ s2.Value;
        return (((s >> 3) ^ s) & 1) != 0;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Square relative_square(Color c, Square s)
    {
        return new Square(s.Value ^ (c * 56));
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Square make_square(File f, Rank r)
    {
        return new Square((r << 3) | f);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static File file_of(Square s)
    {
        return new File(s.Value & 7);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Rank rank_of(Square s)
    {
        return new Rank(s.Value >> 3);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Square pawn_push(Color c)
    {
        return c == Color.WHITE ? new Square(DELTA_N) : new Square(DELTA_S);
    }
}