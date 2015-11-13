using System.Runtime.CompilerServices;

internal struct Bitboard
{
    internal static readonly Bitboard DarkSquares = new Bitboard(0xAA55AA55AA55AA55UL);

    internal static readonly Bitboard FileABB = new Bitboard(0x0101010101010101UL);

    internal static readonly Bitboard FileBBB = FileABB << 1;

    internal static readonly Bitboard FileCBB = FileABB << 2;

    internal static readonly Bitboard FileDBB = FileABB << 3;

    internal static readonly Bitboard FileEBB = FileABB << 4;

    internal static readonly Bitboard FileFBB = FileABB << 5;

    internal static readonly Bitboard FileGBB = FileABB << 6;

    internal static readonly Bitboard FileHBB = FileABB << 7;

    internal static readonly Bitboard Rank1BB = new Bitboard(0xFF);

    internal static readonly Bitboard Rank2BB = Rank1BB << (8*1);

    internal static readonly Bitboard Rank3BB = Rank1BB << (8*2);

    internal static readonly Bitboard Rank4BB = Rank1BB << (8*3);

    internal static readonly Bitboard Rank5BB = Rank1BB << (8*4);

    internal static readonly Bitboard Rank6BB = Rank1BB << (8*5);

    internal static readonly Bitboard Rank7BB = Rank1BB << (8*6);

    internal static readonly Bitboard Rank8BB = Rank1BB << (8*7);

    private ulong Value { get; }

    #region constructors

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Bitboard(Bitboard bitboard)
        : this(bitboard.Value)
    {
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Bitboard(ulong value)
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
        return new Bitboard(b.Value & Utils.SquareBB[(int)s].Value);
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
        return new Bitboard(b.Value | Utils.SquareBB[(int)s].Value);
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
        return new Bitboard(b.Value ^ Utils.SquareBB[(int)s].Value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static bool more_than_one(Bitboard b)
    {
        return (b.Value & (b.Value - 1)) != 0;
    }

    /// shift_bb() moves a bitboard one step along direction Delta. Mainly for pawns
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Bitboard shift_bb(Square Delta, Bitboard b)
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