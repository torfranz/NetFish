using System.Runtime.CompilerServices;

#if PRIMITIVE
using ValueT = System.Int32;
#endif
/// Score enum stores a middlegame and an endgame value in a single integer
/// (enum). The least significant 16 bits are used to store the endgame value
/// and the upper 16 bits are used to store the middlegame value.
internal struct Score
{
    internal static Score SCORE_ZERO = new Score(0);

    private readonly int value;
    
#if FORCEINLINE
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public override string ToString()
    {
        return $"{value}";
    }

    #region constructors

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal Score(int value)
    {
        this.value = value;
    }

    #endregion

    #region base operators

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Score operator +(Score v1, Score v2)
    {
        return new Score(v1.value + v2.value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Score operator -(Score v1, Score v2)
    {
        return new Score(v1.value - v2.value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Score operator -(Score v1)
    {
        return new Score(-v1.value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Score operator *(int v1, Score v2)
    {
        return new Score(v1*v2.value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Score operator *(Score v1, int v2)
    {
        return new Score(v1.value*v2);
    }

    #endregion

    #region extended operators

    /// Division of a Score must be handled separately for each term
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Score operator /(Score v1, int v2)
    {
        return make_score(mg_value(v1)/v2, eg_value(v1)/v2);
    }

    #endregion

    /// Extracting the signed lower and upper 16 bits is not so trivial because
    /// according to the standard a simple cast to short is implementation defined
    /// and so is a right shift of a signed integer.
#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static ValueT mg_value(Score s)
    {
        // union { uint16_t u; int16_t s; }
        // mg = { uint16_t(unsigned(s + 0x8000) >> 16) };
        return Value.Create((short) (((uint) s.value + 0x8000) >> 16));
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static Score operator *(Score s, Eval.Weight w)
    {
        return make_score(mg_value(s)*w.mg/256, eg_value(s)*w.eg/256);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static ValueT eg_value(Score s)
    {
        // union { uint16_t u; int16_t s; }
        // eg = { uint16_t(unsigned(s)) };
        return Value.Create((short) s.value);
    }

#if FORCEINLINE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static Score make_score(int mg, int eg)
    {
        return new Score((mg << 16) + eg);
    }
}