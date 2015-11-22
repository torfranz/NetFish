using System.Runtime.CompilerServices;

#if PRIMITIVE
using ValueT = System.Int32;
using ScoreT = System.Int32;
#else
/// Score enum stores a middlegame and an endgame value in a single integer
/// (enum). The least significant 16 bits are used to store the endgame value
/// and the upper 16 bits are used to store the middlegame value.
internal struct ScoreT
{
    private readonly int value;
    
#if FORCEINLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public override string ToString()
    {
        return $"{value}";
    }

    #region constructors

    internal ScoreT(int value)
    {
        this.value = value;
    }

    #endregion

    #region base operators

    public static implicit operator int (ScoreT s)
    {
        return s.value;
    }

    public static ScoreT operator +(ScoreT v1, ScoreT v2)
    {
        return new ScoreT(v1.value + v2.value);
    }

    public static ScoreT operator -(ScoreT v1, ScoreT v2)
    {
        return new ScoreT(v1.value - v2.value);
    }

    public static ScoreT operator -(ScoreT v1)
    {
        return new ScoreT(-v1.value);
    }

    public static ScoreT operator *(int v1, ScoreT v2)
    {
        return new ScoreT(v1*v2.value);
    }

    public static ScoreT operator *(ScoreT v1, int v2)
    {
        return new ScoreT(v1.value*v2);
    }

    #endregion
    
}
#endif

internal static class Score
{

#if PRIMITIVE
    internal const ScoreT SCORE_ZERO = 0;
    
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static ScoreT Create(int value)
    {
        return value;
    }

#else
    internal static ScoreT SCORE_ZERO = new ScoreT(0);

    public static ScoreT Create(int value)
    {
        return new ScoreT(value);
    }
#endif

    /// Division of a Score must be handled separately for each term
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static ScoreT Divide(ScoreT v1, int v2)
    {
        return make_score(mg_value(v1) / v2, eg_value(v1) / v2);
    }

    /// Extracting the signed lower and upper 16 bits is not so trivial because
    /// according to the standard a simple cast to short is implementation defined
    /// and so is a right shift of a signed integer.
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static ValueT mg_value(ScoreT s)
    {
        // union { uint16_t u; int16_t s; }
        // mg = { uint16_t(unsigned(s + 0x8000) >> 16) };
        return Value.Create((short)(((uint)(int)s + 0x8000) >> 16));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static ScoreT Multiply(ScoreT s, Eval.Weight w)
    {
        return make_score(mg_value(s) * w.mg / 256, eg_value(s) * w.eg / 256);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static ValueT eg_value(ScoreT s)
    {
        // union { uint16_t u; int16_t s; }
        // eg = { uint16_t(unsigned(s)) };
        return Value.Create((short)(int)s);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static ScoreT make_score(int mg, int eg)
    {
        return Create((mg << 16) + eg);
    }

}