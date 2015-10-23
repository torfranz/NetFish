public struct Bitboard
{
    public const ulong DarkSquares = 0xAA55AA55AA55AA55UL;

    public static readonly Bitboard FileABB = new Bitboard(0x0101010101010101UL);

    public static readonly Bitboard FileBBB = FileABB << 1;

    public static readonly Bitboard FileCBB = FileABB << 2;

    public static readonly Bitboard FileDBB = FileABB << 3;

    public static readonly Bitboard FileEBB = FileABB << 4;

    public static readonly Bitboard FileFBB = FileABB << 5;

    public static readonly Bitboard FileGBB = FileABB << 6;

    public static readonly Bitboard FileHBB = FileABB << 7;

    public static readonly Bitboard Rank1BB = new Bitboard(0xFF);

    public static readonly Bitboard Rank2BB = Rank1BB << (8*1);

    public static readonly Bitboard Rank3BB = Rank1BB << (8*2);

    public static readonly Bitboard Rank4BB = Rank1BB << (8*3);

    public static readonly Bitboard Rank5BB = Rank1BB << (8*4);

    public static readonly Bitboard Rank6BB = Rank1BB << (8*5);

    public static readonly Bitboard Rank7BB = Rank1BB << (8*6);

    public static readonly Bitboard Rank8BB = Rank1BB << (8*7);

    private ulong Value { get; }

    #region constructors

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 

#endif

    public Bitboard(Bitboard bitboard)
        : this(bitboard.Value)
    {
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Bitboard(ulong value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    #endregion

    /// Overloads of bitwise operators between a Bitboard and a Square for testing
    /// whether a given bit is set in a bitboard, and for setting and clearing bits.
#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    public static Bitboard operator &(Bitboard b, Square s)
    {
        return new Bitboard(b.Value & Utils.SquareBB[s].Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Bitboard operator &(Bitboard b1, Bitboard b2)
    {
        return new Bitboard(b1.Value & b2.Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Bitboard operator -(Bitboard b1, Bitboard b2)
    {
        return new Bitboard(b1.Value - b2.Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Bitboard operator +(Bitboard b1, Bitboard b2)
    {
        return new Bitboard(b1.Value + b2.Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Bitboard operator >>(Bitboard b1, int i)
    {
        return new Bitboard(b1.Value >> i);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Bitboard operator <<(Bitboard b1, int i)
    {
        return new Bitboard(b1.Value << i);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Bitboard operator |(Bitboard b1, Bitboard b2)
    {
        return new Bitboard(b1.Value | b2.Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Bitboard operator *(Bitboard b1, Bitboard b2)
    {
        return new Bitboard(b1.Value*b2.Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Bitboard operator |(Bitboard b, Square s)
    {
        return new Bitboard(b.Value | Utils.SquareBB[s].Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static bool operator ==(Bitboard b1, Bitboard b2)
    {
        return b1.Value == b2.Value;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static bool operator !=(Bitboard b1, Bitboard b2)
    {
        return b1.Value != b2.Value;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Bitboard operator ~(Bitboard b)
    {
        return new Bitboard(~b.Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static implicit operator bool(Bitboard b)
    {
        return b.Value != 0;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static implicit operator ulong(Bitboard b)
    {
        return b.Value;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Bitboard operator ^(Bitboard b1, Bitboard b2)
    {
        return new Bitboard(b1.Value ^ b2.Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Bitboard operator ^(Bitboard b, Square s)
    {
        return new Bitboard(b.Value ^ Utils.SquareBB[s].Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static bool more_than_one(Bitboard b)
    {
        return (b.Value & (b.Value - 1)) != 0;
    }

    /// shift_bb() moves a bitboard one step along direction Delta. Mainly for pawns
#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    public static Bitboard shift_bb(Square Delta, Bitboard b)
    {
        return Delta == Square.DELTA_N
            ? b << 8
            : Delta == Square.DELTA_S
                ? b >> 8
                : Delta == Square.DELTA_NE
                    ? (b & ~FileHBB) << 9
                    : Delta == Square.DELTA_SE
                        ? (b & ~FileHBB) >> 7
                        : Delta == Square.DELTA_NW
                            ? (b & ~FileABB) << 7
                            : Delta == Square.DELTA_SW ? (b & ~FileABB) >> 9 : new Bitboard(0);
    }
}