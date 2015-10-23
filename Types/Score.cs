/// Score enum stores a middlegame and an endgame value in a single integer
/// (enum). The least significant 16 bits are used to store the endgame value
/// and the upper 16 bits are used to store the middlegame value.
public struct Score
{
    public static Score SCORE_ZERO = new Score(0);

    private int Value { get; }

    #region constructors

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Score(int value)
    {
        Value = value;
    }

    #endregion

    #region base operators

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Score operator +(Score v1, Score v2)
    {
        return new Score(v1.Value + v2.Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Score operator +(Score v1, int v2)
    {
        return new Score(v1.Value + v2);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Score operator +(int v1, Score v2)
    {
        return new Score(v1 + v2.Value);
    }

    public static Score operator -(Score v1, Score v2)
    {
        return new Score(v1.Value - v2.Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Score operator -(Score v1, int v2)
    {
        return new Score(v1.Value - v2);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Score operator -(Score v1)
    {
        return new Score(-v1.Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Score operator *(int v1, Score v2)
    {
        return new Score(v1*v2.Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Score operator *(Score v1, int v2)
    {
        return new Score(v1.Value*v2);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static implicit operator int(Score s)
    {
        return s.Value;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static bool operator ==(Score v1, Score v2)
    {
        return v1.Value == v2.Value;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static bool operator !=(Score v1, Score v2)
    {
        return v1.Value != v2.Value;
    }

    #endregion

    #region extended operators

    /// Division of a Score must be handled separately for each term
#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    public static Score operator /(Score v1, int v2)
    {
        return make_score(v1.mg_value()/v2, v1.eg_value()/v2);
    }

    #endregion

    /// Extracting the signed lower and upper 16 bits is not so trivial because
    /// according to the standard a simple cast to short is implementation defined
    /// and so is a right shift of a signed integer.
#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif
    public Value mg_value()
    {
        // union { uint16_t u; int16_t s; }
        // mg = { uint16_t(unsigned(s + 0x8000) >> 16) };
        return new Value((ushort) (Value >> 16));
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Value eg_value()
    {
        // union { uint16_t u; int16_t s; }
        // eg = { uint16_t(unsigned(s)) };
        return new Value((ushort) Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Score make_score(int mg, int eg)
    {
        return new Score((mg << 16) + eg);
    }
}