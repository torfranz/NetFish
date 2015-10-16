using System.Runtime.CompilerServices;

using Key = System.UInt64;
using Bitboard = System.UInt64;
using Move = System.Int32;
using File = System.Int32;
using Rank = System.Int32;
using Score = System.Int32;
using Square = System.Int32;
using Color = System.Int32;
using Value = System.Int32;
using PieceType = System.Int32;
using Piece = System.Int32;
using CastleRight = System.Int32;
using Depth = System.Int32;
using Result = System.Int32;
using ScaleFactor = System.Int32;
using Phase = System.Int32;

internal static class Bitcount
{
    /// count_1s() counts the number of nonzero bits in a bitboard.
    /// We have different optimized versions according if platform
    /// is 32 or 64 bits, and to the maximum number of nonzero bits.
    /// We also support hardware popcnt instruction. See Readme.txt
    /// on how to pgo compile with popcnt support.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int popcount_Full(ulong b)
    {
#if X64
            b -=  (b >> 1) & 0x5555555555555555UL;
         b  = ((b >> 2) & 0x3333333333333333UL) + (b & 0x3333333333333333UL);
        b  = ((b >> 4) + b) & 0x0F0F0F0F0F0F0F0FUL;
         return (int)(b * 0x0101010101010101UL) >> 56;
#else
        uint w = (uint)(b >> 32), v = (uint)b;
        v -= (v >> 1) & 0x55555555; // 0-2 in 2 bits
        w -= (w >> 1) & 0x55555555;
        v = ((v >> 2) & 0x33333333) + (v & 0x33333333); // 0-4 in 4 bits
        w = ((w >> 2) & 0x33333333) + (w & 0x33333333);
        v = ((v >> 4) + v + (w >> 4) + w) & 0x0F0F0F0F;
        return (int)(v * 0x01010101) >> 24;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int popcount_Max15(ulong b)
    {
#if X64
            b -=  (b >> 1) & 0x5555555555555555UL;
  b  = ((b >> 2) & 0x3333333333333333ULL) + (b & 0x3333333333333333UL);
  return (b * 0x1111111111111111UL) >> 60;
#else
        uint w = (uint)(b >> 32), v = (uint)(b);
        v -= (v >> 1) & 0x55555555; // 0-2 in 2 bits
        w -= (w >> 1) & 0x55555555;
        v = ((v >> 2) & 0x33333333) + (v & 0x33333333); // 0-4 in 4 bits
        w = ((w >> 2) & 0x33333333) + (w & 0x33333333);
        return (int)((v + w) * 0x11111111) >> 28;
#endif
    }
}

