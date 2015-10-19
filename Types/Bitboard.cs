using System.Runtime.CompilerServices;

public struct Bitboard
{
    public const ulong DarkSquares = 0xAA55AA55AA55AA55UL;

    public const ulong FileABB = 0x0101010101010101UL;

    public const ulong FileBBB = FileABB << 1;

    public const ulong FileCBB = FileABB << 2;

    public const ulong FileDBB = FileABB << 3;

    public const ulong FileEBB = FileABB << 4;

    public const ulong FileFBB = FileABB << 5;

    public const ulong FileGBB = FileABB << 6;

    public const ulong FileHBB = FileABB << 7;

    public const ulong Rank1BB = 0xFF;

    public const ulong Rank2BB = Rank1BB << (8 * 1);

    public const ulong Rank3BB = Rank1BB << (8 * 2);

    public const ulong Rank4BB = Rank1BB << (8 * 3);

    public const ulong Rank5BB = Rank1BB << (8 * 4);

    public const ulong Rank6BB = Rank1BB << (8 * 5);

    public const ulong Rank7BB = Rank1BB << (8 * 6);

    public const ulong Rank8BB = Rank1BB << (8 * 7);

    public ulong Value { get; }

    #region constructors

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitboard(Bitboard bitboard)
        : this(bitboard.Value)
    {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitboard(ulong value)
    {
        this.Value = value;
    }

    #endregion

    /// Overloads of bitwise operators between a Bitboard and a Square for testing
    /// whether a given bit is set in a bitboard, and for setting and clearing bits.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Bitboard operator &(Bitboard b, Square s)
    {
        return new Bitboard(b.Value & Utils.SquareBB[s.Value].Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Bitboard operator |(Bitboard b, Square s)
    {
        return new Bitboard(b.Value | Utils.SquareBB[s.Value].Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Bitboard operator ^(Bitboard b, Square s)
    {
        return new Bitboard(b.Value ^ Utils.SquareBB[s.Value].Value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool more_than_one()
    {
        return (this.Value & (this.Value - 1)) != 0;
    }

    /// shift_bb() moves a bitboard one step along direction Delta. Mainly for pawns
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Bitboard shift_bb(Square Delta)
    {
        return
            new Bitboard(
                Delta.Value == Square.DELTA_N
                    ? this.Value << 8
                    : Delta.Value == Square.DELTA_S
                          ? this.Value >> 8
                          : Delta.Value == Square.DELTA_NE
                                ? (this.Value & ~FileHBB) << 9
                                : Delta.Value == Square.DELTA_SE
                                      ? (this.Value & ~FileHBB) >> 7
                                      : Delta.Value == Square.DELTA_NW
                                            ? (this.Value & ~FileABB) << 7
                                            : Delta.Value == Square.DELTA_SW ? (this.Value & ~FileABB) >> 9 : 0);
    }
}