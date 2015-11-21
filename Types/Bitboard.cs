using System.Runtime.CompilerServices;

#if PRIMITIVE
using SquareT = System.Int32;
using BitboardT = System.UInt64;
#else
internal struct BitboardT
{
    private readonly ulong Value;

#region constructors

    internal BitboardT(ulong value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }

#endregion

    public static BitboardT operator &(BitboardT b1, BitboardT b2)
    {
        return Bitboard.Create(b1.Value & b2.Value);
    }

    public static BitboardT operator -(BitboardT b1, BitboardT b2)
    {
        return Bitboard.Create(b1.Value - b2.Value);
    }

    public static BitboardT operator >>(BitboardT b1, int i)
    {
        return Bitboard.Create(b1.Value >> i);
    }

    public static BitboardT operator <<(BitboardT b1, int i)
    {
        return Bitboard.Create(b1.Value << i);
    }

    public static BitboardT operator |(BitboardT b1, BitboardT b2)
    {
        return Bitboard.Create(b1.Value | b2.Value);
    }

    public static BitboardT operator *(BitboardT b1, BitboardT b2)
    {
        return Bitboard.Create(b1.Value*b2.Value);
    }

    public static bool operator ==(BitboardT b1, BitboardT b2)
    {
        return b1.Value == b2.Value;
    }

    public static bool operator !=(BitboardT b1, BitboardT b2)
    {
        return b1.Value != b2.Value;
    }

    public static BitboardT operator ~(BitboardT b)
    {
        return Bitboard.Create(~b.Value);
    }

    public static implicit operator ulong(BitboardT b)
    {
        return b.Value;
    }

    public static BitboardT operator ^(BitboardT b1, BitboardT b2)
    {
        return Bitboard.Create(b1.Value ^ b2.Value);
    }


}
#endif

internal static class Bitboard
{

#if PRIMITIVE

    internal const BitboardT DarkSquares = 0xAA55AA55AA55AA55UL;
             
    internal const BitboardT FileABB = 0x0101010101010101UL;
             
    internal const BitboardT FileBBB = FileABB << 1;
             
    internal const BitboardT FileCBB = FileABB << 2;
             
    internal const BitboardT FileDBB = FileABB << 3;
             
    internal const BitboardT FileEBB = FileABB << 4;
             
    internal const BitboardT FileFBB = FileABB << 5;
             
    internal const BitboardT FileGBB = FileABB << 6;
             
    internal const BitboardT FileHBB = FileABB << 7;
             
    internal const BitboardT Rank1BB = 0xFF;
             
    internal const BitboardT Rank2BB = Rank1BB << (8 * 1);
             
    internal const BitboardT Rank3BB = Rank1BB << (8 * 2);
             
    internal const BitboardT Rank4BB = Rank1BB << (8 * 3);
             
    internal const BitboardT Rank5BB = Rank1BB << (8 * 4);
             
    internal const BitboardT Rank6BB = Rank1BB << (8 * 5);
             
    internal const BitboardT Rank7BB = Rank1BB << (8 * 6);

    internal const BitboardT Rank8BB = Rank1BB << (8 * 7);

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static BitboardT Create(ulong value)
    {
        return value;
    }

#else
    internal static readonly BitboardT DarkSquares = Bitboard.Create(0xAA55AA55AA55AA55UL);

    internal static readonly BitboardT FileABB = Bitboard.Create(0x0101010101010101UL);

    internal static readonly BitboardT FileBBB = FileABB << 1;

    internal static readonly BitboardT FileCBB = FileABB << 2;

    internal static readonly BitboardT FileDBB = FileABB << 3;

    internal static readonly BitboardT FileEBB = FileABB << 4;

    internal static readonly BitboardT FileFBB = FileABB << 5;

    internal static readonly BitboardT FileGBB = FileABB << 6;

    internal static readonly BitboardT FileHBB = FileABB << 7;

    internal static readonly BitboardT Rank1BB = Bitboard.Create(0xFF);

    internal static readonly BitboardT Rank2BB = Rank1BB << (8 * 1);

    internal static readonly BitboardT Rank3BB = Rank1BB << (8 * 2);

    internal static readonly BitboardT Rank4BB = Rank1BB << (8 * 3);

    internal static readonly BitboardT Rank5BB = Rank1BB << (8 * 4);

    internal static readonly BitboardT Rank6BB = Rank1BB << (8 * 5);

    internal static readonly BitboardT Rank7BB = Rank1BB << (8 * 6);

    internal static readonly BitboardT Rank8BB = Rank1BB << (8 * 7);

    public static BitboardT Create(ulong value)
    {
        return new BitboardT(value);
    }
#endif

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static bool more_than_one(BitboardT b)
    {
        return (b & (b - 1)) != 0;
    }

    /// shift_bb() moves a bitboard one step along direction Delta. Mainly for pawns
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static BitboardT shift_bb(SquareT Delta, BitboardT b)
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
                            : Delta == Square.DELTA_SW ? (b & ~FileABB) >> 9 : Bitboard.Create(0);
    }

    /// Overloads of bitwise operators between a Bitboard and a Square for testing
    /// whether a given bit is set in a bitboard, and for setting and clearing bits.
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static BitboardT AndWithSquare(BitboardT b, SquareT s)
    {
        return Bitboard.Create(b & Utils.SquareBB[s]);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static BitboardT XorWithSquare(BitboardT b, SquareT s)
    {
        return Bitboard.Create(b ^ Utils.SquareBB[s]);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static BitboardT OrWithSquare(BitboardT b, SquareT s)
    {
        return Bitboard.Create(b | Utils.SquareBB[s]);
    }
}