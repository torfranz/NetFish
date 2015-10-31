using System.Runtime.CompilerServices;

public struct Rank
{
    public const int RANK_1C = 0;
    public const int RANK_2C = 1;
    public const int RANK_3C = 2;
    public const int RANK_4C = 3;
    public const int RANK_5C = 4;
    public const int RANK_6C = 5;
    public const int RANK_7C = 6;
    public const int RANK_8C = 7;
    
    public static Rank RANK_1 = new Rank(RANK_1C);

    public static Rank RANK_2 = new Rank(RANK_2C);

    public static Rank RANK_3 = new Rank(RANK_3C);

    public static Rank RANK_4 = new Rank(RANK_4C);

    public static Rank RANK_5 = new Rank(RANK_5C);

    public static Rank RANK_6 = new Rank(RANK_6C);

    public static Rank RANK_7 = new Rank(RANK_7C);

    public static Rank RANK_8 = new Rank(RANK_8C);

    public static Rank RANK_NB = new Rank(8);

    private int Value { get; }

    #region constructors

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Rank(uint value)
        : this((int) value)
    {
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public Rank(int value)
    {
        Value = value;
        //Debug.Assert(this.Value >= -8 && this.Value <= 8);
    }

    #endregion

    #region base operators

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Rank operator +(Rank v1, Rank v2)
    {
        return new Rank(v1.Value + v2.Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Rank operator +(Rank v1, int v2)
    {
        return new Rank(v1.Value + v2);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Rank operator +(int v1, Rank v2)
    {
        return new Rank(v1 + v2.Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Rank operator -(Rank v1, Rank v2)
    {
        return new Rank(v1.Value - v2.Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Rank operator -(Rank v1, int v2)
    {
        return new Rank(v1.Value - v2);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Rank operator -(Rank v1)
    {
        return new Rank(-v1.Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Rank operator *(int v1, Rank v2)
    {
        return new Rank(v1*v2.Value);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Rank operator *(Rank v1, int v2)
    {
        return new Rank(v1.Value*v2);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static implicit operator int(Rank r)
    {
        return r.Value;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static bool operator ==(Rank v1, Rank v2)
    {
        return v1.Value == v2.Value;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static bool operator !=(Rank v1, Rank v2)
    {
        return v1.Value != v2.Value;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Rank operator ++(Rank v1)
    {
        return new Rank(v1.Value + 1);
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Rank operator --(Rank v1)
    {
        return new Rank(v1.Value - 1);
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    #endregion

    #region extended operators

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static int operator /(Rank v1, Rank v2)
    {
        return v1.Value/v2.Value;
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Rank operator /(Rank v1, int v2)
    {
        return new Rank(v1.Value/v2);
    }

    #endregion

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Rank relative_rank(Color c, Rank r)
    {
        return new Rank(r.Value ^ (c*7));
    }

#if FORCEINLINE  
	[MethodImpl(MethodImplOptions.AggressiveInlining)] 
#endif

    public static Rank relative_rank(Color c, Square s)
    {
        return relative_rank(c, Square.rank_of(s));
    }
}