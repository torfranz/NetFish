using System.Diagnostics;

/// xorshift64star Pseudo-Random Number Generator
/// This class is based on original code written and dedicated
/// to the public domain by Sebastiano Vigna (2014).
/// It has the following characteristics:
/// 
/// -  Outputs 64-bit numbers
/// -  Passes Dieharder and SmallCrush test batteries
/// -  Does not require warm-up, no zeroland to escape
/// -  Internal state is a single 64-bit integer
/// -  Period is 2^64 - 1
/// -  Speed: 1.60 ns/call (Core i7 @3.40GHz)
/// 
/// For further analysis see
/// <http:// vigna.di.unimi.it/ ftp/ papers/ xorshift.pdf>

public class PRNG
{
    private ulong s;

    public PRNG(ulong seed)
    {
        this.s = seed;
        Debug.Assert(seed != 0);
    }

    public ulong rand64()
    {
        this.s ^= this.s >> 12;
        this.s ^= this.s << 25;
        this.s ^= this.s >> 27;
        return this.s * 2685821657736338717L;
    }

    public ulong rand()
    {
        return this.rand64();
    }

    /// Special generator used to fast init magic numbers.
    /// Output values only have 1/8th of their bits set on average.
    public ulong sparse_rand()
    {
        return (this.rand64() & this.rand64() & this.rand64());
    }
};